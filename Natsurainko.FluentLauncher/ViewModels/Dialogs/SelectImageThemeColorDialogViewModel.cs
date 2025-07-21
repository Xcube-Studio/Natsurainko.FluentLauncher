using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class SelectImageThemeColorDialogViewModel : DialogVM
{
    [ObservableProperty]
    public partial Color[] Colors { get; set; }

    [ObservableProperty]
    public partial Color Color { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; } = true;

    public override void HandleParameter(object param)
    {
        if (param is not string imageFile)
            throw new ArgumentException("Parameter must be a string representing the image file path.", nameof(param));

        if (!File.Exists(imageFile))
            throw new FileNotFoundException("The specified image file does not exist.", imageFile);

        Task.Run(() => ThemeColorExtractor.GetThemeColors(imageFile))
            .ContinueWith(t => Dispatcher.TryEnqueue(() => Colors = t.Result), TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith(t => Dispatcher.TryEnqueue(() => Loading = false));
    }
}
