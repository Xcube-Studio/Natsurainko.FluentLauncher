﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    private readonly static JsonSerializerOptions JsonSerializerOptions = new()
    {
        IncludeFields = false,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [JsonIgnore]
    public string FilePath { get; set; }

    [ObservableProperty]
    private string nickName;

    [ObservableProperty]
    private bool enableSpecialSetting;

    [ObservableProperty]
    private bool enableIndependencyCore;

    [ObservableProperty]
    private bool enableFullScreen;

    [ObservableProperty]
    private int gameWindowWidth = 854;

    [ObservableProperty]
    private int gameWindowHeight = 480;

    [ObservableProperty]
    private string serverAddress;

    [ObservableProperty]
    private string gameWindowTitle;

    [ObservableProperty]
    private Account account;

    [ObservableProperty]
    private bool enableTargetedAccount;

    [ObservableProperty]
    private IEnumerable<string> vmParameters;


    private DateTime? lastLaunchTime;

    public DateTime? LastLaunchTime
    {
        get => lastLaunchTime;
        set
        {
            App.DispatcherQueue.TryEnqueue(() => SetProperty(ref lastLaunchTime, value));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!string.IsNullOrEmpty(FilePath))
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonSerializerOptions));
    }
}
