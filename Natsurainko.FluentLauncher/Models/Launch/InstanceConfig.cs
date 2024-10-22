using CommunityToolkit.Mvvm.ComponentModel;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable
namespace Natsurainko.FluentLauncher.Models.Launch;

internal partial class InstanceConfig : ObservableObject
{
    [JsonIgnore]
    public string FilePath { get; set; }

    [ObservableProperty]
    private string nickName;

    private bool enableSpecialSetting;
    public bool EnableSpecialSetting
    {
        get => enableSpecialSetting;
        set => SetProperty(ref enableSpecialSetting, value);
    }

    private bool enableIndependencyCore;
    public bool EnableIndependencyCore
    {
        get => enableIndependencyCore;
        set => SetProperty(ref enableIndependencyCore, value);
    }

    private bool enableFullScreen;
    public bool EnableFullScreen
    {
        get => enableFullScreen;
        set => SetProperty(ref enableFullScreen, value);
    }

    private int gameWindowWidth = 854;
    public int GameWindowWidth
    {
        get => gameWindowWidth;
        set => SetProperty(ref gameWindowWidth, value);
    }

    private int gameWindowHeight = 480;
    public int GameWindowHeight
    {
        get => gameWindowHeight;
        set => SetProperty(ref gameWindowHeight, value);
    }

    private string serverAddress;
    public string ServerAddress
    {
        get => serverAddress;
        set => SetProperty(ref serverAddress, value);
    }

    private string gameWindowTitle;
    public string GameWindowTitle
    {
        get => gameWindowTitle;
        set => SetProperty(ref gameWindowTitle, value);
    }

    private Account account;
    public Account Account
    {
        get => account;
        set => SetProperty(ref account, value);
    }

    private bool enableTargetedAccount;
    public bool EnableTargetedAccount
    {
        get => enableTargetedAccount;
        set => SetProperty(ref enableTargetedAccount, value);
    }

    private IEnumerable<string> vmParameters;
    public IEnumerable<string> VmParameters
    {
        get => vmParameters;
        set => SetProperty(ref vmParameters, value);
    }

    private DateTime? lastLaunchTime;
    public DateTime? LastLaunchTime
    {
        get => lastLaunchTime;
        set => App.DispatcherQueue.TryEnqueue(() => SetProperty(ref lastLaunchTime, value));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!string.IsNullOrEmpty(FilePath))
        {
            var content = JsonSerializer.Serialize(this, InstanceConfigSerializerContext.Default.InstanceConfig);
            File.WriteAllText(FilePath, content);
        }
    }
}
