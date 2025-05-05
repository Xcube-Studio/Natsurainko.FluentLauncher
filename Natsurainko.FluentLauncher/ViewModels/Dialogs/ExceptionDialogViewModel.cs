using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class ExceptionDialogViewModel(string errorMessage = "") : ObservableObject
{
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = errorMessage;

    [RelayCommand]
    async Task CopyAndLaunchGitHub()
    {
        ClipboardHepler.SetText(ErrorMessage);
        await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose"));
    }
}