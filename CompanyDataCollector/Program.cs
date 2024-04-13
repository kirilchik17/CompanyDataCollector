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
using CompanyDataCollector.Logic;
namespace CompanyDataCollector
{
    internal class Program
    {
        const string KOLZHUT_BASE_URL = "https://www.kolzchut.org.il";
        static void Main(string[] args)
        {
            WebDriver driver = new ChromeDriver(new ChromeOptions
            {
                BinaryLocation = "Your Location of the chrome.exe"
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
                    var guidestar = companyDoc.Body.SelectSingleNode(CollectorManager.XQueryLinkByText("גיידסטאר:")).GetLink();


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
                    var company = CollectorManager.ScrapKolzhut(driver, companyDoc, guidestarLink, category);
                    if (guidestarLink == null)
                    {
                        writer.WriteRecord(company);
                        writer.Flush();
                        writer.NextRecord();
                        return;
                    }
                    driver.Navigate().GoToUrl(guidestarLink);

                    company = CollectorManager.ScrapGuideStar(driver, parser, company);
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
        
        
    }

}
