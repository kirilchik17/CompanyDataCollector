using OpenQA;
using AngleSharp;
using AngleSharp.XPath;
using AngleSharpExtensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Reflection.Metadata;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Xml.XPath;
using AngleSharp.Browser;
using System.Text;
using CsvHelper;
using CompanyDataCollector.Shared;
using System.Numerics;
using OpenQA.Selenium.Support.UI;
using System.Linq;
namespace CompanyDataCollector
{
    internal class Program
    {
        const string KOLZHUT_BASE_URL = "https://www.kolzchut.org.il";
        static void Main(string[] args)
        {
            WebDriver driver = new ChromeDriver(new ChromeOptions
            {
                BinaryLocation = "C:\\Users\\Asus\\source\\repos\\chromeDriver\\chrome.exe"
            });
            var filepath = $"./output/outputCompanies{DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year}.csv";
            Directory.CreateDirectory("./output");
            File.Create(filepath).Close();
            using var streamWriter = new StreamWriter(File.OpenWrite(filepath));
            using var writer = new CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture);
            writer.WriteHeader<Company>();

            writer.Flush();
            writer.NextRecord();

            driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + "/he/קטגוריה:פורטלים");
            HtmlParser parser = new HtmlParser();
            

            //li[data-raofz="18"]
            var categoriesDoc = parser.ParseDocument(driver.PageSource);

            var categoryLinks = categoriesDoc.QuerySelectorAll(".mw-category-group li a");
            foreach(var categoryLink in categoryLinks )
            {
                var speciality = categoryLink.TextContent;
                driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + categoryLink.GetLink());

                var categoryDoc = parser.ParseDocument(driver.PageSource);
                var companyKolLink = categoryDoc.QuerySelector("h3+ul")?.QuerySelectorAll("li a").GetLinks();
                var categoryGovLinks = categoryDoc.QuerySelectorAll(".kz-help-table .plainlinks .help-name a").GetLinks();

                foreach (var companyLink in companyKolLink ?? new string[0] )
                {

                    if (IsRedirectLink(companyLink))
                        continue;
                    driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + companyLink);
                    var companyDoc = parser.ParseDocument(driver.PageSource);
                    var guidestar = companyDoc.Body.SelectSingleNode(XQueryLinkByText("גיידסטאר:")).GetLink();


                    var linksToSubCompanies = companyDoc.QuerySelectorAll(".wikitable tr td:nth-child(1) a").GetLinks();
                    if (linksToSubCompanies == null || linksToSubCompanies.Count == 0)
                    {
                        ScrapCompany(guidestar, speciality, companyDoc);
                        continue;
                    }
                    
                    foreach (var subLink in linksToSubCompanies)
                    {
                        if (!IsRedirectLink(subLink))
                            driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + subLink);
                        else
                            continue;
                        var subCompanyDoc = parser.ParseDocument(driver.PageSource);
                        var guidestarelem = subCompanyDoc.Body.SelectSingleNode("//a[text()='הארגון בגיידסטאר']");
                        
                        var linksToSub2Companies = subCompanyDoc.QuerySelectorAll("tbody tr td:nth-child(1) a:not([class])").GetLinks();
                        if (linksToSub2Companies == null || linksToSub2Companies.Count == 0)
                        {
                            ScrapCompany(guidestarelem?.GetLink(), speciality, companyDoc);
                            continue;
                        }

                        foreach (var sub in linksToSub2Companies)
                        {
                            if (!IsRedirectLink(subLink))
                                driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + subLink);
                            else
                                continue;
                            using var sub2CompanyDoc = parser.ParseDocument(driver.PageSource);
                            var guidestar2 = sub2CompanyDoc.Body.SelectSingleNode("//a[text()='הארגון בגיידסטאר']").GetLink();
                            var linksToSub3Companies = subCompanyDoc.QuerySelectorAll("tbody tr td:nth-child(1) a:not([class])").GetLinks();
                            if (linksToSub3Companies == null || linksToSub3Companies.Count == 0)
                            {
                                ScrapCompany(guidestarelem?.GetLink(), speciality, sub2CompanyDoc);
                                continue;
                            }

