﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Models;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

public partial class GetStarted : ObservableObject
{
    public GetStarted()
    {
        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = false,
            NextPage = null
        });
    }

    [RelayCommand]
    public void Start()
    {
        App.MainWindow.ContentFrame.Navigate(typeof(Views.Pages.MainContainer));
        App.Configuration.FinishGuide = true;
    }
}
