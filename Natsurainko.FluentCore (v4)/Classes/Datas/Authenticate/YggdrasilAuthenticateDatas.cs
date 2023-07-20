using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Authenticate;

public class LoginRequestModel
{
    [JsonPropertyName("agent")]
    public Agent Agent { get; set; } = new Agent();

    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("requestUser")]
    public bool RequestUser { get; set; } = true;

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
}

public class YggdrasilRequestModel
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
}

public class Agent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Minecraft";

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
}

public class YggdrasilResponseModel
{
    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("availableProfiles")]
    public IEnumerable<ProfileModel> AvailableProfiles { get; set; }

    [JsonPropertyName("selectedProfile")]
    public ProfileModel SelectedProfile { get; set; }
}

public class ErrorResponseModel
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("cause")]
    public string Cause { get; set; }
}

public class User
{
    [JsonPropertyName("properties")]
    public IEnumerable<PropertyModel> Properties { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public class PropertyModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public class ProfileModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}