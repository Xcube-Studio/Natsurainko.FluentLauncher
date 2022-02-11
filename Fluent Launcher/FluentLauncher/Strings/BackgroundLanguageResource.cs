using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Strings
{
    public class BackgroundLanguageResource
    { 
        public BackgroundLanguageResource(string language)
        {
            switch (language)
            {
                case "中文":
                    App_Exception = "应用程序未捕获的异常";
                    MainPage_LaunchButton_SubTitle = "未选中核心";
                    Background_SetSuitableJavaRuntime_False = "我们无法找到一个合适的 Java 运行时 来启动当前游戏，请前往 基础设置页面 进行添加";
                    Background_SetSuitableJavaRuntime_True = "当前游戏需要 Java 运行时 版本 {major} 以上，已自动切换 Java 运行时";
                    Background_SearchJava_Error = "无法在你的计算机中找到Java运行时，请手动选择一个";
                    Background_MakeScript_Successfully = "成功生成启动脚本";
                    break;
                case "English":
                    App_Exception = "Application Unhandled Exception";
                    MainPage_LaunchButton_SubTitle = "No Core Selected";
                    Background_SetSuitableJavaRuntime_False = "We were unable to find a suitable Java runtime to start the current game, please go to the basic settings page to add it";
                    Background_SetSuitableJavaRuntime_True = "Current game requires Java Runtime version {major}+, Java runtime switched automatically";
                    Background_SearchJava_Error = "Unable to find Java Runtime in your computer, please select one manually";
                    Background_MakeScript_Successfully = "Make the launch script successfully";
                    break;
                default:
                    break;
            }
        }

        public string App_Exception { get; private set; }

        public string MainPage_LaunchButton_SubTitle { get; private set; }

        public string Background_SetSuitableJavaRuntime_False { get; private set; }

        public string Background_SetSuitableJavaRuntime_True { get; private set; }

        public string Background_SearchJava_Error { get; private set; }

        public string Background_MakeScript_Successfully { get; private set; }
    }
}
