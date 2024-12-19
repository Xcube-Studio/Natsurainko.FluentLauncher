﻿using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Dialogs;

public class WinUIDialogProvider : DialogProvider<ContentDialog>
{
    public static WinUIDialogProviderBuilder CreateBuilder() => new WinUIDialogProviderBuilder();

    public WinUIDialogProvider(
        IReadOnlyDictionary<string, DialogDescriptor> registeredDialogs,
        IServiceProvider serviceProvider)
        : base(registeredDialogs, serviceProvider) { }

    protected override void ConfigureViewModel(ContentDialog dialog, object viewModel)
        => dialog.DataContext = viewModel;
}
