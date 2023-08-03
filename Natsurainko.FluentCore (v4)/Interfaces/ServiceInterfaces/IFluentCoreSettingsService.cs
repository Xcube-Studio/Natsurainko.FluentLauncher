using Nrk.FluentCore.Classes.Datas.Launch;
using System.Collections.ObjectModel;

namespace Nrk.FluentCore.Interfaces.ServiceInterfaces;

/// <summary>
/// 设置服务接口
/// </summary>
public interface IFluentCoreSettingsService
{
    public string ActiveMinecraftFolder { get; set; }

    public ObservableCollection<string> MinecraftFolders { get; }

    public string ActiveJava { get; set; }

    public ObservableCollection<string> Javas { get; }

    public int JavaMemory { get; set; }

    public GameInfo ActiveGameInfo { get; set; }

    public bool EnableDemoUser { get; set; }
}
