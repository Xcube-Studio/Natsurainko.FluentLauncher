using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Dialogs;

class WinUIDialogActivationService : IDialogActivationService<ContentDialogResult>
{
    public Task<ContentDialogResult> ShowDialogAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task<ContentDialogResult> ShowDialogAsync(string key, object param)
    {
        throw new NotImplementedException();
    }
}
