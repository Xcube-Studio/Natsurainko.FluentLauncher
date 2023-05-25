using AppSettingsManagement;
using Natsurainko.FluentCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Settings;

partial class SettingsService : SettingsContainer
{
    [SettingItem(typeof(string), "CurrentGameCore", Default = "")]
    [SettingItem(typeof(string), "CurrentGameFolder", Default = "")]
    [SettingItem(typeof(string), "CurrentJavaRuntime", Default = "")]
    //[SettingsCollection(typeof(string), "GameFolders")]
    //[SettingsCollection(typeof(string), "JavaRuntimes")]
    [SettingItem(typeof(int), "JavaVirtualMachineMemory", Default = 1024)]
    [SettingItem(typeof(bool), "EnableAutoMemory", Default = true)]
    [SettingItem(typeof(bool), "EnableAutoJava", Default = true)]
    [SettingItem(typeof(bool), "EnableFullScreen", Default = false)]
    [SettingItem(typeof(bool), "EnableIndependencyCore", Default = false)]
    [SettingItem(typeof(string), "GameServerAddress", Default = "")]
    [SettingItem(typeof(int), "GameWindowHeight", Default = 480)]
    [SettingItem(typeof(int), "GameWindowWidth", Default = 854)]
    [SettingItem(typeof(string), "GameWindowTitle", Default = "")]
    //[SettingsCollection(typeof(IAccount), "Accounts")]
    [SettingItem(typeof(bool), "EnableDemoUser", Default = false)]
    [SettingItem(typeof(bool), "AutoRefresh", Default = true)]
    [SettingItem(typeof(bool), "UseDeviceFlowAuth", Default = false)]
    [SettingItem(typeof(string), "CurrentDownloadSource", Default = "Mcbbs")]
    [SettingItem(typeof(bool), "EnableFragmentDownload", Default = true)]
    [SettingItem(typeof(int), "MaxDownloadThreads", Default = 128)]
    [SettingItem(typeof(string), "CurrentLanguage", Default = "en-US, English")] // TODO: remove default value; set to system language if null
    [SettingItem(typeof(int), "AppWindowHeight", Default = 500)]
    [SettingItem(typeof(int), "AppWindowWidth", Default = 950)]
    [SettingItem(typeof(bool), "FinishGuide", Default = false)]
    public SettingsService(ISettingsStorage storage) : base(storage)
    {

    }
}
