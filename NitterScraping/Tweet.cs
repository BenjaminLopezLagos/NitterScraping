namespace NitterScraping;
    
public class Tweet
{
    public string User { get; }
    public string Content { get; }
    public string Date { get; }

    public Tweet(string user, string content, string date)
    {
        User = user;
        Content = content;
        Date = date;
    }
}