using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace Natsurainko.FluentLauncher.Models.UI;

public partial class ShaderpackItem : ObservableObject
{
    public string AbsolutePath { get; private set; }
    public string FileName => Path.GetFileName(AbsolutePath);
    public string DisplayName { get; }
    public bool IsDirectory { get; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public ShaderpackItem(string path)
    {
        AbsolutePath = path;
        IsDirectory = Directory.Exists(path);

        bool disabled = path.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
        IsEnabled = !disabled;

        string cleanName = disabled ? path[..^9] : path;
        DisplayName = Path.GetFileNameWithoutExtension(cleanName);
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
