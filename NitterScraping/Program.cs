// See https://aka.ms/new-console-template for more information
using Microsoft.Data.Analysis;
using System.Collections;
using System.Globalization;
using CsvHelper;
using NitterScraping;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

var startDate = new DateTime(2022, 4, 5);
var dates = new List<DateTime>();
for (var dt = startDate; dt <= DateTime.Now; dt = dt.AddDays(1))
{
    dates.Add(dt);
}


Console.WriteLine("Hello, World!");
var driverOptions = new EdgeOptions();
driverOptions.AddArguments("headless");
var driver = new EdgeDriver(driverOptions);
var waiter = new WebDriverWait(driver: driver, TimeSpan.FromSeconds(10));
driver.Navigate().GoToUrl("https://nitter.net/settings");
driver.FindElement(By.CssSelector("label:nth-of-type(1) span")).Click();
driver.FindElement(By.CssSelector("button.pref-submit")).Click();
foreach (var date in dates)
{
    var currentDate = $"{date:yyyy-MM-dd}";
    var followingDate = $"{date.AddDays(1):yyyy-MM-dd}";
    driver.Navigate().GoToUrl($"https://nitter.net/search?f=tweets&q=elon+musk&e-nativeretweets=on&e-media=on&e-videos=on&e-news=on&e-verified=on&e-native_video=on&e-replies=on&e-images=on&e-pro_video=on&since={currentDate}&until={followingDate}&near=");
    IReadOnlyCollection<IWebElement>? nodes = null;
    var repeatedNodesCheck = 0;
    var previousNodeCount = 0;
    while (true)
    {
        nodes = driver.FindElements(By.CssSelector("div.tweet-body"));
        var js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("window.scrollBy(0, 1000)");
        Thread.Sleep(3000);
        var count = nodes.Count;
        
        if (previousNodeCount == count)
        {
            repeatedNodesCheck++;
        }
        else
        {
            repeatedNodesCheck = 0;
        }
        
        if (count > 200 || repeatedNodesCheck > 5) { break; }
        Console.WriteLine($"{count} --> repeated by:{repeatedNodesCheck} times.");
        previousNodeCount = count;
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
        var tweetDate = node.
            FindElement(By.CssSelector("div.tweet-header")).
            FindElement(By.CssSelector("div.tweet-name-row")).
            FindElement(By.CssSelector("span.tweet-date")).
            FindElement(By.CssSelector("a")).
            GetAttribute("title");
        tweets.Add(new Tweet(username, content, tweetDate));
        Console.WriteLine($"{username}\n{content}\n{tweetDate}\n\n");
    }

    using var writer = new StreamWriter($"D:\\scraped tweets\\{date:yyyy-MM-dd}.csv");
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    // populating the CSV file
    csv.WriteRecords(tweets);
    csv.Flush();
}
//driver.Manage().Window.Maximize();
