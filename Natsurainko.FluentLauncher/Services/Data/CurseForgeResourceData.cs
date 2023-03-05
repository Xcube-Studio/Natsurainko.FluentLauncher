using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentCore.Model.Mod.CureseForge;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Data;

internal partial class CurseForgeResourceData
{
    public CurseForgeResource InnerData { get; }

    public string Name { get; }

    public string Description { get; }

    public string Authors { get; }

    public DateTime UpdateTime { get; }

    public string DownloadCount { get; }

    public int Id { get; }

    public string Icon { get; }

    public CurseForgeResourceData(CurseForgeResource innerData, string name, string description, string authors, DateTime updateTime, string downloadCount, int id, string icon)
    {
        InnerData = innerData;
        Name = name;
        Description = description;
        Authors = authors;
        UpdateTime = updateTime;
        DownloadCount = downloadCount;
        Id = id;
        Icon = icon;
    }

    [RelayCommand]
    public async void Open()
    {
        var dialog = new CurseForgeModDialog();
        dialog.DataContext = this;
        dialog.XamlRoot = MainContainer._XamlRoot;

        await dialog.ShowAsync();
    }
}
