using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class SkinViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly CacheSkinService _cacheSkinService;
    private readonly NotificationService _notificationService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    private readonly HttpClient _httpClient;

    public SkinViewModel(
        SettingsService settingsService,
        AccountService accountService,
        CacheSkinService cacheSkinService,
        NotificationService notificationService,
        IDialogActivationService<ContentDialogResult> dialogs,
        HttpClient httpClient)
    {
        _settingsService = settingsService;
        _cacheSkinService = cacheSkinService;
        _notificationService = notificationService;
        _dialogs = dialogs;

        ActiveAccount = accountService.ActiveAccount!;

        (this as ISettingsViewModel).InitializeSettings();

        Task.Run(LoadModel);
        _httpClient = httpClient;
    }

    [ObservableProperty]
    public partial Account ActiveAccount { get; set; }

    public ObservableElement3DCollection ModelGeometry { get; private set; } = [];

    public bool IsYggdrasilAccount => ActiveAccount.Type == AccountType.Yggdrasil;

    #region Skin 3D Model Load

    public async Task LoadModel()
    {
        try
        {
            var loader = new ObjReader();
            var object3Ds = loader.Read(Path.Combine(Package.Current.InstalledLocation.Path, $"Assets/{(await IsSlimSkin() ? "Rig_alex.obj" : "Rig_steve.obj")}"));

            #region Create Skin Texture Stream

            using var fileStream = File.Open(_cacheSkinService.GetSkinFilePath(ActiveAccount), FileMode.Open, FileAccess.Read, FileShare.Read);
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

                ModelGeometry.Clear();

                foreach (var object3D in object3Ds)
                    ModelGeometry.Add(new MeshGeometryModel3D() { Material = material, Geometry = object3D.Geometry });

                stream.Dispose();
            });
        }
        catch (Exception ex)
        {
            _notificationService.NotifyException(
                LocalizedStrings.Notifications__SkinDisplayExceptionT,
                ex,
                LocalizedStrings.Notifications__SkinDisplayExceptionD);
        }
    }

    async Task<bool> IsSlimSkin()
    {
        string requestUrl = string.Empty;
        string accessToken = string.Empty;

        if (ActiveAccount is MicrosoftAccount microsoft)
        {
            accessToken = microsoft.AccessToken;
            requestUrl = "https://api.minecraftservices.com/minecraft/profile";
        }
        else if (ActiveAccount is YggdrasilAccount yggdrasil)
        {
            accessToken = yggdrasil.AccessToken;
            requestUrl = yggdrasil.YggdrasilServerUrl
                + "/sessionserver/session/minecraft/profile/"
                + yggdrasil.Uuid.ToString("N");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();

        if (ActiveAccount is MicrosoftAccount)
        {
            var json = JsonNode.Parse(responseMessage.Content.ReadAsString())!["skins"]!
                .AsArray().Where(item => (item!["state"]?.GetValue<string>().Equals("ACTIVE")).GetValueOrDefault()).FirstOrDefault();

            if (json!["variant"]?.GetValue<string>() == "SLIM")
                return true;
        }
        else if (ActiveAccount is YggdrasilAccount)
        {
            var jsonBase64 = JsonNode.Parse(responseMessage.Content.ReadAsString())!["properties"]![0]!["value"];
            var json = JsonNode.Parse(jsonBase64!.GetValue<string>().ConvertFromBase64());

            if (json!["textures"]?["SKIN"]?["metadata"]?["model"]!.GetValue<string>() == "slim")
                return true;
        }

        return false;
    }

    #endregion

    [RelayCommand]
    async Task UploadSkin()
    {
        await _dialogs.ShowAsync("UploadSkinDialog", ActiveAccount);
        await LoadModel();
    }

    [RelayCommand]
    void NavigateToWebsite()
    {
        if (ActiveAccount is YggdrasilAccount yggdrasilAccount)
            _ = Launcher.LaunchUriAsync(new Uri("https://" + new Uri(yggdrasilAccount.YggdrasilServerUrl).Host));
        else _ = Launcher.LaunchUriAsync(new Uri("https://www.minecraft.net/msaprofile/mygames/editskin"));
    }

    #region Converters Methods

    internal string GetAccountTypeName(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    internal string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    #endregion
}
