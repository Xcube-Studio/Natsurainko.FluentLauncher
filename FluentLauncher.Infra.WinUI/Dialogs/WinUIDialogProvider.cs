using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.WinUI.Mvvm;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Dialogs;

public class WinUIDialogProvider(
    IReadOnlyDictionary<string, DialogDescriptor> registeredDialogs,
    IServiceProvider serviceProvider) 
    : DialogProvider<ContentDialog>(registeredDialogs, serviceProvider)
{
    public static WinUIDialogProviderBuilder CreateBuilder() => new();

    protected override void ConfigureViewModel(ContentDialog dialog, object viewModel)
    {
        dialog.DataContext = viewModel;

        if (viewModel is IViewAssociated viewAssociatedModel)
        {
            viewAssociatedModel.Dispatcher = dialog.DispatcherQueue;

            dialog.Loading += (_, _) => viewAssociatedModel.OnLoading();
            dialog.Loaded += (_, _) => viewAssociatedModel.OnLoaded();
            dialog.Unloaded += (_, _) => viewAssociatedModel.OnUnloaded();

            if (viewAssociatedModel is IViewAssociated<ContentDialog> dialogAssociatedModel)
                dialogAssociatedModel.SetView(dialog);
        }
    }
}
