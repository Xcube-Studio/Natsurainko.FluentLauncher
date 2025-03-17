using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.WinUI.Mvvm;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Dialogs;

public class WinUIDialogProvider : DialogProvider<ContentDialog>
{
    public static WinUIDialogProviderBuilder CreateBuilder() => new();

    public WinUIDialogProvider(
        IReadOnlyDictionary<string, DialogDescriptor> registeredDialogs,
        IServiceProvider serviceProvider)
        : base(registeredDialogs, serviceProvider) { }

    protected override void ConfigureViewModel(ContentDialog dialog, object viewModel)
    {
        dialog.DataContext = viewModel;

        if (viewModel is IViewAssociated viewAssociatedModel)
        {
            viewAssociatedModel.Dispatcher = dialog.DispatcherQueue;

            dialog.Loaded += (_, _) => viewAssociatedModel.OnLoaded();
            dialog.Unloaded += (_, _) => viewAssociatedModel.OnUnloaded();

            if (viewAssociatedModel is IViewAssociated<ContentDialog> dialogAssociatedModel)
                dialogAssociatedModel.SetView(dialog);
        }
    }
}