                            foreach (var sub3 in linksToSub3Companies)
                            {
                                if (!IsRedirectLink(sub3))
                                    driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + sub3);
                                else
                                    continue;
                                using var sub3CompanyDoc = parser.ParseDocument(driver.PageSource);
                                var guidestar3 = sub3CompanyDoc.Body.SelectSingleNode("//a[text()='הארגון בגיידסטאר']").GetLink();
                                ScrapCompany(guidestar3, speciality, sub3CompanyDoc);

                            }
                        }                                               
                    }//build in recursion
                    //%D7%90%D7%A8%D7%92%D7%95%D7%A0%D7%99_%D7%A1%D7%99%D7%95%D7%A2


                }
            }



            void ScrapCompany(string guidestarLink, string category, IDocument companyDoc)
            {
                try
                {

                    var linkToSite = companyDoc.Body.SelectSingleNode(XQueryLinkByText("אתר:")).GetLink();
                    var email = string.Join("",companyDoc.Body.SelectSingleNode(XQueryLinkByText("דוא\"ל:")).GetLink()?.SkipWhile(x=> x != ':').Skip(1) ?? "");
                    var phone = companyDoc.QuerySelector(".phonenum")?.TextContent;
                    var areaOfExpertiseKolzhut = companyDoc.QuerySelector("#תחומי_פעילות")?.Parent?.NextSibling?.ChildNodes.GetTexts();
                    var address = companyDoc.Body.SelectSingleNode(XQueryByText("כתובת:"))?.TextContent;
                    var fax = companyDoc.Body.SelectSingleNode(XQueryByText("פקס:"))?.TextContent;
                    var facebook = companyDoc.Body.SelectSingleNode(XQueryFacebookByText())?.GetLink();
                    var name = companyDoc.QuerySelector("#firstHeading")?.TextContent;

                    var price = companyDoc.Body.SelectSingleNode(XQueryByText("עלות:"))?.TextContent;
                    var FirstPrioritySpec = companyDoc.Body.SelectSingleNode(XQueryByText("תחום ראשי:"))?.TextContent;
                    var SecondPrioritySpec = companyDoc.Body.SelectSingleNode(XQueryByText("תחום משני:"))?.TextContent;
                    var parentOrganization = companyDoc.Body.SelectSingleNode(XQueryByText("ארגון מפעיל:"))?.TextContent;


                    var company = new Company()
                    {             
                        Name = name,
                        Category = category,
                        Site = linkToSite,
                        Phone = phone,
                        Email = email,
                        Address = address,
                        Facebook = facebook,
                        Fax = fax,
                        GuideStarLink = guidestarLink,
                        Price = price,
                        PrimarySpeciality = FirstPrioritySpec,
                        SecondarySpeciality = SecondPrioritySpec,
                        ParentOrganization = parentOrganization

                    };
                    if (guidestarLink == null)
                    {
                        company.AreaOfExpertise = string.Join(", ", areaOfExpertiseKolzhut ?? new string[0]);
                        writer.WriteRecord(company);
                        writer.Flush();
                        writer.NextRecord();
                        return;
                    }
                    driver.Navigate().GoToUrl(guidestarLink);

                    if (!WaitUntilElementExists(driver, ".h3-vertical-bar"))
                        return;
                    WaitUntilElementExists(driver, ".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell", 10);
                    var GSCompanyDoc = parser.ParseDocument(driver.PageSource);

                    var b = GSCompanyDoc.QuerySelectorAll(".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell").ToArray();
                    var statRows = b?.Select(x => x.TextContent).ToArray();
                    var s = GSCompanyDoc.QuerySelectorAll("i.h3-vertical-bar");
                    var targetGroups = s.FirstOrDefault(x=> x.NextSibling?.TextContent == "קהל יעד")?.Parent?.NextSibling?.TextContent;
                    var spetialities = GSCompanyDoc.Body.SelectNodes("//h3[text()=\"תחום פעילות\"]/following-sibling::div").GetTexts().Distinct();

                    for (int i = 0; i < statRows.Length; i += 2)
                    {//div[contains(.,'קהל יעד') ]
                        var areaAase = new AreaCase
                        {
                            Amount = ParseInt(statRows[i + 1]),
                            AreaName = statRows[i]
                        };
                        company.AddCase(areaAase);
                    
                    }//got statistics, next get contact info, if active and main area of activity
                    var activeArea = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("אזור פעילות"))?.TextContent;
                    var status = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("סטטוס"))?.TextContent;
                    var companyId = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("מספר ארגון"))?.TextContent;
                    company.ActiveArea = activeArea;
                    company.Status = status;
                    company.TargetGroups = targetGroups;
                    company.CompanyId = companyId;
                    company.AreaOfExpertise = string.Join(", ", spetialities.Concat(areaOfExpertiseKolzhut?? new string[0]));
                    writer.WriteRecord(company);
                    writer.Flush();
                    writer.NextRecord();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        static bool IsRedirectLink(string link)
        {
            return link.StartsWith("http");
        }
        public static bool WaitUntilElementExists(IWebDriver driver , string selector, int timeout = 15)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(x=> x.FindElement(By.CssSelector(selector))) != null;
            }
            catch (Exception)
            {
                Console.WriteLine("Element was not found in current context page.");
                return false;
            }
        }
        static string XQueryLinkByText(string companyDataString)
        {
            return $"//th[text()='{companyDataString}']/following-sibling::td[1]/a";
        }

        static string XQueryGuidestarByText(string companyDataString)
        {
            return $"//span[text()='{companyDataString}']/parent::div/span[@class='label-value-value ng-star-inserted']";
        }

        static string XQueryByText(string companyDataString)
        {
            return $"//th[text()='{companyDataString}']/following-sibling::td[1]";
        }
        static string XQueryFacebookByText()
        {
            return $"//th[text()=' פייסבוק:']//parent::th/following-sibling::td[1]/a";
        }
        static int ParseInt(string text)
        {
            var str = string.Join("", text.Where(char.IsDigit));
           return int.Parse(str);
        }
    }

}
