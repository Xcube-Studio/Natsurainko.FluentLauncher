using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class AddVmArgumentDialogViewModel : ObservableObject, IDialogParameterAware
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial string? Argument { get; set; }

    private Action<string> _addAction = null!;
    private ContentDialog _dialog = null!;

    public AddVmArgumentDialogViewModel() { }

    void IDialogParameterAware.HandleParameter(object param)
    {
        _addAction = (Action<string>)param;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = (ContentDialog)grid.FindName("Dialog");
    }

    [RelayCommand(CanExecute = nameof(EnableAddArgument))]
    public void Add()
    {
        _addAction(Argument!); // Argument is not null when EnableAddArgument is true
        _dialog.Hide();
    }

    [RelayCommand]
    public void Cancel() => _dialog.Hide();

    [MemberNotNullWhen(true, nameof(Argument))]
    private bool EnableAddArgument => !string.IsNullOrEmpty(Argument);
}
