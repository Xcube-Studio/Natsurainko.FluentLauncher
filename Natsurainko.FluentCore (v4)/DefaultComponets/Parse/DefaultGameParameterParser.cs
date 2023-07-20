using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Nrk.FluentCore.DefaultComponets.Parse;

public static class DefaultGameParameterParser
{
    public static IEnumerable<string> Parse(JsonNode jsonNode)
    {
        var jsonGame = jsonNode["arguments"]?["game"];
        var jsonMinecraftArguments = jsonNode["minecraftArguments"];

        if (jsonMinecraftArguments != null && !string.IsNullOrEmpty(jsonMinecraftArguments.GetValue<string>()))
            foreach (var arg in StringExtensions.ArgumnetsGroup(jsonMinecraftArguments.GetValue<string>().Split(' ')))
                yield return arg;

        if (jsonGame != null)
            foreach (var item in StringExtensions.ArgumnetsGroup(jsonGame.AsArray().Where(x => x is JsonValue).Select(x => x.GetValue<string>())))
                yield return item;
    }
}
