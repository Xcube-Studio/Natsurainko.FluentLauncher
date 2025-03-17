using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class AddVmArgumentDialogViewModel : DialogVM
{
    private Action<string> _addAction = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial string? Argument { get; set; }

    [MemberNotNullWhen(true, nameof(Argument))]
    private bool EnableAddArgument => !string.IsNullOrEmpty(Argument);

    public override void HandleParameter(object param) => _addAction = (Action<string>)param;

    [RelayCommand(CanExecute = nameof(EnableAddArgument))]
    public void Add()
    {
        _addAction(Argument!); // Argument is not null when EnableAddArgument is true
        View.Hide();
    }

    [RelayCommand]
    public void Cancel() => View.Hide();
}
