using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Dialogs;

public abstract class DialogProviderBuilder<TDialogProvider, TDialogBase> where TDialogProvider : DialogProvider<TDialogBase>
{
    protected readonly Dictionary<string, DialogDescriptor> _registeredDialogs = new();

    public IDictionary<string, DialogDescriptor> RegisteredDialogs => _registeredDialogs;

    public DialogProviderBuilder() { }

    public DialogProviderBuilder<TDialogProvider, TDialogBase> WithDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDialog, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(string key)
        => WithDialog(key, typeof(TDialog), typeof(TViewModel));

    public DialogProviderBuilder<TDialogProvider, TDialogBase> WithDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDialog>(string key)
        => WithDialog(key, typeof(TDialog));

    public DialogProviderBuilder<TDialogProvider, TDialogBase> WithDialog(
        string key,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type dialogType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? viewModelType = null)
    {
        _registeredDialogs.Add(key, new DialogDescriptor(dialogType, viewModelType));
        return this;
    }

    public abstract TDialogProvider Build(IServiceProvider serviceProvider);
}
