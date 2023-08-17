using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Windows;

// Sevice for operations of a window
interface IWindowService
{
    string Title { get; set; }

    /// <summary>
    /// Close the window
    /// </summary>
    void Close();
    /// <summary>
    /// Hide the window
    /// </summary>
    void Hide();
    /// <summary>
    /// Activate the window
    /// </summary>
    void Activate();
}
