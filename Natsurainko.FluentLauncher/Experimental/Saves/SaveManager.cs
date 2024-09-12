using System;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentLauncher.Experimental.Saves;

internal class SaveManager
{
    private readonly List<(FileInfo, Exception)> _errorLevelData = [];

    public IReadOnlyList<(FileInfo, Exception)> ErrorLevelData { get; init; }

    public string SavesFolder { get; private set; }

    public SaveManager(string savesFolder)
    {
        SavesFolder = savesFolder;
        ErrorLevelData = _errorLevelData;
    }

    public async IAsyncEnumerable<SaveInfo> EnumerateSavesAsync()
    {
        if (!Directory.Exists(SavesFolder))
            yield break;

        foreach (var dir in Directory.EnumerateDirectories(SavesFolder))
        {
            var levelDataFile = new FileInfo(Path.Combine(dir, "level.dat"));

            if (!levelDataFile.Exists)
                continue;

            SaveInfo? saveInfo = default;

            try
            {
                saveInfo = await SaveInfoParser.ParseAsync(dir);
            }
            catch (Exception ex)
            {
                _errorLevelData.Add((levelDataFile, ex));
            }

            if (saveInfo != null)
                yield return saveInfo;
        }
    }
}
