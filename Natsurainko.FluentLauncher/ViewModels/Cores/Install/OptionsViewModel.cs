using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Cores.Install;
using Nrk.FluentCore.Resources;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Install;

internal partial class OptionsViewModel : WizardViewModelBase
{
    private readonly CurseForgeClient curseForgeClient = App.GetService<CurseForgeClient>();
    private readonly ModrinthClient modrinthClient = App.GetService<ModrinthClient>();
    public readonly InstanceInstallConfig _installConfig;

    public OptionsViewModel(InstanceInstallConfig instanceInstallConfig)
    {
        XamlPageType = typeof(OptionsPage);

        _installConfig = instanceInstallConfig;
        Init();
    }

    #region Properties
    public override bool CanNext => true;

    public override bool CanPrevious => true;

    [ObservableProperty]
    private bool fabricApiAvailable;

    [ObservableProperty]
    private bool optiFabricAvailable;

    [ObservableProperty]
    private bool enabledFabricApi;

    [ObservableProperty]
    private bool enabledOptiFabric;

    [ObservableProperty]
    private bool loadingFabricApi = true;

    [ObservableProperty]
    private ModrinthFile fabricApi;

    [ObservableProperty]
    private bool loadingOptiFabric = true;

    [ObservableProperty]
    private CurseForgeFile optiFabric;
    #endregion

    public void Init()
    {
        string vanillaId = _installConfig.ManifestItem.Id;

        Task.Run(async () =>
        {
            foreach (var item in await modrinthClient.GetProjectVersionsAsync("P7dR8mSH"))
            {
                if (item.McVersion.Equals(vanillaId))
                {
                    App.DispatcherQueue.TryEnqueue(() =>
                    {
                        FabricApi = item;
                        LoadingFabricApi = false;
                    });

                    break;
                }
            };

        }).ContinueWith(x => App.DispatcherQueue.TryEnqueue(() => LoadingFabricApi = false));

        Task.Run(async () =>
        {
            var resource = await curseForgeClient.GetResourceAsync(322385);

            foreach (var item in resource.Files)
            {
                if (item.McVersion.Equals(vanillaId))
                {
                    App.DispatcherQueue.TryEnqueue(() =>
                    {
                        OptiFabric = item;
                        LoadingOptiFabric = false;
                    });
                    return;
                }
            };
        }).ContinueWith(x => App.DispatcherQueue.TryEnqueue(() => LoadingOptiFabric = false));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(FabricApi)) FabricApiAvailable = true;
        if (e.PropertyName == nameof(OptiFabric)) OptiFabricAvailable = true;
    }
}