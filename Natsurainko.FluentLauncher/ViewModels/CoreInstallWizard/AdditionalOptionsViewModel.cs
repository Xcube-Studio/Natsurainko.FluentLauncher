using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;
using Nrk.FluentCore.Resources;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class AdditionalOptionsViewModel : WizardViewModelBase
{
    public readonly CoreInstallationInfo _coreInstallationInfo;

    private readonly CurseForgeClient curseForgeClient = App.GetService<CurseForgeClient>();
    private readonly ModrinthClient modrinthClient = App.GetService<ModrinthClient>();

    public AdditionalOptionsViewModel(CoreInstallationInfo coreInstallationInfo)
    {
        XamlPageType = typeof(AdditionalOptionsPage);

        _coreInstallationInfo = coreInstallationInfo;
        Init();
    }

    [ObservableProperty]
    private bool fabricApiAvailable;

    [ObservableProperty]
    private bool optiFabricAvailable;

    [ObservableProperty]
    private bool enabledFabricApi;

    [ObservableProperty]
    private bool enabledOptiFabric;

    [ObservableProperty]
    private ModrinthFile fabricApi;

    [ObservableProperty]
    private CurseForgeFile optiFabric;

    public override bool CanNext => true;

    public override bool CanPrevious => true;

    public void Init()
    {
        Task.Run(async () =>
        {
            foreach (var item in await modrinthClient.GetProjectVersionsAsync("P7dR8mSH"))
            {
                if (item.McVersion.Equals(_coreInstallationInfo.ManifestItem.Id))
                {
                    App.DispatcherQueue.TryEnqueue(() => FabricApi = item);
                    break;
                }
            };
        });

        Task.Run(async () =>
        {
            var resource = await curseForgeClient.GetResourceAsync(322385);

            foreach (var item in resource.Files)
            {
                if (item.McVersion.Equals(_coreInstallationInfo.ManifestItem.Id))
                {
                    App.DispatcherQueue.TryEnqueue(() => OptiFabric = item);
                    break;
                }
            };
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(FabricApi)) FabricApiAvailable = true;
        if (e.PropertyName == nameof(OptiFabric)) OptiFabricAvailable = true;
    }
}
