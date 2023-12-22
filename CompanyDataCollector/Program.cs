using OpenQA;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace CompanyDataCollector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebDriver driver = new ChromeDriver(new ChromeOptions
            {
                BinaryLocation = "C:\\Users\\Asus\\source\\repos\\chromeDriver\\chrome.exe"
            });
            driver.Navigate().GoToUrl("https://www.kolzchut.org.il/he/קטגוריה:פורטלים");
            //li[data-raofz="18"]
        }
    }
}
