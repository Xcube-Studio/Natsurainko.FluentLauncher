using FluentCore.Model.Auth;
using System;
using System.Threading.Tasks;

namespace FluentCore.Interface
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    public interface IAuthenticator : IDisposable
    {
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        Tuple<BaseResponseModel, AuthResponseType> Authenticate();

        /// <summary>
        /// 验证(异步)
        /// </summary>
        /// <returns></returns>
        Task<Tuple<BaseResponseModel, AuthResponseType>> AuthenticateAsync();
    }
}
