using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

public partial class GetStartedViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    public GetStartedViewModel(SettingsService settings)
    {
        _settings = settings;
        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = false,
            NextPage = null
        });
    }

    [RelayCommand]
    public void Start()
    {
        App.MainWindow.ContentFrame.Navigate(typeof(Views.ShellPage));
        _settings.FinishGuide = true;
    }
}
