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
    public partial bool FabricApiAvailable { get; set; }

    [ObservableProperty]
    public partial bool OptiFabricAvailable { get; set; }

    [ObservableProperty]
    public partial bool EnabledFabricApi { get; set; }

    [ObservableProperty]
    public partial bool EnabledOptiFabric { get; set; }

    [ObservableProperty]
    public partial bool LoadingFabricApi { get; set; } = true;

    [ObservableProperty]
    public partial ModrinthFile FabricApi { get; set; }

    [ObservableProperty]
    public partial bool LoadingOptiFabric { get; set; } = true;

    [ObservableProperty]
    public partial CurseForgeFile OptiFabric { get; set; }
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