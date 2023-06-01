using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.Generic;
using System.ComponentModel;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

partial class AppearanceViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    string currentLanguage;

    public List<string> SupportedLanguages => LanguageResources.SupportedLanguages;


    public AppearanceViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();

        PropertyChanged += AppearanceViewModel_PropertyChanged;
    }

    private void AppearanceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentLanguage))
            LanguageResources.ApplyLanguage(CurrentLanguage);
    }
}
