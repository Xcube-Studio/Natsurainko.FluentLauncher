using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class EnterCoreSettingsViewModel : WizardViewModelBase
{
    public override bool CanNext => CheckAbsoluteId();

    public override bool CanPrevious => true;

    private readonly GameService _gameService = App.GetService<GameService>();

    public readonly CoreInstallationInfo _coreInstallationInfo;

    public EnterCoreSettingsViewModel(CoreInstallationInfo coreInstallationInfo)
    {
        XamlPageType = typeof(EnterCoreSettingsPage);
        _coreInstallationInfo = coreInstallationInfo;

        AbsoluteId = GetDefaultId();
        EnableIndependencyCore = _coreInstallationInfo.PrimaryLoader != null && _coreInstallationInfo.SecondaryLoader != null;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public string absoluteId;

    [ObservableProperty]
    public string nickName;

    [ObservableProperty]
    public bool enableIndependencyCore;

    private string GetDefaultId()
    {
        if (_coreInstallationInfo.PrimaryLoader == null)
            return _coreInstallationInfo.ManifestItem.Id;

        var tags = new List<string>
        {
            _coreInstallationInfo.ManifestItem.Id,
            _coreInstallationInfo.PrimaryLoader.Type.ToString() + "_" + _coreInstallationInfo.PrimaryLoader.SelectedItem.DisplayText
        };

        if (_coreInstallationInfo.SecondaryLoader != null)
            tags.Add(_coreInstallationInfo.SecondaryLoader.Type.ToString() + "_" + _coreInstallationInfo.SecondaryLoader.SelectedItem.DisplayText);

        return string.Join("-", tags);
    }

    public bool CheckAbsoluteId()
    {
        if (string.IsNullOrEmpty(AbsoluteId) || _gameService.Games.Where(x => x.InstanceId.Equals(AbsoluteId)).Any())
            return false;

        return true;
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        _coreInstallationInfo.AbsoluteId = AbsoluteId;
        _coreInstallationInfo.NickName = NickName;
        _coreInstallationInfo.EnableIndependencyCore = EnableIndependencyCore;

        return new AdditionalOptionsViewModel(_coreInstallationInfo);
    }
}
