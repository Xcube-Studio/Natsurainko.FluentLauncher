using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.UserControls;

class NoScrollingComboBox : ComboBox
{
    protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
    {
        // Disable changing option by scrolling
    }
}
