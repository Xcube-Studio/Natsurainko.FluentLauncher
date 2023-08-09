using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.DefaultComponents.Parse;

/// <summary>
/// 默认虚拟机参数解析器
/// </summary>
public static class DefaultVmParameterParser
{
    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="jsonNode"></param>
    /// <returns></returns>
    public static IEnumerable<string> Parse(JsonNode jsonNode)
    {
        var jsonJvm = jsonNode["arguments"]?["jvm"]?.AsArray();
        var jvm = new List<string>();

        if (jsonJvm == null)
        {
            yield return "-Djava.library.path=${natives_directory}";
            yield return "-Dminecraft.launcher.brand=${launcher_name}";
            yield return "-Dminecraft.launcher.version=${launcher_version}";
            yield return "-cp ${classpath}";

            yield break;
        }

        foreach (var item in jsonJvm)
        {
            if (item is JsonValue jsonValue)
            {
                var value = jsonValue.GetValue<string>().Trim();

                if (value.Contains(' '))
                    jvm.AddRange(value.Split(' '));
                else jvm.Add(value);
            }
        }

        foreach (var item in StringExtensions.ArgumnetsGroup(jvm))
            yield return item;

        if (!jvm.Contains("-cp"))
            yield return "-cp ${classpath}";
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
