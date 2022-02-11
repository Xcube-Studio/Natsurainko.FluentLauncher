using FluentCore.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentCore.Interface
{
    /// <summary>
    /// Authlib-Injector调用接口
    /// </summary>
    public interface IAuthlibInjector
    {
        /// <summary>
        /// 外置验证服务器地址
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// 获取Authlib-Injector参数
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetArguments();

        /// <summary>
        /// 获取Authlib-Injector参数(异步)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetArgumentsAsync();

        /// <summary>
        /// 获取JavaAgent参数
        /// </summary>
        /// <returns></returns>
        JavaAgentModel GetJavaAgent();
    }
}
