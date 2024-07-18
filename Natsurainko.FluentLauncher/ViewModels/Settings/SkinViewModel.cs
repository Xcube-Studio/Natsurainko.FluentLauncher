using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.Settings.Mvvm;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class SkinViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly AccountService _accountService;
    private readonly CacheSkinService _cacheSkinService;

    [ObservableProperty]
    private Account activeAccount;

    public ObservableElement3DCollection ModelGeometry { get; private set; } = new ObservableElement3DCollection();

    public SkinViewModel(
        SettingsService settingsService,
        AccountService accountService,
        CacheSkinService cacheSkinService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        _cacheSkinService = cacheSkinService;

        ActiveAccount = accountService.ActiveAccount!;

        (this as ISettingsViewModel).InitializeSettings();

        Task.Run(LoadModel);
    }

    private async void LoadModel()
    {
        var loader = new ObjReader();
        var object3Ds = loader.Read(Path.Combine(Package.Current.InstalledLocation.Path, $"Assets/{(await IsSlimSkin() ? "Rig_alex.obj" : "Rig_steve.obj")}"));

        #region Create Skin Texture Stream

        using var fileStream = File.OpenRead(_cacheSkinService.GetSkinFilePath(ActiveAccount));
        using var randomAccessStream = fileStream.AsRandomAccessStream();

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

        var transform = new BitmapTransform
        {
            InterpolationMode = BitmapInterpolationMode.NearestNeighbor,
            ScaledWidth = (uint)1024,
            ScaledHeight = (uint)1024
        };
        InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();

        using var bmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

        encoder.SetSoftwareBitmap(bmp);
        await encoder.FlushAsync();

        #endregion

        App.DispatcherQueue.TryEnqueue(() =>
        {
            var material = new DiffuseMaterial();
            material.DiffuseMap = TextureModel.Create(stream.AsStreamForRead());

            foreach (var object3D in object3Ds)
                ModelGeometry.Add(new MeshGeometryModel3D() { Material = material, Geometry = object3D.Geometry });
        });
    }

    private async Task<bool> IsSlimSkin()
    {
        var authorization = new Tuple<string, string>("Bearer", ActiveAccount.AccessToken);
        var skinUrl = string.Empty;

        if (ActiveAccount is YggdrasilAccount yggdrasil)
        {
            using var responseMessage = HttpUtils.HttpGet(
                yggdrasil.YggdrasilServerUrl +
                "/sessionserver/session/minecraft/profile/" +
                yggdrasil.Uuid.ToString("N").ToLower()
                , authorization);

            var jsonBase64 = JsonNode.Parse(responseMessage.Content.ReadAsString())["properties"][0]["value"];
            var json = JsonNode.Parse(jsonBase64.GetValue<string>().ConvertFromBase64());

            if (json["textures"]?["SKIN"]?["metadata"]?["model"].GetValue<string>() == "slim")
                return true;
        }

        if (ActiveAccount is MicrosoftAccount microsoft)
        {
            using var responseMessage = HttpUtils.HttpGet("https://api.minecraftservices.com/minecraft/profile", authorization);
            var json = JsonNode.Parse(responseMessage.Content.ReadAsString())["skins"]
                .AsArray().Where(item => (item["state"]?.GetValue<string>().Equals("ACTIVE")).GetValueOrDefault()).FirstOrDefault();

            if (json["variant"]?.GetValue<string>() == "SLIM")
                return true;
        }

        return false;
    }

}
