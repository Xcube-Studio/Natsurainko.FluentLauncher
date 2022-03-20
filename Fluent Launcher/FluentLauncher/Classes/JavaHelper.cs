using FluentLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace FluentLauncher.Classes
{
    public class JavaHelper
    {
        public async static Task<JreInfo> GetInfo(JavaRuntimeEnvironment java)
        {
            var res = await App.DesktopBridge.Connection.SendMessageAsync(new ValueSet()
            {
                { "Header", "GetJavaRuntimeEnvironmentInfo" },
                { "Path", java.Path }
            });

            return JsonConvert.DeserializeObject<JreInfo>((string)res.Message["Response"]);
        }

        public async static Task<JreInfo> GetInfo(string java)
        {
            var res = await App.DesktopBridge.Connection.SendMessageAsync(new ValueSet()
            {
                { "Header", "GetJavaRuntimeEnvironmentInfo" },
                { "Path", java }
            });

            return JsonConvert.DeserializeObject<JreInfo>((string)res.Message["Response"]);
        }
    }
}
