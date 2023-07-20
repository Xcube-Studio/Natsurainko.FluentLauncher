using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Nrk.FluentCore.DefaultComponets.Parse;

/// <summary>
/// 默认虚拟机参数解析器
/// </summary>
public static class DefaultVmParameterParser
{
    public static IEnumerable<string> Parse(JsonNode jsonNode)
    {
        var jsonJvm = jsonNode["arguments"]?["jvm"];

        if (jsonJvm == null)
        {
            yield return "-Djava.library.path=${natives_directory}";
            yield return "-Dminecraft.launcher.brand=${launcher_name}";
            yield return "-Dminecraft.launcher.version=${launcher_version}";
            yield return "-cp ${classpath}";

            yield break;
        }

        foreach (var item in StringExtensions. ArgumnetsGroup(jsonJvm.AsArray().Where(x => x is JsonValue).Select(x => x.GetValue<string>())))
            yield return item;
    }

    /// <summary>
    /// 获取虚拟机环境参数
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetEnvironmentJVMArguments()
    {
        switch (EnvironmentUtils.PlatformName)
        {
            case "windows":
                yield return "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump";
                if (Environment.OSVersion.Version.Major == 10)
                {
                    yield return "-Dos.name=\"Windows 10\"";
                    yield return "-Dos.version=10.0";
                }
                break;
            case "osx":
                yield return "-XstartOnFirstThread";
                break;
        }

        if (EnvironmentUtils.SystemArch == "32")
            yield return "-Xss1M";
    }
}
