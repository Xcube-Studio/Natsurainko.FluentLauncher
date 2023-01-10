using Newtonsoft.Json;

namespace Natsurainko.FluentCore.Class.Model.Auth.Microsoft;

public class OAuth20TokenResponseModel
{
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("user_id")]
    public string UserId { get; set; }

    [JsonProperty("foci")]
    public string Foci { get; set; }
}
