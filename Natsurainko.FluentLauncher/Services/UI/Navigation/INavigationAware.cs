using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

/// <summary>
/// Implemented by a view model that can respond to navigation events
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object parameter) { }

    void OnNavigatedFrom() { }
}
