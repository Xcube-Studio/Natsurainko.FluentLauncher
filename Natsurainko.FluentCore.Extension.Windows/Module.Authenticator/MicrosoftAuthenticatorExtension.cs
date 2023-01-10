using Natsurainko.FluentCore.Module.Authenticator;
using System.Diagnostics;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Extension.Windows.Module.Authenticator
{
    [SupportedOSPlatform("windows")]
    public static class MicrosoftAuthenticatorExtension
    {
        public static async Task<bool> GetAccessCode(this MicrosoftAuthenticator authenticator, bool selectAccount = false)
        {
            string url = "https://login.live.com/oauth20_authorize.srf" +
                $"?client_id={authenticator.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={authenticator.RedirectUri}" +
                "&scope=XboxLive.signin%20offline_access";

            if (selectAccount)
                url += "&prompt=select_account";

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

            string code = string.Empty;
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

            if (code.Contains("error"))
                return false;

            authenticator.Code = code;

            httpListener.Stop();
            httpListener.Close();

            return true;
        }

        public static async Task<bool> GetAccessCode(this MicrosoftAuthenticator authenticator, CancellationToken cancellationToken, bool selectAccount = false)
        {
            string url = "https://login.live.com/oauth20_authorize.srf" +
                $"?client_id={authenticator.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={authenticator.RedirectUri}" +
                "&scope=XboxLive.signin%20offline_access";

            if (selectAccount)
                url += "&prompt=select_account";

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

            string code = string.Empty;
            var res = httpListener.GetContextAsync();

            try
            {
                while (!res.IsCompleted)
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(100);
                }

                using (var stream = (await res).Response.OutputStream)
                {
                    code = (await res).Request.Url.ToString().Split('=')[1];

                    if (code.Contains("error"))
                        stream.Write(Encoding.Default.GetBytes("Login Failed ! Please Back to Launcher to Retry"));
                    else stream.Write(Encoding.Default.GetBytes("Login Successfully ! Please Back to Launcher to Check"));

                    stream.Flush();
                    await Task.Delay(1000);
                }

                if (code.Contains("error"))
                    return false;

                authenticator.Code = code;

            }
            catch
            {
                httpListener.Stop();
                httpListener.Close();

                return false;
            }

            httpListener.Stop();
            httpListener.Close();

            return true;
        }
    }
}
