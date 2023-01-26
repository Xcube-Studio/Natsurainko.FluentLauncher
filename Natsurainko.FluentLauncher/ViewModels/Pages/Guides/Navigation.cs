using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Guides;

public partial class Navigation : ObservableObject
{
    [ObservableProperty]
    private bool canNext;

    private Type NextPage;

    public Navigation()
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
