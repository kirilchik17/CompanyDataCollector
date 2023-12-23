using OpenQA;
using AngleSharp;
using AngleSharp.XPath;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Reflection.Metadata;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Xml.XPath;
using AngleSharp.Browser;
using System.Text;
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

            var categoryLinks = categoriesDoc.QuerySelectorAll(".mw-category-group li a").Select(x=> x.GetAttribute("href"));//Create Shortcut
            foreach(var categoryLink in categoryLinks )
            {
                driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + categoryLink);

                var categoryDoc = parser.ParseDocument(driver.PageSource);
                var companyKolLink = categoryDoc.QuerySelector("h3+ul")?.QuerySelectorAll("li a").Select(x => x.GetAttribute("href"));
                foreach(var companyLink in companyKolLink)
                {

                    driver.Navigate().GoToUrl(KOLZHUT_BASE_URL + companyLink);
                    var companyDoc = parser.ParseDocument(driver.PageSource);
                    var linkToSite = ((IElement)companyDoc.Body.SelectSingleNode("//th[text()='אתר:']/following-sibling::td[1]/a")).GetAttribute("href");//create extension

                }
            }

        }
    }
}
