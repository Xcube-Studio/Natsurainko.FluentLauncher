using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;

namespace Natsurainko.FluentLauncher.ViewModels.Settings
{
    public partial class Appearance : SettingViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        private List<string> languages = LanguageResources.SupportedLanguages;

        [ObservableProperty]
        private string currentLanguage;

        #endregion


        protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
        {
#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
            if (!loading && e.PropertyName == nameof(CurrentLanguage))
                LanguageResources.ApplyLanguage(CurrentLanguage);
#endif
        }
    }
}
