// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Globalization;
using CsvHelper;
using NitterScraping;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

Console.WriteLine("Hello, World!");
var driverOptions = new EdgeOptions();
driverOptions.AddArguments("headless");
var driver = new EdgeDriver(driverOptions);
var waiter = new WebDriverWait(driver: driver, TimeSpan.FromSeconds(10));

driver.Navigate().GoToUrl("https://nitter.net/settings");
driver.FindElement(By.CssSelector("label:nth-of-type(1) span")).Click();
driver.FindElement(By.CssSelector("button.pref-submit")).Click();
driver.Navigate().GoToUrl("https://nitter.net/search?f=tweets&q=elon+musk");
//driver.Manage().Window.Maximize();
IReadOnlyCollection<IWebElement>? nodes = null;
while (true)
{
    nodes = driver.FindElements(By.CssSelector("div.tweet-body"));
    var js = (IJavaScriptExecutor)driver;
    js.ExecuteScript("window.scrollBy(0, 1000)");
    Thread.Sleep(3000);
    var count = nodes.Count;
    if (count > 1000)
    {
        break;
    }
    Console.WriteLine(count);
}

var tweets = new List<Tweet>(nodes.Count);
foreach (var node in nodes)
{
    var username = node.FindElement(By.CssSelector("div.tweet-header")).
        FindElement(By.CssSelector("div.tweet-name-row")).
        FindElement(By.CssSelector("div.fullname-and-username")).
        FindElement(By.CssSelector("a.username")).
        GetAttribute("title");;
    var content = node.FindElement(By.CssSelector("div.tweet-content.media-body")).Text;
    var date = node.
        FindElement(By.CssSelector("div.tweet-header")).
        FindElement(By.CssSelector("div.tweet-name-row")).
        FindElement(By.CssSelector("span.tweet-date")).
        FindElement(By.CssSelector("a")).
        GetAttribute("title");
    tweets.Add(new Tweet(username, content, date));
    Console.WriteLine($"{username}\n{content}\n{date}\n\n");
}

using var writer = new StreamWriter("D:\\output.csv");
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
// populating the CSV file
csv.WriteRecords(tweets);
csv.Flush();