using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Game
{
    public class Downloads
    {
        [JsonProperty("artifact")]
        public FileModel Artifact { get; set; }

        [JsonProperty("classifiers")]
        public Dictionary<string, FileModel> Classifiers { get; set; }
    }

    //public class Extract
    //{
    //    [JsonProperty("exclude")] 
    //    public List<string> Exclude { get; set; }
    //}

    public class RuleModel
    {
        [JsonProperty("action")]
        public string Action { get; set; }


        [JsonProperty("os")]
        public Dictionary<string, string> System { get; set; }

    }

    public class AssetIndex : FileModel
    {
        [JsonProperty("totalSize")]
        public int TotalSize { get; set; }
    }

    public class Client
    {
        [JsonProperty("argument")]
        public string Argument { get; set; }

        [JsonProperty("file")]
        public FileModel File { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Logging
    {
        [JsonProperty("client")]
        public Client Client { get; set; }
    }

    public class Arguments
    {
        [JsonProperty("game")]
        public List<object> Game { get; set; } = new List<object>();

        [JsonProperty("jvm")]
        public List<object> Jvm { get; set; } = new List<object>();
    }

    public class JavaVersion
    {
        [JsonProperty("component")]
        public string Component { get; set; }

        [JsonProperty("majorVersion")]
        public int MajorVersion { get; set; }
    }

    public class AuthenticationDataModel
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("properties")]
        public IEnumerable<PropertyModel> Properties { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
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

    public class LauncherVersionModel
    {
        [JsonProperty("format")]
        public int Format { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profilesFormat")]
        public int ProfilesFormat { get; set; }
    }

    public class SelectedUserModel
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }
    }

    public class AssetsObjectsModel
    {
        [JsonProperty("objects")]
        public Dictionary<string, Asset> Objects { get; set; }
    }
}
