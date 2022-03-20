using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.DesktopBridge
{
    public class MinecraftCoreInfo
    {
        public string Id { get; set; }

        public string Tag { get; set; }
    }

    public class LaunchModel
    {
        public string Uuid { get; set; }

        public string AccessToken { get; set; }

        public string UserName { get; set; }

        public string JavaPath { get; set; }

        public string GameFolder { get; set; }

        public bool IsIndependent { get; set; }

        public string Id { get; set; }

        public int MaximumMemory { get; set; } = 1024;

        public int? MinimumMemory { get; set; } = 512;
    }

    public class JavaRuntimeEnvironment
    {
        public string Title { get; set; } = "Java(TM) Platform SE Binary";

        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            var item = (JavaRuntimeEnvironment)obj;
            if (this.Title == item.Title && this.Path == item.Path)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode() ^ this.Path.GetHashCode();
        }
    }

    public class JreInfo
    {
        public string JAVA_VM_VERSION { get; set; }

        public string JAVA_VM_NAME { get; set; }

        public string JAVA_HOME { get; set; }

        public string JAVA_VENDOR { get; set; }

        public string JAVA_VM_VENDOR { get; set; }

        public string JAVA_VERSION { get; set; }
    }

    public class InstallInfomation
    {
        public string Folder { get; set; }

        public string McVersion { get; set; }

        public string JavaPath { get; set; }

        public string ModLoader { get; set; }
    }

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
}
