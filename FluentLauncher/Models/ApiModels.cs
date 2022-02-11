using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Models
{
    public class AbstractModLoader
    {
        public string McVersion { get; set; }

        public string LoaderVersion { get; set; }

        public string Type { get; set; }

        public object Build { get; set; }
    }

    public class ForgeBuild
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("build")]
        public int Build { get; set; }

        [JsonProperty("mcversion")]
        public string McVersion { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public class OptiFineBuild
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("mcversion")]
        public string McVersion { get; set; }

        [JsonProperty("patch")]
        public string Patch { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }
    }

    public class RromosForgeItem
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("build")]
        public ForgeBuild Build { get; set; }
    }
}
