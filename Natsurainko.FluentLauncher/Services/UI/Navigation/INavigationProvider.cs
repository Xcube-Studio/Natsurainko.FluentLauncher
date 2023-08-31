using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

#nullable enable

/// <summary>
/// A page or window that provides navigation
/// </summary>
public interface INavigationProvider
{
    /// <summary>
    /// The UI element that provides navigation
    /// </summary>
    object NavigationControl { get; }
    string? DefaultPageKey { get; }

    /// <summary>
    /// Allow the navigation provider to get its associated navigation service
    /// </summary>
    /// <remarks>This is needed because pages do not support DI if they are used in Frame.
    /// This might be removed because pages can access their viewmodel, which support DI.</remarks>
    /// <param name="navigationService"></param>
    void Initialize(INavigationService navigationService) { } // Optional; might be removed
}
