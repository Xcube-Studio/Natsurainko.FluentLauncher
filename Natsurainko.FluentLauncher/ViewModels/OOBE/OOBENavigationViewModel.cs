using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class OOBENavigationViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private bool canNext;

    private readonly INavigationService _navigationService;
    
    private Type NextPage;

    public OOBENavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        WeakReferenceMessenger.Default.Register<GuideNavigationMessage>(this, (r, m) =>
        {
            CanNext = m.CanNext;
            NextPage = m.NextPage;
        });
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _navigationService.NavigateTo("OOBELanguagePage");
    }

    [RelayCommand]
    public void Next(Frame frame) => frame.Navigate(
        NextPage,
        null,
        new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

    [RelayCommand]
    public void Back(Frame frame) => frame.GoBack();
}
