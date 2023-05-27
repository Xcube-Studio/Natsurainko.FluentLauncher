using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExceptionPage : Page
    {
        public ExceptionPage(string errorMessage = "")
        {
            InitializeComponent();
            DataContext = new ExceptionDialogViewModel(errorMessage);
        }
    }
}
