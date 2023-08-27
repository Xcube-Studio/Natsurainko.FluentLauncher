using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class EnterCoreSettingsViewModel : WizardViewModelBase
{
    public override bool CanNext
    {
        get
        {
            if ((!_gameService.GameInfos.Select(x => x.AbsoluteId)?.ToList().Contains(AbsoluteId)).GetValueOrDefault())
                return true;

            return false;
        }
    }

    public override bool CanPrevious => true;

    private readonly GameService _gameService = App.GetService<GameService>();

    public EnterCoreSettingsViewModel(VersionManifestItem manifestItem)
    {
        XamlPageType = typeof(EnterCoreSettingsPage);
        AbsoluteId = manifestItem.Id;
    }

    [ObservableProperty]
    public string absoluteId;

    [ObservableProperty]
    public string nickName;

    [ObservableProperty]
    public bool enableIndependencyCore;

    public override WizardViewModelBase GetNextViewModel()
    {
        throw new System.Exception();
    }
}
