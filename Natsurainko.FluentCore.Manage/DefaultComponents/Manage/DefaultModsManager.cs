using Nrk.FluentCore.Classes.Datas;
using Nrk.FluentCore.Components.Manage;
using Nrk.FluentCore.DefaultComponents.Parse;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nrk.FluentCore.DefaultComponents.Manage;

public class DefaultModsManager : BaseModsManager
{
    private readonly List<(Exception, string)> _errorMods = new();
    public List<(Exception, string)> ErrorMods => _errorMods;

    public DefaultModsManager(string modsFolder) : base(modsFolder) { }

    public override IEnumerable<ModInfo> EnumerateMods()
    {
        foreach (var file in Directory.EnumerateFiles(_modsFolder))
        {
            var fileExtension = Path.GetExtension(file);

            if (!(fileExtension.Equals(".jar") || fileExtension.Equals(".disabled"))) continue;

            ModInfo modInfo = default;

            try { modInfo = DefaultModInfoParser.Parse(file); }
            catch (Exception ex)
            {
                _errorMods.Add((ex, file));

                modInfo = new ModInfo
                {
                    AbsolutePath = file,
                    DisplayName = Path.GetFileNameWithoutExtension(file),
                    IsEnabled = Path.GetExtension(file).Equals(".jar")
                };
            }

            yield return modInfo;
        }
    }

    public override void Delete(ModInfo modInfo) => File.Delete(modInfo.AbsolutePath);

    public override void Switch(ModInfo modInfo, bool isEnable)
    {
        var rawFilePath = modInfo.AbsolutePath;

        modInfo.IsEnabled = isEnable;
        modInfo.AbsolutePath = Path.Combine(Path.GetDirectoryName(rawFilePath), Path.GetFileNameWithoutExtension(rawFilePath) + (isEnable ? ".jar" : ".disabled"));

        File.Move(rawFilePath, modInfo.AbsolutePath);
    }
}
