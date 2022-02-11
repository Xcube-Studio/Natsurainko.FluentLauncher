using FluentCore.Model.Game;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Auth.Yggdrasil
{
    /// <summary>
    /// Yggdrasil标准返回模型
    /// </summary>
    public class YggdrasilResponseModel : BaseResponseModel
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("availableProfiles")]
        public IEnumerable<ProfileModel> AvailableProfiles { get; set; }

        [JsonProperty("selectedProfile")]
        public ProfileModel SelectedProfile { get; set; }
    }

    /// <summary>
    /// Yggdrasil错误返回模型
    /// </summary>
    public class ErrorResponseModel : BaseResponseModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("cause")]
        public string Cause { get; set; }
    }

    public class User
    {
        [JsonProperty("properties")]
        public IEnumerable<PropertyModel> Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ProfileModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
