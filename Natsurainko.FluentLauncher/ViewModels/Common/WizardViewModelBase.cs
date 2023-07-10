using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class WizardViewModelBase : ObservableObject
{
    public virtual bool CanNext => false;

    public virtual bool CanPrevious => true;

    public virtual bool CanCancel => true;

    public Type XamlPageType { get; init; }

    public virtual WizardViewModelBase GetNextViewModel()
    {
        throw new NotImplementedException();
    }
}
