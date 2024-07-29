using System;

#nullable disable
namespace Natsurainko.FluentLauncher.Experimental.Saves;

internal record SaveInfo
{
    public string Folder {  get; set; }

    public string FolderName { get; set; }

    public string LevelName { get; set; }

    public string Version { get; set; }

    public bool AllowCommands {  get; set; }

    public DateTime LastPlayed { get; set; }

    public long Seed { get; set; } 

    public string IconFilePath { get; set; }

    public int GameType { get; set; }
}


