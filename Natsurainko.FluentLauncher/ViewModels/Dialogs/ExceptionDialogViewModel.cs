using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class ExceptionDialogViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string ErrorMessage { get; set; }

    public ExceptionDialogViewModel(string errorMessage = "")
    {
        ErrorMessage = errorMessage;
    }

    [RelayCommand]
    void CopyAndLaunchGitHub()
    {
        // Copy error message
        var package = new DataPackage();
        package.SetText(ErrorMessage);
        Clipboard.SetContent(package);

        // Start GitHub
        _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose"));
    }
}