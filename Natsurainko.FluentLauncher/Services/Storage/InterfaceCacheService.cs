using Natsurainko.FluentLauncher.Classes.Data.UI;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;

namespace Natsurainko.FluentLauncher.Services.Storage;

internal class InterfaceCacheService
{
    private readonly LocalStorageService _localStorageService;

    public InterfaceCacheService(LocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public string GetLocalFileOfInterface(string url, bool autoRefresh = false)
    {
        var localFile = GetFilePathOfInterface(url);
        bool fetchFailed = false;

        if (!File.Exists(localFile))
        {
            HttpUtils.DownloadElementAsync(new DownloadElement
            {
                AbsolutePath = localFile,
                Url = url
            }, new DownloadSetting
            {
                EnableLargeFileMultiPartDownload = false
            }).ContinueWith(task =>
            {
                if (task.IsFaulted || task.Result.IsFaulted)
                    fetchFailed = true;
            }).Wait();
        }

        if (fetchFailed)
            return string.Empty;

        if (autoRefresh)
        {
            HttpUtils.DownloadElementAsync(new DownloadElement
            {
                AbsolutePath = localFile,
                Url = url
            }).ContinueWith(task =>
            {

            });
        }

        return localFile;
    }

    public string GetLocalFileOfInterface(string fileFullPath, Func<bool> fetchAction, bool autoRefresh = false)
    {
        bool fetchFailed = !File.Exists(fileFullPath) && fetchAction();

        if (fetchFailed) return string.Empty; 
        if (autoRefresh) Task.Run(fetchAction);

        return fileFullPath;
    }

    private string GetFilePathOfInterface(string url)
    {
        var uri = new Uri(url);
        return Path.Combine(
            _localStorageService.GetDirectory("cache-interfaces").FullName,
            uri.Host,
            HttpUtility.UrlDecode(uri.Segments.Last().Replace(':', '-')));
    }

    public Task<NewsData[]> FetchNews() => Task.Run(() =>
    {
        var localFile = GetLocalFileOfInterface("https://launchercontent.mojang.com/news.json", true);
        if (string.IsNullOrEmpty(localFile)) throw new ArgumentNullException(nameof(localFile));

        return JsonNode.Parse(File.ReadAllText(localFile))["entries"].AsArray()
            .Select(x =>
            {
                var contentData = x.Deserialize<NewsData>();
                contentData.ImageUrl = $"https://launchercontent.mojang.com{x["newsPageImage"]["url"].GetValue<string>()}";
                return contentData;
            }).ToArray();
    });

    public Task<PublishData[]> FetchMinecraftPublishes() => Task.Run(() =>
    {
        PublishData ParseJsonNode(JsonNode jsonNode)
        {
            var title = jsonNode["default_tile"]["title"].GetValue<string>();

            var data = new PublishData
            {
                Date = jsonNode["publish_date"].GetValue<string>(),
                ReadMoreUrl = "https://www.minecraft.net" + jsonNode["article_url"].GetValue<string>(),
                Title = title,
                Version = title.ReplaceFromDictionary(new()
                {
                    { " Release Candidate ", "-rc" },
                    { " Pre-Release ", "-pre" },
                }).Split(' ').Last()
            };

            var content = GetContent(data.ReadMoreUrl, 
                Path.Combine(
                    _localStorageService.GetDirectory("cache-interfaces").FullName, 
                    "minecraft.net", 
                    HttpUtility.UrlDecode(new Uri(data.ReadMoreUrl).Segments.Last().Replace(':', '-')) + ".html"));

            var skip_1 = "<!--/* Display Article Hero Image-->";
            var skip_2 = "<img src=\"";

            string str = string.Concat(content.Skip(content.IndexOf(skip_1) + skip_1.Length));
            var fisrt = string.Concat(str.Skip(str.IndexOf(skip_2) + skip_2.Length));
            var second = fisrt[..fisrt.IndexOf('\"')];

            data.ImageUrl = "https://www.minecraft.net" + second;

            return data;
        }

        string GetContent(string url, string filePath)
        {
            filePath = GetLocalFileOfInterface(filePath, () => 
            {
                bool fetchFailed = false;

                Task.Run(async () =>
                {
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                    requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    requestMessage.Headers.Connection.Add("keep-alive");

                    using var responseMessage = await HttpUtils.HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                    responseMessage.EnsureSuccessStatusCode();

                    var parentFolder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(parentFolder)) Directory.CreateDirectory(parentFolder);

                    using var stream = await responseMessage.Content.ReadAsStreamAsync();
                    using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);

                    using var fileStream = File.Create(filePath);
                    using var rentMemory = HttpUtils.MemoryPool.Rent(1024);

                    int readMemory = 0;

                    while ((readMemory = await gZipStream.ReadAsync(rentMemory.Memory)) > 0)
                        await fileStream.WriteAsync(rentMemory.Memory[..readMemory]);

                }).ContinueWith(task => fetchFailed = task.IsFaulted).Wait();

                return fetchFailed;

            }, true);

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            return File.ReadAllText(filePath);
        }

        var content = GetContent(
            "https://www.minecraft.net/content/minecraft-net/_jcr_content.articles.grid?tileselection=auto&pageSize=50&tagsPath=minecraft:stockholm/minecraft",
            Path.Combine(_localStorageService.GetDirectory("cache-interfaces").FullName, "minecraft.net", "articles.grid.json"));

        var articles = new List<JsonNode>();
        var release = new List<JsonNode>();

        foreach (var item in JsonNode.Parse(content)["article_grid"].AsArray())
        {
            var sub_header = item["default_tile"]["sub_header"].GetValue<string>();
            var title = item["default_tile"]["title"].GetValue<string>();

            if (title.StartsWith("Minecraft"))
            {
                if (sub_header.StartsWith("A Minecraft Java"))
                    articles.Add(item);
                else if (sub_header.StartsWith("Minecraft Java") && sub_header.EndsWith("Released"))
                    release.Add(item);
            }
        }

        return new PublishData[]
        {
            ParseJsonNode(release.Any() ? release.First() : articles[1]),
            ParseJsonNode(articles[0])
        };
    });
}
