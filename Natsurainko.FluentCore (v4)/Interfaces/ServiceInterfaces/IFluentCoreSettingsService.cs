using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
