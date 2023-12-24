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
using CompanyDataCollector.Shared;
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
                foreach(var companyLink in companyKolLink)
                {
                    

                    driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + companyLink);
                    var companyDoc = parser.ParseDocument(driver.PageSource);

                    var linkToSite = companyDoc.Body.SelectSingleNode(XQueryLinkByText("אתר:")).GetLink();
                    var email = companyDoc.Body.SelectSingleNode(XQueryLinkByText("דוא\"ל:")).GetLink();
                    var guidestar = companyDoc.Body.SelectSingleNode(XQueryLinkByText("גיידסטאר:")).GetLink();
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
                        GuideStarLink = guidestar,
                        
                    };
                    driver.Navigate().GoToUrl(guidestar);
                    //.desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell
                    var GSCompanyDoc = parser.ParseDocument(driver.PageSource); 
                    var statRows = GSCompanyDoc.QuerySelectorAll(".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell").Select(x=> x.TextContent).ToArray();
                    company.ActivityStatistics = new ActivityStatistics() { AreaCases = new List<AreaCase>()};
                    for (int i = 0; i < statRows.Length; i+= 2)
                    {
                        var areaAase = new AreaCase
                        {
                            Amount = int.Parse(statRows[i + 1]),
                            AreaName = statRows[i]
                        };
                        company.ActivityStatistics.AreaCases.Add(areaAase);
                        company.ActivityStatistics.TotalCases += areaAase.Amount;
                    }//got statistics, next get contact info, if active and main area of activity
                    var activeArea = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText("אזור פעילות")).TextContent;
                    var status = GSCompanyDoc.Body.SelectSingleNode(XQueryGuidestarByText(" בהליכי מחיקה")).TextContent;
                    company.ActiveArea = activeArea;
                    company.Status = status;

                }
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
    }

}
