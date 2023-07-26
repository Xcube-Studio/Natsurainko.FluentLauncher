using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Authenticate;

public class DisplayClaimsModel
{
    [JsonPropertyName("xui")]
    public JsonArray Xui { get; set; }
}

public class OAuth20TokenResponseModel
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("foci")]
    public string Foci { get; set; }
}

public class XBLAuthenticateRequestModel
{
    [JsonPropertyName("Properties")]
    public XBLAuthenticatePropertiesModel Properties { get; set; } = new XBLAuthenticatePropertiesModel();

    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; } = "http://auth.xboxlive.com";

    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; } = "JWT";
}

public class XBLAuthenticateResponseModel
{
    [JsonPropertyName("IssueInstant")]
    public string IssueInstant { get; set; }

    [JsonPropertyName("NotAfter")]
    public string NotAfter { get; set; }

    [JsonPropertyName("Token")]
    public string Token { get; set; }

    [JsonPropertyName("DisplayClaims")]
    public DisplayClaimsModel DisplayClaims { get; set; }
}

public class XBLAuthenticatePropertiesModel
{
    [JsonPropertyName("AuthMethod")]
    public string AuthMethod { get; set; } = "RPS";

    [JsonPropertyName("SiteName")]
    public string SiteName { get; set; } = "user.auth.xboxlive.com";

    [JsonPropertyName("RpsTicket")]
    public string RpsTicket { get; set; } = "d=<access token>";
}

public class XSTSAuthenticateRequestModel
{
    [JsonPropertyName("Properties")]
    public XSTSAuthenticatePropertiesModels Properties { get; set; } = new XSTSAuthenticatePropertiesModels();

    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; } = "rp://api.minecraftservices.com/";

    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; } = "JWT";
}

public class XSTSAuthenticateResponseModel
{
    [JsonPropertyName("IssueInstant")]
    public string IssueInstant { get; set; }

    [JsonPropertyName("NotAfter")]
    public string NotAfter { get; set; }

    [JsonPropertyName("Token")]
    public string Token { get; set; }

    [JsonPropertyName("DisplayClaims")]
    public DisplayClaimsModel DisplayClaims { get; set; }
}

public class XSTSAuthenticateErrorModel
{
    [JsonPropertyName("Identity")]
    public string Identity { get; set; }

    [JsonPropertyName("XErr")]
    public long XErr { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }

    [JsonPropertyName("Redirect")]
    public string Redirect { get; set; }
}

public class XSTSAuthenticatePropertiesModels
{
    [JsonPropertyName("SandboxId")]
    public string SandboxId { get; set; } = "RETAIL";

    [JsonPropertyName("UserTokens")]
    public List<string> UserTokens { get; set; } = new List<string>();
}

public class MicrosoftAuthenticationResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("skins")]
    public List<SkinModel> Skins { get; set; }

    [JsonPropertyName("capes")]
    public JsonArray Capes { get; set; }
}

public class SkinModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("alias")]
    public string Alias { get; set; }
}

public class DeviceCodeResponse
{
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; }

    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; }

    [JsonPropertyName("verification_uri")]
    public string VerificationUrl { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class DeviceFlowResponse
{
    public bool Success { get; set; }

    public OAuth20TokenResponseModel OAuth20TokenResponse { get; set; }
}

