using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Dialogs;

public abstract class DialogProvider<TDialogBase> : IDialogProvider
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IReadOnlyDictionary<string, DialogDescriptor> _registeredDialogs;

    public IReadOnlyDictionary<string, DialogDescriptor> RegisteredDialogs => _registeredDialogs;

    public DialogProvider(IReadOnlyDictionary<string, DialogDescriptor> registeredDialogs, IServiceProvider serviceProvider)
    {
        _registeredDialogs = registeredDialogs;
        _serviceProvider = serviceProvider;
    }

    public object GetDialog(string key)
    {
        var dialogType = _registeredDialogs[key].DialogType;
        var vmType = _registeredDialogs[key].ViewModelType;

        if (vmType is null)
        {
            return _serviceProvider.GetRequiredService(dialogType);
        }
        else
        {
            var dialog = (TDialogBase)_serviceProvider.GetRequiredService(dialogType);
            var vm = _serviceProvider.GetRequiredService(vmType);
            ConfigureViewModel(dialog, vm);
            return dialog;
        }
    }

    public object? GetViewModel(string key)
    {
        var vmType = _registeredDialogs[key].ViewModelType;

        if (vmType is null)
            return null;

        return _serviceProvider.GetRequiredService(vmType);
    }

    protected abstract void ConfigureViewModel(TDialogBase dialog, object viewModel);
}
