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

            var categoryLinks = categoriesDoc.QuerySelectorAll(".mw-category-group li a").GetLinks();
            foreach(var categoryLink in categoryLinks )
            {
                driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + categoryLink);

                var categoryDoc = parser.ParseDocument(driver.PageSource);
                var companyKolLink = categoryDoc.QuerySelector("h3+ul")?.QuerySelectorAll("li a").GetLinks();
                foreach(var companyLink in companyKolLink ?? new string[0] )
                {
                    if (IsRedirectLink(companyLink))
                        continue;
                    driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + companyLink);

                    var companyDoc = parser.ParseDocument(driver.PageSource);
                    var guidestar = companyDoc.Body.SelectSingleNode(XQueryLinkByText("גיידסטאר:")).GetLink();
                    
                    if(guidestar == null)
                    {
                        var linksToSubCompanies = companyDoc.QuerySelectorAll(".wikitable tr td:nth-child(1) a").GetLinks();
                        foreach(var subLink in linksToSubCompanies)
                        {
                            if (!IsRedirectLink(subLink))
                                driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + subLink);
                            else
                                continue;
                            var subCompanyDoc = parser.ParseDocument(driver.PageSource);
                            var guidestarSub = companyDoc.Body.SelectSingleNode(XQueryLinkByText("גיידסטאר:")).GetLink();
                            ScrapCompany(guidestarSub, subCompanyDoc);
                        }
                    }
                    else
                    {
                        ScrapCompany(guidestar, companyDoc);
                    }


                }
            }



            void ScrapCompany(string guidestarLink, IDocument companyDoc)
            {
                var linkToSite = companyDoc.Body.SelectSingleNode(XQueryLinkByText("אתר:")).GetLink();
                var email = string.Join("",companyDoc.Body.SelectSingleNode(XQueryLinkByText("דוא\"ל:")).GetLink()?.SkipWhile(x=> x != ':').Skip(1) ?? "");
                var phone = companyDoc.QuerySelector(".phonenum")?.TextContent;
                var address = companyDoc.Body.SelectSingleNode(XQueryByText("כתובת:")).GetLink();
                var fax = companyDoc.Body.SelectSingleNode(XQueryByText("פקס:")).GetLink();
                var facebook = companyDoc.Body.SelectSingleNode(XQueryFacebookByText()).GetLink();
                var company = new Company()
                {
                    Site = linkToSite,
                    Phone = phone,
                    Email = email,
                    Address = address,
                    Facebook = facebook,
                    Fax = fax,
                    GuideStarLink = guidestarLink,

                };
                if (guidestarLink == null)
                    return;
                driver.Navigate().GoToUrl(guidestarLink);
                //.desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell
                var GSCompanyDoc = parser.ParseDocument(driver.PageSource);
                var statRows = GSCompanyDoc.QuerySelectorAll(".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell").Select(x => x.TextContent).ToArray();
                for (int i = 0; i < statRows.Length; i += 2)
                {
                    var areaAase = new AreaCase
                    {
                        Amount = int.Parse(statRows[i + 1]),
                        AreaName = statRows[i]
                    };
                    
                }//got statistics, next get contact info, if active and main area of activity
                var activeArea = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("אזור פעילות"))?.TextContent;
                var status = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText(" בהליכי מחיקה"))?.TextContent;
                var companyId = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("מספר ארגון"))?.TextContent;
                company.ActiveArea = activeArea;
                company.Status = status;
                company.CompanyId = companyId;
                writer.WriteRecord(company);
                writer.Flush();
                writer.NextRecord();
            }
        }
        static bool IsRedirectLink(string link)
        {
            return link.StartsWith("http");
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
    }

}
