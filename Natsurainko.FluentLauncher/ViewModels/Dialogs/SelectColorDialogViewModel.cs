using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Windows.UI;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class SelectColorDialogViewModel : DialogVM
{
    [ObservableProperty]
    public partial Color Color { get; set; }

    public Color DefaultColor { get; private set; } = Colors.Transparent;

    public override void HandleParameter(object param)
    {
        if (param is Color color)
            DefaultColor = color;
    }

    protected override void OnLoaded()
    {
        Color = DefaultColor;
    }
}
