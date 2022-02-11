using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.DesktopBridge
{
    public class LanguageResource
    {
        public LanguageResource(string language)
        {
            switch (language)
            {
                case "中文":
                    Launching_Info_1 = "准备启动参数";
                    Launching_Info_2 = "检查和补全依赖";
                    Launching_Info_3 = "等待游戏启动";
                    Launching_DownloadInfo_1 = "{0} 已下载 {1}/{2}";
                    Launching_DownloadInfo_2 = "依赖可能没有完全补全";
                    InstallJava_Downloding = "正在下载必要的文件..";
                    InstallJava_Extracting = "正在解压文件..";
                    break;
                case "English":
                    Launching_Info_1 = "Preparing Startup Parameters";
                    Launching_Info_2 = "Check Dependences and Complete";
                    Launching_Info_3 = "Waiting for Minecraft to start";
                    Launching_DownloadInfo_1 = "{0} Downloaded {1}/{2}";
                    Launching_DownloadInfo_2 = "There may be some dependencies that are not fully completed";
                    InstallJava_Downloding = "Downloading the required files..";
                    InstallJava_Extracting= "Extracting files..";
                    break;
                default:
                    break;
            }
        }

        public string Launching_Info_1 { get; private set; }

        public string Launching_Info_2 { get; private set; }

        public string Launching_Info_3 { get; private set; }

        public string Launching_DownloadInfo_1 { get; private set; }

        public string Launching_DownloadInfo_2 { get; private set; }

        public string InstallJava_Downloding { get; private set; }

        public string InstallJava_Extracting { get; private set; }

    }
}
