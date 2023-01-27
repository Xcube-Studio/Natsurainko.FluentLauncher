using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Guides;

public partial class Language : SettingViewModel
{
    public Language() : base()
    {
        CanNext = true;

        Languages = LanguageResources.SupportedLanguages;
    }

    [ObservableProperty]
    private bool canNext;

    [ObservableProperty]
    private List<string> languages;

    [ObservableProperty]
    private string currentLanguage;

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        if (!loading && e.PropertyName == nameof(CurrentLanguage))
            LanguageResources.ApplyLanguage(CurrentLanguage);
#endif

        if (e.PropertyName != nameof(CanNext))
            CanNext = LanguageResources.SupportedLanguages.Contains(CurrentLanguage);

        if (e.PropertyName == nameof(CanNext))
            WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
            {
                CanNext = canNext,
                NextPage = typeof(Views.Pages.Guides.Basic)
            });
    }
}
