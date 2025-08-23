using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Notification;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.WinUI;
using HelixToolkit.WinUI.CommonDX;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.System;
using static Nrk.FluentCore.Utils.PlayerTextureHelper;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class SkinViewModel : SettingsPageVM,
    ISettingsViewModel, IDisposable
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly CacheInterfaceService _cacheInterfaceService;
    private readonly INotificationService _notificationService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    private readonly long startAniTime = Stopwatch.GetTimestamp();

    private List<IAnimationUpdater> animationUpdaters = [];
    private CompositionTargetEx? compositeHelper;

    public SkinViewModel(
        SettingsService settingsService,
        AccountService accountService,
        CacheInterfaceService cacheInterfaceService,
        INotificationService notificationService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _settingsService = settingsService;
        _cacheInterfaceService = cacheInterfaceService;
        _notificationService = notificationService;
        _dialogs = dialogs;

        ActiveAccount = accountService.ActiveAccount!;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    public partial Account ActiveAccount { get; set; }

    [ObservableProperty]
    public partial SceneNodeGroupModel3D? Root { get; set; }

    [ObservableProperty]
    public partial OrthographicCamera Camera { get; set; } = new() { NearPlaneDistance = 1e-2, FarPlaneDistance = 1e4 };

    [ObservableProperty]
    public partial IEffectsManager? EffectsManager { get; set; }

    [ObservableProperty]
    public partial PlayerTextureProfile? TextureProfile { get; set; }

    public double LoadingWidth { get; set; }

    public bool IsYggdrasilAccount => ActiveAccount.Type == AccountType.Yggdrasil;

    public async Task LoadTextureProfile()
    {
        TextureProfile = await _cacheInterfaceService.RequestTextureProfileAsync(ActiveAccount);
    }

    public void RenderModel()
    {
        if (TextureProfile is null) return;

        var model = TextureProfile.GetRenderModel();

        model.Root.Attach(EffectsManager);
        model.Root.UpdateAllTransformMatrix();

        if (model.Root.TryGetBound(out var bound))
            FocusCameraToScene(bound);

        Root!.AddNode(model.Root);

        if (model.HasAnimation)
        {
            animationUpdaters = [.. model.Animations.CreateAnimationUpdaters().Values];

            foreach (var animationUpdater in animationUpdaters)
            {
                animationUpdater.RepeatMode = AnimationRepeatMode.Loop;
                animationUpdater.Reset();
            }

            compositeHelper!.Rendering += CompositeHelper_Rendering;
        }
    }

    public void RenderSkinTexture()
    {
        if (TextureProfile is null) return;
        var material = TextureProfile.CreateSkinTexture();

        foreach (var node in Root!.SceneNode.Traverse())
            if (!node.Name.Contains("cape", StringComparison.OrdinalIgnoreCase) && node is MeshNode meshNode)
                meshNode.Material = material;
    }

    public void RenderCapeTexture()
    {
        if (TextureProfile is null) return;
        var material = TextureProfile.CreateCapeTexture();

        foreach (var node in Root!.SceneNode.Traverse())
            if (node.Name.Contains("cape", StringComparison.OrdinalIgnoreCase) && node is MeshNode meshNode)
                meshNode.Material = material;
    }

    partial void OnTextureProfileChanged(PlayerTextureProfile? value)
    {
        Dispatcher.TryEnqueue(() =>
        {
            this.Dispose();

            Root = new SceneNodeGroupModel3D();
            EffectsManager = new DefaultEffectsManager();

            compositeHelper = new CompositionTargetEx();
            compositeHelper.Rendering += CompositeHelper_Rendering;

            RenderModel();
            RenderSkinTexture();
            RenderCapeTexture();
        });
    }

    [RelayCommand]
    async Task UploadSkin()
    {
        if (await _dialogs.ShowAsync("UploadSkinDialog", ActiveAccount) == ContentDialogResult.Primary)
        {
            _cacheInterfaceService.RequestTextureProfileAsync(ActiveAccount)
                .ContinueWith(t => TextureProfile = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                .Forget();
        }
    }

    [RelayCommand]
    async Task NavigateToWebsite()
    {
        string url = ActiveAccount switch
        {
            YggdrasilAccount yggdrasilAccount => "https://" + new Uri(yggdrasilAccount.YggdrasilServerUrl).Host,
            _ => "https://www.minecraft.net/msaprofile/mygames/editskin",
        };

        await Launcher.LaunchUriAsync(new Uri(url));
    }

    [RelayCommand]
    async Task OpenSkinFile()
    {
        var textureProfile = await _cacheInterfaceService.RequestTextureProfileAsync(ActiveAccount);
        string skinFilePath = textureProfile.GetSkinTexturePath(out _);

        if (!File.Exists(skinFilePath))
            return;

        ExplorerHelper.ShowAndSelectFile(skinFilePath);
    }

    private void CompositeHelper_Rendering(object? sender, Microsoft.UI.Xaml.Media.RenderingEventArgs e)
    {
        foreach (var animationUpdater in animationUpdaters)
            animationUpdater.Update(Stopwatch.GetTimestamp() - startAniTime, Stopwatch.Frequency);
    }

    private void FocusCameraToScene(BoundingBox boundingBox)
    {
        var maxSize = Math.Max(Math.Max(boundingBox.Width, boundingBox.Height), boundingBox.Depth);
        double aspect = LoadingWidth / 550;

        Camera.Position = new Vector3(90, 130, 120);
        Camera.LookDirection = new Vector3(-110, -35, -165);
        Camera.UpDirection = Vector3.UnitY;
        Camera.Width = maxSize * aspect * 1.25;
    }

    public void Dispose()
    {
        Root?.Dispose();
        EffectsManager?.Dispose();

        compositeHelper?.Rendering -= CompositeHelper_Rendering;
        compositeHelper?.Dispose();

        animationUpdaters.Clear();
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

    internal string GetActiveCapeDisplayText(PlayerTextureProfile textureProfile)
    {
        if (textureProfile?.ActiveCape is null)
            return LocalizedStrings.Settings_SkinPage__NoCape;

        return $"{textureProfile.ActiveCape.Alias} {LocalizedStrings.Settings_SkinPage__Cape}";
    }

    internal string GetSkinModelDisplayText(PlayerTextureProfile textureProfile)
    {
        if (textureProfile is null) return string.Empty;

        string variant = textureProfile.ActiveSkin?.Variant ?? PlayerTextureProfileExtensions.CalculateModel(textureProfile.Uuid);
        return variant switch
        {
            "slim" => LocalizedStrings.Settings_SkinPage__Slim,
            _ => LocalizedStrings.Settings_SkinPage__Classic
        };
    }

    internal bool CanOpenSkinFile(PlayerTextureProfile? textureProfile)
    {
        if (textureProfile is null) return false;

        textureProfile.GetSkinTexturePath(out var value);
        return !value;
    }

    #endregion
}

internal static partial class SkinViewModelNotifications
{
    [ExceptionNotification(Title = "Notifications__SkinDisplayExceptionT", Message = "Notifications__SkinDisplayExceptionD")]
    public static partial void SkinDisplayFailed(this INotificationService notificationService, Exception exception);
}