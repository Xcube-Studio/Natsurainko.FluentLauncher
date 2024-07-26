using NbtToolkit.Binary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Experimental.Saves;

internal static class SaveInfoParser
{
    public static async Task<SaveInfo> ParseAsync(string saveFolder)
    {
        var saveInfo = new SaveInfo
        {
            FolderName = new DirectoryInfo(saveFolder).Name,
            Folder = saveFolder
        };

        await Task.Run(() => 
        {
            using var fileStream = File.OpenRead(Path.Combine(saveFolder, "level.dat"));
            using var nbtReader = new NbtReader(fileStream, NbtCompression.GZip);

            var rootTag = nbtReader.ReadRootTag();
            var dataTagCompound = rootTag["Data"].AsTagCompound();

            saveInfo.LevelName = dataTagCompound["LevelName"].AsString();
            saveInfo.AllowCommands = dataTagCompound["allowCommands"].AsBool();
            saveInfo.GameType = dataTagCompound["GameType"].AsInt();
            saveInfo.Version = dataTagCompound["Version"].AsTagCompound()["Name"].AsString();
            saveInfo.Seed = dataTagCompound["WorldGenSettings"].AsTagCompound()["seed"].AsLong();

            saveInfo.LastPlayed = DateTimeOffset.FromUnixTimeMilliseconds(dataTagCompound["LastPlayed"].AsLong()).ToLocalTime().DateTime;

            if (File.Exists(Path.Combine(saveFolder, "icon.png"))) 
                saveInfo.IconFilePath = Path.Combine(saveFolder, "icon.png");
        });

        return saveInfo;
    }
}
