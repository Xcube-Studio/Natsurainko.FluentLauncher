using FluentCore.Model.Game;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Install.Forge
{
    /// <summary>
    /// Forge安装概述模型
    /// </summary>
    public class ForgeInstallProfileModel
    {
        [JsonProperty("_comment_")]
        public List<string> Comments { get; set; }

        [JsonProperty("spec")]
        public int Spec { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("minecraft")]
        public string Minecraft { get; set; }

        [JsonProperty("serverJarPath")]
        public string ServerJarPath { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, ForgeInstallDataModel> Data { get; set; }

        [JsonProperty("processors")]
        public List<ForgeInstallProcessorModel> Processors { get; set; }

        [JsonProperty("libraries")]
        public List<Library> Libraries { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("json")]
        public string Json { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("mirrorList")]
        public string MirrorList { get; set; }

        [JsonProperty("welcome")]
        public string Welcome { get; set; }
    }
}
