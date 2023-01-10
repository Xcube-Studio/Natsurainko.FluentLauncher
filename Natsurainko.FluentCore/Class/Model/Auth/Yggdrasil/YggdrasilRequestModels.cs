using Newtonsoft.Json;

namespace Natsurainko.FluentCore.Class.Model.Auth.Yggdrasil;

public class LoginRequestModel : BaseYggdrasilRequestModel
{
    [JsonProperty("agent")]
    public Agent Agent { get; set; } = new Agent();

    [JsonProperty("username")]
    public string UserName { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("requestUser")]
    public bool RequestUser { get; set; } = true;
}

public class YggdrasilRequestModel : BaseYggdrasilRequestModel
{
    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }
}

public class Agent
{
    [JsonProperty("name")]
    public string Name { get; set; } = "Minecraft";

    [JsonProperty("version")]
    public int Version { get; set; } = 1;
}

public class BaseYggdrasilRequestModel
{
    [JsonProperty("clientToken")]
    public string ClientToken { get; set; }
}