using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Storage;

class SkinCacheService
{
    private readonly LocalStorageService _localStorageService;
    private readonly Dictionary<Account, IEnumerable<SoftwareBitmapSource>> RuntimeCache = new();

    public SkinCacheService(LocalStorageService storage)
    {
        _localStorageService = storage;
    }

    public async void SetSkinHeadControlContent(Border container, Account account)
    {
        var grid = new Grid();

        await foreach (var item in GetSkinHeadSourcesAsync((int)container.ActualWidth, (int)container.ActualHeight, account))
        {
            var bitmapIcon = new Microsoft.UI.Xaml.Controls.Image
            {
                Source = item
            };

            grid.Children.Add(bitmapIcon);
        };

        container.Child = grid;
    }

    public void TryCacheSkin(Account account)
    {
        if (account == null || account.Type.Equals(AccountType.Offline))
            return;

        var authorization = new Tuple<string, string>("Bearer", account.AccessToken);
        var skinUrl = string.Empty;

        if (account is YggdrasilAccount yggdrasil)
        {
            using var responseMessage = HttpUtils.HttpGet(
                yggdrasil.YggdrasilServerUrl +
                "/sessionserver/session/minecraft/profile/" +
                yggdrasil.Uuid.ToString("N").ToLower()
                , authorization);

            var jsonBase64 = JsonNode.Parse(responseMessage.Content.ReadAsString())["properties"][0]["value"];
            var json = JsonNode.Parse(jsonBase64.GetValue<string>().ConvertFromBase64());

            skinUrl = json["textures"]?["SKIN"]?["url"]?.GetValue<string>();
        }

        if (account is MicrosoftAccount microsoft)
        {
            using var responseMessage = HttpUtils.HttpGet("https://api.minecraftservices.com/minecraft/profile", authorization);
            var json = JsonNode.Parse(responseMessage.Content.ReadAsString())["skins"]
                .AsArray().Where(item => (item["state"]?.GetValue<string>().Equals("ACTIVE")).GetValueOrDefault()).FirstOrDefault();

            skinUrl = json?["url"]?.GetValue<string>();
        }

        if (string.IsNullOrEmpty(skinUrl)) return;

        var skinFilePath = GetSkinFilePath(account);
        var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
        {
            AbsolutePath = skinFilePath,
            Url = skinUrl,
        });

        downloadTask.Wait();

        if (downloadTask.Result.IsFaulted) return;

        CreateHeadsFile(skinFilePath);
    }

    private async IAsyncEnumerable<SoftwareBitmapSource> GetSkinHeadSourcesAsync(int width, int height, Account account)
    {
        /* //TODO: 运行时缓存
        if (RuntimeCache.ContainsKey(account))
        {
            foreach (var item in RuntimeCache[account])
                yield return item;

            yield break;
        }*/

        var sources = new List<SoftwareBitmapSource>();
        var dir = Path.Combine(_localStorageService.GetDirectory("cache-skins").FullName, $"{account.Type}-{account.Uuid}");

        string[] paths = new string[]
        {
            Path.Combine(dir, "background.png"),
            Path.Combine(dir, "foreground.png")
        };

        for (int i = 0; i < paths.Length; i++)
        {
            var path = paths[i];

            if (i == 0 && !File.Exists(path))
                path = (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Icons/steve.png"))).Path;

            if (!File.Exists(path))
            {
                yield return null;
                sources.Add(null);
                continue;
            }

            using var fileStream = File.OpenRead(path);
            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

            var transform = new BitmapTransform
            {
                InterpolationMode = BitmapInterpolationMode.NearestNeighbor,
                ScaledWidth = (uint)width,
                ScaledHeight = (uint)height
            };

            using var bmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bmp);

            yield return source;
            sources.Add(source);
        }

        if (!RuntimeCache.ContainsKey(account))
            RuntimeCache.Add(account, sources);
    }

    private string GetSkinFilePath(Account account)
    {
        var dir = Path.Combine(_localStorageService.GetDirectory("cache-skins").FullName, $"{account.Type}-{account.Uuid}");

        return Path.Combine(dir, "skin.png");
    }

    private static void CreateHeadsFile(string skinFile)
    {
        using var originImage = System.Drawing.Image.FromFile(skinFile);

        var rects = new (string, Rectangle)[]
        {
            ("background", new Rectangle(8, 8, 8, 8)),
            ("foreground", new Rectangle(40, 8, 8, 8))
        };

        foreach (var value in rects)
        {
            var rectangle = value.Item2;
            var fileName = value.Item1;

            using var resultBitmap = new Bitmap(8, 8);
            using var graphics = Graphics.FromImage(resultBitmap);

            graphics.DrawImage(originImage, new Rectangle(0, 0, 8, 8), rectangle, GraphicsUnit.Pixel);

            bool isTransparent = true;

            for (int i = 0; i < resultBitmap.Width; i++)
                for (int j = 0; j < resultBitmap.Height; j++)
                    if (!resultBitmap.GetPixel(i, j).Equals(System.Windows.Media.Colors.Transparent))
                        isTransparent = false;

            if (!isTransparent) resultBitmap.Save(skinFile.Replace("skin.png", fileName + ".png"));
        }
    }
}
