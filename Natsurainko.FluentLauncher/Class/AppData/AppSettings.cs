using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Class.Model.Launch;
using System.Collections.Generic;
using Language = Natsurainko.FluentLauncher.Class.ViewData.Language;

namespace Natsurainko.FluentLauncher.Class.AppData;

public class AppSettings
{
    public Language CurrentLanguage { get; set; } = GlobalResources.SupportedLanguages[0];

    #region Global Launch Setting

    public GameCore CurrentGameCore { get; set; }

    public List<string> GameFolders { get; set; } = new();

    public string CurrentGameFolder { get; set; }

    public List<string> JavaRuntimes { get; set; } = new();

    public string CurrentJavaRuntime { get; set; }

    public int? JavaVirtualMachineMemory { get; set; } = 1024;

    public bool? EnableAutoMemory { get; set; } = false;

    public string GameWindowTitle { get; set; } = string.Empty;

    public int? GameWindowWidth { get; set; } = 854;

    public int? GameWindowHeight { get; set; } = 480;

    public string GameServerAddress { get; set; } = string.Empty;

    public bool? EnableFullScreen { get; set; } = false;

    public bool? EnableIndependencyCore { get; set; } = false;

    #endregion

    #region Account Setting

    public List<Account> Accounts { get; set; } = new() { Account.Default };

    public Account CurrentAccount { get; set; } = Account.Default;

    public bool? EnableDemoUser { get; set; } = false;

    #endregion

    #region Download Setting

    public string CurrentDownloadSource { get; set; } = GlobalResources.DownloadSources[2];

    public int? MaxDownloadThreads { get; set; } = 128;

    public bool? EnableFragmentDownload { get; set; } = false;

    #endregion

    #region CoresPage UI Setting

    public int? CoreSortBy { get; set; } = 0;

    public int? CoreVisibility { get; set; } = 0;

    #endregion

    #region HomePage UI Setting

    public int? ShowNews { get; set; } = 1;

    #endregion

    public static readonly AppSettings Default = new();
}
