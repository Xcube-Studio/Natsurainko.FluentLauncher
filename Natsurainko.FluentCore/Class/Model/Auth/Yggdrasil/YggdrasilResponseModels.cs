using Newtonsoft.Json;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Auth.Yggdrasil
{
    public class YggdrasilResponseModel : BaseYggdrasilResponseModel
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

    public class ErrorResponseModel : BaseYggdrasilResponseModel
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

    public class PropertyModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ProfileModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class BaseYggdrasilResponseModel { }
}
