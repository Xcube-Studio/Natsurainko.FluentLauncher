using Natsurainko.FluentCore.Class.Model.Launch;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Extension.Windows.Class.Model.Launch
{
    [SupportedOSPlatform("windows")]
    public static class LaunchResponseExtension
    {
        public static void SetMainWindowTitle(this LaunchResponse launchResponse, string title, int frequency = 500)
        {
            launchResponse.Process?.WaitForInputIdle();

            string raw = launchResponse.Process.MainWindowTitle;

            Task.Run(async () =>
            {
                try
                {
                    while (!(launchResponse.Process?.HasExited).GetValueOrDefault())
                    {
                        if (launchResponse.Process != null && launchResponse.Process?.MainWindowTitle != title)
                            DllImports.SetWindowText(launchResponse.Process.MainWindowHandle, title);

                        await Task.Delay(frequency);
                        launchResponse.Process?.Refresh();
                    }
                }
                catch //(Exception ex)
                {
                    //throw;
                }
            });
        }
    }
}
