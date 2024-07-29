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
            var time = DateTime.Now;

            using var fileStream = new FileStream(Path.Combine(saveFolder, "level.dat"), FileMode.Open, FileAccess.Read);
            using var _nbtReader = new NbtReader(fileStream, NbtCompression.GZip, true);

            var rootTag = _nbtReader.ReadRootTag();

            var dataTagCompound = rootTag["Data"].AsTagCompound();

            saveInfo.LevelName = dataTagCompound["LevelName"].AsString();
            saveInfo.AllowCommands = dataTagCompound["allowCommands"].AsBool();
            saveInfo.GameType = dataTagCompound["GameType"].AsInt();
            saveInfo.Version = dataTagCompound["Version"].AsTagCompound()["Name"].AsString();

            if (dataTagCompound.ContainsKey("WorldGenSettings"))
                saveInfo.Seed = dataTagCompound["WorldGenSettings"].AsTagCompound()["seed"].AsLong();
            else if (dataTagCompound.ContainsKey("RandomSeed"))
                saveInfo.Seed = dataTagCompound["RandomSeed"].AsLong();

            saveInfo.LastPlayed = DateTimeOffset.FromUnixTimeMilliseconds(dataTagCompound["LastPlayed"].AsLong()).ToLocalTime().DateTime;

            if (File.Exists(Path.Combine(saveFolder, "icon.png"))) 
                saveInfo.IconFilePath = Path.Combine(saveFolder, "icon.png");

            var gap = DateTime.Now - time;
        });

        return saveInfo;
    }
}
