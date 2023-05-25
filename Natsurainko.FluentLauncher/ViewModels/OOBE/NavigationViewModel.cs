using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Models;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

public partial class OOBENavigationViewModel : ObservableObject
{
    [ObservableProperty]
    private bool canNext;

    private Type NextPage;

    public OOBENavigationViewModel()
    {
        WeakReferenceMessenger.Default.Register<GuideNavigationMessage>(this, (r, m) =>
        {
            CanNext = m.CanNext;
            NextPage = m.NextPage;
        });
    }

    [RelayCommand]
    public void Next(Frame frame) => frame.Navigate(
        NextPage,
        null,
        new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

    [RelayCommand]
    public void Back(Frame frame) => frame.GoBack();
}
