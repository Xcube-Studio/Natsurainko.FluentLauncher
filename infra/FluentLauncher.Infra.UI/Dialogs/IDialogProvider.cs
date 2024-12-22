using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Dialogs;

/// <summary>
/// A factory for providing dialogs.
/// </summary>
public interface IDialogProvider
{
    IReadOnlyDictionary<string, DialogDescriptor> RegisteredDialogs { get; }

    object GetDialog(string key);

    object? GetViewModel(string key);
}

public record DialogDescriptor
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type DialogType { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? ViewModelType { get; init; }

    public DialogDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type dialogType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? vmType)
    {
        DialogType = dialogType;
        ViewModelType = vmType;
    }
}
