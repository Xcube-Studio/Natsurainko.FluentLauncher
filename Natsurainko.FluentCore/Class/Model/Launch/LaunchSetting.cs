using Natsurainko.FluentCore.Class.Model.Auth;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentCore.Class.Model.Launch;

public class LaunchSetting
{
    public DirectoryInfo NativesFolder { get; set; }

    public DirectoryInfo WorkingFolder { get; set; }

    public Account Account { get; set; }

    public JvmSetting JvmSetting { get; set; }

    public GameWindowSetting GameWindowSetting { get; set; } = new GameWindowSetting();

    public ServerSetting ServerSetting { get; set; }

    public XmlOutputSetting XmlOutputSetting { get; set; } = new XmlOutputSetting();

    public bool IsDemoUser { get; set; } = false;

    public bool EnableIndependencyCore { get; set; } = false;

    public LaunchSetting() { }

    public LaunchSetting(JvmSetting jvmSetting)
    {
        this.JvmSetting = jvmSetting;
    }
}

public class JvmSetting
{
    public FileInfo Javaw { get; set; }

    public int MaxMemory { get; set; } = 2048;

    public int MinMemory { get; set; } = 512;

    public IEnumerable<string> AdvancedArguments { get; set; }

    public IEnumerable<string> GCArguments { get; set; }

    public JvmSetting() { }

    public JvmSetting(string file) => this.Javaw = new FileInfo(file);

    public JvmSetting(FileInfo fileInfo) => this.Javaw = fileInfo;
}

public class GameWindowSetting
{
    public int Width { get; set; } = 854;

    public int Height { get; set; } = 480;

    public bool IsFullscreen { get; set; } = false;
}

public class ServerSetting
{
    public string IPAddress { get; set; }

    public int Port { get; set; }
}

public class XmlOutputSetting
{
    public bool Enable { get; set; } = false;

    public FileInfo LogConfigFile { get; set; }
}