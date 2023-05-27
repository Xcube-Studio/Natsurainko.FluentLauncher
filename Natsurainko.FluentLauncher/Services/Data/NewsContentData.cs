namespace Natsurainko.FluentLauncher.Services.Data;

internal class NewsContentData
{
    public NewsContentData(string imageUrl, string title, string tag, string date, string text, string readMoreUrl)
    {
        ImageUrl = imageUrl;
        Title = title;
        Tag = tag;
        Date = date;
        Text = text;
        ReadMoreUrl = readMoreUrl;
    }

    public string ImageUrl { get; }

    public string Title { get; }

    public string Tag { get; }

    public string Date { get; }

    public string Text { get; }

    public string ReadMoreUrl { get; }
}
