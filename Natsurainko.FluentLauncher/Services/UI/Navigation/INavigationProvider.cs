using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

/// <summary>
/// A page or window that provides navigation
/// </summary>
public interface INavigationProvider
{
    /// <summary>
    /// The UI element that provides navigation
    /// </summary>
    object NavigationControl { get; }
}
