using FluentCore.Interface;
using FluentCore.Model.Auth;
using FluentCore.Model.Auth.Yggdrasil;
using FluentCore.Model.Game;
using FluentCore.Service.Local;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Authenticator
{
    /// <summary>
    /// 离线验证器
    /// </summary>
    public class OfflineAuthenticator : IAuthenticator
    {
        public OfflineAuthenticator(string userName, Guid uuid = default)
        {
            this.UserName = userName;
            this.Uuid = uuid;
        }

        public string UserName { get; private set; }

        public Guid Uuid { get; private set; }

        public Tuple<BaseResponseModel, AuthResponseType> Authenticate()
        {
            Uuid = Uuid.Equals(null) ? UuidHelper.FromString(UserName) : Guid.NewGuid();

            var model = new YggdrasilResponseModel
            {
                AccessToken = Guid.NewGuid().ToString("N"),
                ClientToken = Guid.NewGuid().ToString("N"),
                SelectedProfile = new ProfileModel
                {
                    Id = Uuid.ToString("N"),
                    Name = UserName
                },
                User = new User()
                {
                    Id = Uuid.ToString("N"),
                    Properties = new List<PropertyModel>()
                    {
                        new PropertyModel
                        {
                            Name = "preferredLanguage",
                            Value = "zh-cn"
                        }
                    }
                }
            };

            return new Tuple<BaseResponseModel, AuthResponseType>(model, AuthResponseType.Succeeded);
        }

        public Task<Tuple<BaseResponseModel, AuthResponseType>> AuthenticateAsync() => Task.Run(Authenticate);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }
    }
}
