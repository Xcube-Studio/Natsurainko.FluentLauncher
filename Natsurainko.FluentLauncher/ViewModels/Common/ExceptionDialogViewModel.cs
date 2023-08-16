using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class ExceptionDialogViewModel : ObservableObject
{
    [ObservableProperty]
    string errorMessage;

    public ExceptionDialogViewModel(string errorMessage = "")
    {
        this.errorMessage = errorMessage;
    }

    [RelayCommand]
    void CopyAndLaunchGitHub()
    {
        // Copy error message
        var package = new DataPackage();
        package.SetText(ErrorMessage);
        Clipboard.SetContent(package);

        // Launch GitHub
        _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose"));
    }
}