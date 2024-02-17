using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Utils;

internal static class JsonNodeUtils
{
    public static JsonNode ParseFile(string filePath)
    {
        JsonNode? jsonNode = null;
        try
        {
            jsonNode = JsonNode.Parse(File.ReadAllText(filePath));
        }
        catch (JsonException) { }

        // jsonNode is null if either
        // - the file is not a valid json file, or
        // - the file contains the string "null" and JsonNode.Parse(string) returns null.
        if (jsonNode is null)
            throw new InvalidDataException($"Error in parsing the json file {filePath}.");

        return jsonNode;
    }
}
