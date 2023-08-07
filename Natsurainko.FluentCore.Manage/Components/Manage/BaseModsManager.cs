using Nrk.FluentCore.Classes.Datas;
using System.Collections.Generic;

namespace Nrk.FluentCore.Components.Manage;

public abstract class BaseModsManager
{
    protected readonly string _modsFolder;

    public BaseModsManager(string modsFolder)
    {
        _modsFolder = modsFolder;
    }

    public abstract IEnumerable<ModInfo> EnumerateMods();

    public abstract void Switch(ModInfo modInfo, bool isEnable);

    public abstract void Delete(ModInfo modInfo);
}
