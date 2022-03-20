using FluentCore.Interface;
using FluentCore.Model.Launch;
using FluentCore.Service.Local;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FluentCore.Service.Component.Launch
{
    /// <summary>
    /// 参数构造器
    /// </summary>
    public class ArgumentsBuilder : IArgumentsBuilder
    {
        public ArgumentsBuilder(GameCore core, LaunchConfig config)
        {
            if (core == null || config == null)
                throw new ArgumentNullException(nameof(core));

            this.GameCore = core;
            this.LaunchConfig = config;
        }

        /// <summary>
        /// 启动配置信息
        /// </summary>
        public LaunchConfig LaunchConfig { get; set; }

        /// <summary>
        /// 游戏核心
        /// </summary>
        public GameCore GameCore { get; set; }

        /// <summary>
        /// 分隔符
        /// </summary>
        public static readonly string Separator = SystemConfiguration.Platform == OSPlatform.Windows ? ";" : ":";

        public string BulidArguments(bool withJavaPath = false)
        {
            var stringBuilder = new StringBuilder();
            if (withJavaPath)
                stringBuilder.Append(LaunchConfig.JavaPath.Contains(" ") ? $"\"{LaunchConfig.JavaPath}\"" : LaunchConfig.JavaPath);

            stringBuilder.Append(this.GetFrontArguments());
            stringBuilder.Append($" {this.GameCore.MainClass}");
            stringBuilder.Append(this.GetBehindArguments());
            return stringBuilder.ToString().Replace("  ", " ").Trim();
        }

        public string GetFrontArguments()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($" -Xmx{this.LaunchConfig.MaximumMemory}M");
            stringBuilder.Append(this.LaunchConfig.MinimumMemory.HasValue ? $" -Xmn{this.LaunchConfig.MinimumMemory}M" : string.Empty);

            this.LaunchConfig.MoreFrontArgs += "-Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true";
            if (!string.IsNullOrEmpty($" {this.LaunchConfig.MoreFrontArgs}"))
                stringBuilder.Append($" {this.LaunchConfig.MoreFrontArgs}");

            stringBuilder.Append(GetEnvironmentJVMArguments());

            //stringBuilder.Append($" {this.GameCore.Logging.Client.Argument}");
            stringBuilder.Append($" {this.GameCore.FrontArguments}");
            if (string.IsNullOrEmpty(this.GameCore.FrontArguments))
            {
                stringBuilder.Append(" -Djava.library.path=${natives_directory}");
                stringBuilder.Append(" -Dminecraft.launcher.brand=${launcher_name}");
                stringBuilder.Append(" -Dminecraft.launcher.version=${launcher_version}");
                stringBuilder.Append(" -cp ${classpath}");
            };

            //stringBuilder.Replace("${path}", this.GameCore.Root.Contains(" ") ? $"\"{PathHelper.GetLogConfigsFolder(this.GameCore.Root)}{PathHelper.X}{this.GameCore.Logging.Client.File.Id}\"" : $"{PathHelper.GetLogConfigsFolder(this.GameCore.Root)}{PathHelper.X}{this.GameCore.Logging.Client.File.Id}");
            stringBuilder.Replace("${natives_directory}", this.LaunchConfig.NativesFolder.Contains(" ") ? $"\"{LaunchConfig.NativesFolder}\"" : LaunchConfig.NativesFolder);
            stringBuilder.Replace("${launcher_name}", "Fluent.Core");
            stringBuilder.Replace("${launcher_version}", "3");
            stringBuilder.Replace("${classpath_separator}", Separator);
            stringBuilder.Replace("${classpath}", GetClasspath());

            return stringBuilder.ToString().Replace("  ", " ");
        }

        public string GetBehindArguments()
        {
            var stringBuilder = new StringBuilder();

            string assetsPath = PathHelper.GetAssetsFolder(this.GameCore.Root);
            LaunchConfig.WorkingFolder = string.IsNullOrEmpty(LaunchConfig.WorkingFolder) || !Directory.Exists(LaunchConfig.WorkingFolder) 
                ? this.GameCore.Root : LaunchConfig.WorkingFolder;

            stringBuilder.Append($" {this.GameCore.BehindArguments}");

            stringBuilder.Replace("${auth_player_name}", this.LaunchConfig.AuthDataModel.UserName);
            stringBuilder.Replace("${version_name}", this.GameCore.Id);
            stringBuilder.Replace("${game_directory}", LaunchConfig.WorkingFolder.Contains(" ") ? $"\"{LaunchConfig.WorkingFolder}\"" : LaunchConfig.WorkingFolder);
            stringBuilder.Replace("${assets_root}", assetsPath.Contains(" ") ? $"\"{assetsPath}\"" : assetsPath);
            stringBuilder.Replace("${assets_index_name}", this.GameCore.AsstesIndex.Id);
            stringBuilder.Replace("${auth_uuid}", this.LaunchConfig.AuthDataModel.Uuid.ToString("N"));
            stringBuilder.Replace("${auth_access_token}", this.LaunchConfig.AuthDataModel.AccessToken);
            stringBuilder.Replace("${user_type}", "Mojang");
            stringBuilder.Replace("${version_type}", this.GameCore.Type);

            stringBuilder.Replace("${user_properties}", "{}");

            //Legacy Minecraft
            stringBuilder.Replace("${game_assets}", assetsPath.Contains(" ") ? $"\"{assetsPath}{PathHelper.X}virtual{PathHelper.X}legacy\"" : $"{assetsPath}{PathHelper.X}virtual{PathHelper.X}legacy");
            stringBuilder.Replace("${auth_session}", this.LaunchConfig.AuthDataModel.AccessToken);

            if (!string.IsNullOrEmpty($" {this.LaunchConfig.MoreBehindArgs}"))
                stringBuilder.Append($" {this.LaunchConfig.MoreBehindArgs}");

            return stringBuilder.ToString();
        }

        public string GetClasspath()
        {
            string Separator = SystemConfiguration.Platform == OSPlatform.Windows ? ";" : ":";
            var stringbuilder = new StringBuilder();

            foreach (var library in this.GameCore.Libraries)
                stringbuilder.Append($"{PathHelper.GetLibrariesFolder(this.GameCore.Root)}{PathHelper.X}{library.GetRelativePath()}{Separator}");

            if (ArgumentsBuilder.LoadMainClass(this.GameCore.MainClass))
                stringbuilder.Append(this.GameCore.MainJar);
            return stringbuilder.ToString().Contains(" ") ? $"\"{stringbuilder}\"" : stringbuilder.ToString();
        }

        /// <summary>
        /// 获取环境JVM参数
        /// </summary>
        /// <returns></returns>
        public static string GetEnvironmentJVMArguments()
        {
            var stringBuilder = new StringBuilder();

            switch (SystemConfiguration.PlatformName)
            {
                case "Windows":
                    stringBuilder.Append(" -XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
                    if (Environment.OSVersion.Version.Major == 10)
                    {
                        stringBuilder.Append(" -Dos.name=\"Windows 10\"");
                        stringBuilder.Append(" -Dos.version=10.0");
                    }
                    break;
                case "OSX":
                    stringBuilder.Append(" -XstartOnFirstThread");
                    break;
            }

            stringBuilder.Append(SystemConfiguration.Arch == "32" ? "-Xss1M" : string.Empty);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 判读是否加载主Jar
        /// </summary>
        /// <param name="mainClass">mainClass字符串</param>
        /// <returns></returns>
        public static bool LoadMainClass(string mainClass)
        {
            return mainClass switch
            {
                "cpw.mods.bootstraplauncher.BootstrapLauncher" => false,
                _ => true,
            };
        }
    }
}
