using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Auth.Microsoft
{
    public class XSTSAuthenticateRequestModel
    {
        [JsonProperty("Properties")]
        public XSTSAuthenticatePropertiesModels Properties { get; set; } = new XSTSAuthenticatePropertiesModels();

        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; } = "rp://api.minecraftservices.com/";

        [JsonProperty("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public class XSTSAuthenticateResponseModel
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public DisplayClaimsModel DisplayClaims { get; set; }
    }

    public class XSTSAuthenticateErrorModel
    {
        [JsonProperty("Identity")]
        public string Identity { get; set; }

        [JsonProperty("XErr")]
        public string XErr { get; set; }

        [JsonProperty("Identity")]
        public string Message { get; set; }

        [JsonProperty("Redirect")]
        public string Redirect { get; set; }
    }

    public class XSTSAuthenticatePropertiesModels
    {
        [JsonProperty("SandboxId")]
        public string SandboxId { get; set; } = "RETAIL";

        [JsonProperty("UserTokens")]
        public List<string> UserTokens { get; set; } = new List<string>();
    }
}
