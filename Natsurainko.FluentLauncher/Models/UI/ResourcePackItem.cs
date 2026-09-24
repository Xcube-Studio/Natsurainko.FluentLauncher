using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Models.UI;

public partial class ResourcePackItem : ObservableObject
{
    public string AbsolutePath { get; private set; }
    public string FileName => Path.GetFileName(AbsolutePath);
    public string DisplayName { get; }
    public string Description { get; private set; } = string.Empty;
    public int PackFormat { get; private set; }
    public string? IconFilePath { get; private set; }
    public bool IsDirectory { get; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public ResourcePackItem(string path)
    {
        AbsolutePath = path;
        IsDirectory = Directory.Exists(path);

        bool disabled = path.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
        IsEnabled = !disabled;

        string cleanName = disabled ? path[..^9] : path;
        DisplayName = Path.GetFileNameWithoutExtension(cleanName);

        ParseMetadata();
    }

    private void ParseMetadata()
    {
        try
        {
            if (IsDirectory)
            {
                string mcmetaPath = Path.Combine(AbsolutePath, "pack.mcmeta");
                if (File.Exists(mcmetaPath))
                {
                    string json = File.ReadAllText(mcmetaPath);
                    ParseMcmetaJson(json);
                }

                string iconPath = Path.Combine(AbsolutePath, "pack.png");
                if (File.Exists(iconPath))
                    IconFilePath = iconPath;
            }
            else
            {
                using var archive = ZipFile.OpenRead(AbsolutePath);
                var mcmetaEntry = archive.GetEntry("pack.mcmeta");
                if (mcmetaEntry != null)
                {
                    using var stream = mcmetaEntry.Open();
                    using var reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    ParseMcmetaJson(json);
                }

                var iconEntry = archive.GetEntry("pack.png");
                if (iconEntry != null)
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "FluentLauncher", "ResourcePackIcons");
                    Directory.CreateDirectory(tempDir);
                    string tempIconPath = Path.Combine(tempDir, $"{DisplayName}_{Path.GetFileName(AbsolutePath)}.png");

                    if (!File.Exists(tempIconPath))
                        iconEntry.ExtractToFile(tempIconPath, true);

                    IconFilePath = tempIconPath;
                }
            }
        }
        catch { }
    }

    private void ParseMcmetaJson(string json)
    {
        try
        {
            var node = JsonNode.Parse(json)?["pack"];
            if (node != null)
            {
                PackFormat = node["pack_format"]?.GetValue<int>() ?? 0;

                var descNode = node["description"];
                if (descNode != null)
                {
                    if (descNode is JsonValue val)
                        Description = val.ToString();
                    else if (descNode is JsonObject obj && obj["text"] != null)
                        Description = obj["text"]!.ToString();
                    else
                        Description = descNode.ToString();
                }
            }
        }
        catch { }
    }

    public bool Switch(bool enable)
    {
        try
        {
            string parent = Path.GetDirectoryName(AbsolutePath)!;
            string newPath;

            if (enable)
            {
                newPath = AbsolutePath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                    ? AbsolutePath[..^9]
                    : AbsolutePath;
            }
            else
            {
                newPath = AbsolutePath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                    ? AbsolutePath
                    : AbsolutePath + ".disabled";
            }

            if (newPath != AbsolutePath)
            {
                if (IsDirectory)
                    Directory.Move(AbsolutePath, newPath);
                else
                    File.Move(AbsolutePath, newPath);

                AbsolutePath = newPath;
                IsEnabled = enable;
                return true;
            }
        }
        catch { }

        return false;
    }

    public void Delete()
    {
        if (IsDirectory)
            Directory.Delete(AbsolutePath, true);
        else
            File.Delete(AbsolutePath);
    }
}
