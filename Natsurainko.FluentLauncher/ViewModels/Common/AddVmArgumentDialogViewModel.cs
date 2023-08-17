using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class AddVmArgumentDialogViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string argument;

    private readonly Action<string> _addAction;
    private ContentDialog _dialog;

    public AddVmArgumentDialogViewModel(Action<string> addAction)
    {
        _addAction = addAction;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = grid.FindName("Dialog") as ContentDialog;
    }

    [RelayCommand(CanExecute = nameof(EnableAddArgument))]
    public void Add()
    {
        _addAction(Argument);
        _dialog.Hide();
    }

    [RelayCommand]
    public void Cancel() => _dialog.Hide();

    private bool EnableAddArgument => !string.IsNullOrEmpty(Argument);
}
