using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Utils;

internal static class ClipboardHepler
{
    public static void SetText(string text)
    {
        DataPackage dataPackage = new();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }
}
