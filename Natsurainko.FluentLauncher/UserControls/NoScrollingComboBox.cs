using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.UserControls;

class NoScrollingComboBox : ComboBox
{
    protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
    {
        // Disable changing option by scrolling
    }
}
