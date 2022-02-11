using FluentCore.Interface;
using FluentCore.Model;
using FluentCore.Service.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Authenticator
{
    /// <summary>
    /// Authlib-Injector封装类
    /// </summary>
    public class AuthlibInjector : IAuthlibInjector
    {
        public AuthlibInjector(string url, string path)
        {
            this.Url = url;
            this.FilePath = path;
        }

        public string Url { get; set; }

        public string FilePath { get; set; }

        public IEnumerable<string> GetArguments() => this.GetArgumentsAsync().GetAwaiter().GetResult();

        public async Task<IEnumerable<string>> GetArgumentsAsync()
        {
            List<string> values = new List<string>();
            values.Add(this.GetJavaAgent().ToArgument());

            string res = await (await HttpHelper.HttpGetAsync(this.Url)).Content.ReadAsStringAsync();
            values.Add($"-Dauthlibinjector.yggdrasil.prefetched={Convert.ToBase64String(Encoding.UTF8.GetBytes(res))}");

            return values;
        }

        public JavaAgentModel GetJavaAgent()
        {
            return new JavaAgentModel
            {
                AgentPath = this.FilePath,
                Parameter = this.Url
            };
        }

        /// <summary>
        /// 下载Authlib-Injector调用Jar(异步)
        /// </summary>
        /// <param name="saveFolder"></param>
        /// <returns></returns>
        public static async Task<FileInfo> DownloadAsync(string saveFolder)
        {
            return (await HttpHelper.HttpDownloadAsync($"https://download.mcbbs.net/mirrors/authlib-injector/artifact/38/authlib-injector-1.1.38.jar", saveFolder)).FileInfo;
        }
    }
}
