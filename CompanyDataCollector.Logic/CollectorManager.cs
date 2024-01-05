using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using AngleSharpExtensions;
using CompanyDataCollector.Shared;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CompanyDataCollector.Logic
{
    public class CollectorManager
    {
        const string KOLZHUT_BASE_URL = "https://www.kolzchut.org.il";
        public static Company ScrapGuideStarScrapGuideStar(IWebDriver driver, IHtmlParser parser, Company company)
        {
            if (!WaitUntilElementExists(driver, ".h3-vertical-bar"))
                return company;
            WaitUntilElementExists(driver, ".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell", 10);
            var GSCompanyDoc = parser.ParseDocument(driver.PageSource);

            var b = GSCompanyDoc.QuerySelectorAll(".desktop-show .ng-star-inserted .malkar-info-chart-pie .chart-pie-table-row .chart-pie-table-cell").ToArray();
            var statRows = b?.Select(x => x.TextContent).ToArray();
            var s = GSCompanyDoc.QuerySelectorAll("i.h3-vertical-bar");
            var targetGroups = s.FirstOrDefault(x => x.NextSibling?.TextContent == "קהל יעד")?.Parent?.NextSibling?.TextContent;
            var spetialities = GSCompanyDoc.Body.SelectNodes("//h3[text()=\"תחום פעילות\"]/following-sibling::div").GetTexts().Distinct();
            var mailto = GSCompanyDoc.Body.SelectSingleNode("//div[@class='malkar-contact-info']/a[contains(@href,'mailto:')]").GetLink();

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
            company.Email ??= mailto;
            company.AreaOfExpertise += string.Join(", ", spetialities);
            return company;
        }

        public static Company ScrapKolzhut(IWebDriver driver, IDocument companyDoc, string guidestarLink, string category)
        {
            var linkToSite = companyDoc.Body.SelectSingleNode(XQueryLinkByText("אתר:")).GetLink();
            var email = string.Join("", companyDoc.Body.SelectSingleNode(XQueryLinkByText("דוא\"ל:")).GetLink()?.SkipWhile(x => x != ':').Skip(1) ?? "");
            var phone = companyDoc.QuerySelector(".phonenum")?.TextContent;
            var areaOfExpertiseKolzhut = companyDoc.QuerySelector("#תחומי_פעילות")?.Parent?.NextSibling?.ChildNodes.GetTexts();
            var address = companyDoc.Body.SelectSingleNode(XQueryByText("כתובת:"))?.TextContent;
            var fax = companyDoc.Body.SelectSingleNode(XQueryByText("פקס:"))?.TextContent;
            var facebook = companyDoc.Body.SelectSingleNode(XQueryFacebookByText())?.GetLink();
            var name = companyDoc.QuerySelector("#firstHeading")?.TextContent;
            var imageURL = companyDoc.QuerySelector(".infobox-logo.text-center img")?.GetAttribute("src");

            var price = companyDoc.Body.SelectSingleNode(XQueryByText("עלות:"))?.TextContent;
            var FirstPrioritySpec = companyDoc.Body.SelectSingleNode(XQueryByText("תחום ראשי:"))?.TextContent;
            var SecondPrioritySpec = companyDoc.Body.SelectSingleNode(XQueryByText("תחום משני:"))?.TextContent;
            var parentOrganization = companyDoc.Body.SelectSingleNode(XQueryByText("ארגון מפעיל:"))?.TextContent;

            if (imageURL != null && imageURL.StartsWith("/"))
                imageURL = KOLZHUT_BASE_URL + imageURL;
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
                ParentOrganization = parentOrganization,
                ImgLink = imageURL,
                AreaOfExpertise = string.Join(", ", areaOfExpertiseKolzhut ?? new string[0])

            };
            return company;
        }
        public static bool WaitUntilElementExists(IWebDriver driver, string selector, int timeout = 15)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(x => x.FindElement(By.CssSelector(selector))) != null;
            }
            catch (Exception)
            {
                Console.WriteLine("Element was not found in current context page.");
                return false;
            }
        }
        public static string XQueryLinkByText(string companyDataString)
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
        public static int ParseInt(string text)
        {
            var str = string.Join("", text.Where(char.IsDigit));
            return int.Parse(str);
        }
    }
}
