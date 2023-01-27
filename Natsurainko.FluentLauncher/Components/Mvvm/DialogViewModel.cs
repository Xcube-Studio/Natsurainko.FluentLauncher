using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

public partial class DialogViewModel : ObservableObject
{
    protected virtual bool EnableConfirmButton() => true;

    protected virtual bool EnableCancelButton() => true;

    [RelayCommand(CanExecute = nameof(EnableConfirmButton))]
    public Task Confirm(ContentDialog dialog) => Task.Run(() => OnConfirm(dialog));

    [RelayCommand(CanExecute = nameof(EnableCancelButton))]
    public Task Cancel(ContentDialog dialog) => Task.Run(() => OnCancel(dialog));

    protected virtual void OnConfirm(ContentDialog dialog)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => dialog.Hide());

    protected virtual void OnCancel(ContentDialog dialog)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => dialog.Hide());
}