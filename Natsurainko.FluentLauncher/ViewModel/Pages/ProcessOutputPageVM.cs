using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages;

public class ProcessOutputPageVM : ViewModelBase<Page>
{
    public ProcessOutputPageVM(Page control) : base(control)
    {
    }

    [Reactive]
    public ObservableCollection<string> Outputs { get; set; }
}
