using FluentCore.Extend.Service.Local;
using FluentCore.Service.Component.Authenticator;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentCore.Extend.Service.Component.Authenticator
{
    /// <summary>
    /// 微软验证的调用浏览器扩展
    /// <para>
    /// 警告:该类仅供Windows环境使用
    /// </para>
    /// <para>
    /// 警告:重定向地址必须以http://localhost:为开头且必须带有端口 例如:http://localhost:5001
    /// </para>
    /// <para>
    /// 警告:若调用程序不带有窗口界面MainWindow 请勿设置参数 activateMainWindow = true
    /// </para>
    /// </summary>
    public static class MicrosoftAuthenticatorWindowsExtend
    {
        public static async Task<string> GetAuthorizationCodeFromBrower(this MicrosoftAuthenticator authenticator, bool activateMainWindow = false)
        {
            string url = "https://login.live.com/oauth20_authorize.srf" +
                $"?client_id={authenticator.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={authenticator.RedirectUri}" +
                "&scope=XboxLive.signin%20offline_access" +
                "&prompt=select_account";

            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"{authenticator.RedirectUri.TrimEnd('/')}/");
            httpListener.Start();

            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    FileName = "cmd"
                },
            };

            process.Start();
            process.StandardInput.WriteLine($"start \"\" \"{url}\"");
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();

            string code;
            var res = await httpListener.GetContextAsync();
            using (var stream = res.Response.OutputStream)
            {
                code = res.Request.Url.ToString().Split('=')[1];

                if (code.Contains("error"))
                    stream.Write(Encoding.Default.GetBytes("Login Failed ! Please Back to Launcher to Retry"));
                else stream.Write(Encoding.Default.GetBytes("Login Successfully ! Please Back to Launcher to Check"));

                stream.Flush();
                await Task.Delay(1000);
            }

            httpListener.Stop();
            httpListener.Close();

            if (activateMainWindow)
                NativeWin32MethodExtend.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

            if (code.Contains("error"))
                return res.Request.Url.ToString().Replace(authenticator.RedirectUri, "");

            return code;
        }
    }
}
