using FluentCore.Interface;
using FluentCore.Model.Auth;
using FluentCore.Model.Auth.Yggdrasil;
using FluentCore.Service.Network;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Authenticator
{
    /// <summary>
    /// Yggdrasil验证器
    /// </summary>
    public class YggdrasilAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Yggdrasil验证服务器地址
        /// </summary>
        public string YggdrasilServerUrl { get; set; } = "https://authserver.mojang.com";

        /// <summary>
        /// 客户端令牌
        /// </summary>
        public string ClientToken { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 登录邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }

        public YggdrasilAuthenticator(string email, string password, string yggdrasilServerUrl = default, string clientToken = default)
        {
            this.Email = email;
            this.Password = password;

            this.YggdrasilServerUrl = string.IsNullOrEmpty(yggdrasilServerUrl) ? this.YggdrasilServerUrl : $"{yggdrasilServerUrl}/authserver";
            this.ClientToken = string.IsNullOrEmpty(clientToken) ? this.ClientToken : clientToken;
        }

        public YggdrasilAuthenticator(string yggdrasilServerUrl = default)
        {
            this.YggdrasilServerUrl = string.IsNullOrEmpty(yggdrasilServerUrl) ? this.YggdrasilServerUrl : $"{yggdrasilServerUrl}/authserver";
        }

        /// <summary>
        /// 登录Yggdrasil服务器
        /// </summary>
        /// <returns></returns>
        public Tuple<BaseResponseModel, AuthResponseType> Authenticate() => AuthenticateAsync().GetAwaiter().GetResult();

        /// <summary>
        /// 登录Yggdrasil服务器(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<BaseResponseModel, AuthResponseType>> AuthenticateAsync()
        {
            string content = JsonConvert.SerializeObject(
                new LoginRequestModel
                {
                    ClientToken = this.ClientToken,
                    UserName = this.Email,
                    Password = this.Password
                }
            );

            using var res = await HttpHelper.HttpPostAsync($"{this.YggdrasilServerUrl}/authenticate", content);

            string text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
                return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<YggdrasilResponseModel>(text), AuthResponseType.Succeeded);
            else return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<ErrorResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Failed);
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <returns></returns>
        public Tuple<BaseResponseModel, AuthResponseType> Refresh(string accessToken, ProfileModel profile = null) => RefreshAsync(accessToken, profile).GetAwaiter().GetResult();

        /// <summary>
        /// 刷新令牌(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<BaseResponseModel, AuthResponseType>> RefreshAsync(string accessToken, ProfileModel profile = null)
        {
            string content = JsonConvert.SerializeObject(
                new
                {
                    clientToken = this.ClientToken,
                    accessToken,
                    requestUser = true
                }
            );

            if (profile != null)
                content = JsonConvert.SerializeObject(
                new
                {
                    clientToken = this.ClientToken,
                    accessToken,
                    requestUser = true,
                    selectedProfile = profile
                }
            );

            using var res = await HttpHelper.HttpPostAsync($"{YggdrasilServerUrl}/refresh", content);

            if (res.IsSuccessStatusCode)
                return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<YggdrasilResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Succeeded);
            else return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<ErrorResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Failed);
        }

        /// <summary>
        /// 验证令牌
        /// </summary>
        /// <returns></returns>
        public Tuple<BaseResponseModel, AuthResponseType> Validate(string accessToken) => ValidateAsync(accessToken).GetAwaiter().GetResult();

        /// <summary>
        /// 验证令牌(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<BaseResponseModel, AuthResponseType>> ValidateAsync(string accessToken)
        {
            string content = JsonConvert.SerializeObject(
                new YggdrasilRequestModel
                {
                    ClientToken = this.ClientToken,
                    AccessToken = accessToken
                }
            );

            using var res = await HttpHelper.HttpPostAsync($"{YggdrasilServerUrl}/validate", content);

            if (res.IsSuccessStatusCode)
                return new Tuple<BaseResponseModel, AuthResponseType>
                    (null, AuthResponseType.Succeeded);
            else return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<ErrorResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Failed);
        }

        /// <summary>
        /// 登出Yggdrasil服务器
        /// </summary>
        /// <returns></returns>
        public Tuple<BaseResponseModel, AuthResponseType> Signout() => SignoutAsync().GetAwaiter().GetResult();

        /// <summary>
        /// 登出Yggdrasil服务器(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<BaseResponseModel, AuthResponseType>> SignoutAsync()
        {
            string content = JsonConvert.SerializeObject(
                new
                {
                    username = this.Email,
                    password = this.Password
                }
            );

            using var res = await HttpHelper.HttpPostAsync($"{YggdrasilServerUrl}/signout", content);

            if (res.IsSuccessStatusCode)
                return new Tuple<BaseResponseModel, AuthResponseType>
                    (null, AuthResponseType.Succeeded);
            else return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<ErrorResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Failed);
        }

        /// <summary>
        /// 使令牌失效
        /// </summary>
        /// <returns></returns>
        public Tuple<BaseResponseModel, AuthResponseType> Invalidate(string accessToken) => InvalidateAsync(accessToken).GetAwaiter().GetResult();

        /// <summary>
        /// 使令牌失效(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<BaseResponseModel, AuthResponseType>> InvalidateAsync(string accessToken)
        {
            string content = JsonConvert.SerializeObject(
                new YggdrasilRequestModel
                {
                    ClientToken = this.ClientToken,
                    AccessToken = accessToken
                }
            );

            using var res = await HttpHelper.HttpPostAsync($"{YggdrasilServerUrl}/invalidate", content);

            if (res.IsSuccessStatusCode)
                return new Tuple<BaseResponseModel, AuthResponseType>
                    (null, AuthResponseType.Succeeded);
            else return new Tuple<BaseResponseModel, AuthResponseType>
                    (JsonConvert.DeserializeObject<ErrorResponseModel>(await res.Content.ReadAsStringAsync()), AuthResponseType.Failed);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Email = null;
                this.Password = null;
                GC.Collect();
            }
        }
    }
}
