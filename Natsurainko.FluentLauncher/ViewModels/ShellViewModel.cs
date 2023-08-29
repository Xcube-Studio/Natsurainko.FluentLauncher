using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels;

class ShellViewModel : INavigationAware
{
    private readonly INavigationService _shellNavigationService;

    public ShellViewModel(INavigationService shellNavigationService)
    {
        _shellNavigationService = shellNavigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        Console.WriteLine(parameter);
    }
}
