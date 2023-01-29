using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentCore.Model.Mod.CureseForge;
using Natsurainko.FluentCore.Module.Mod;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Natsurainko.FluentLauncher.Views.Pages;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Mods;

public partial class CurseForge : ObservableObject
{
    public static IEnumerable<Category> Categories { get; private set; }

    public static IEnumerable<string> Versions { get; private set; }

    public static IEnumerable<Resource> FeatruedResources { get; private set; }

    public CurseForge()
    {
        Task.Run(async () =>
        {
            if (Categories == null)
                Categories = (await CurseForgeApi.GetCategoriesMain()).ToList().Select(x => new Category(x));

            App.MainWindow.DispatcherQueue.TryEnqueue(() => CurseForgeCategories = new(Categories));

            if (Versions == null)
                Versions = new string[] { "All" }.Union(await CurseForgeApi.GetMinecraftVersions());

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                CurseForgeVersions = new(Versions);
                SelectedVersion = CurseForgeVersions[0];
            });

            if (FeatruedResources == null)
                FeatruedResources = (await CurseForgeApi.GetFeaturedResources()).ToList().Select(x => new Resource(x));

            App.MainWindow.DispatcherQueue.TryEnqueue(() => Resources = new(FeatruedResources));
        });
    }

    public partial class Category : ObservableObject
    {
        public Category(CurseForgeCategory category)
        {
            Name = category.Name;
            Id = category.Id;
            Uri = category.IconUrl;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string uri;
    }

    public partial class Resource : ObservableObject
    {
        public Resource(CurseForgeResource resource)
        {
            Data = resource;
            Name = resource.Name;
            Id = resource.Id;
            Description = resource.Summary;
            DownloadCount = resource.DownloadCount.FormatUnit();
            UpdateTime = resource.DateModified;
            Authors = string.Join(", ", resource.Author.Select(x => x.Name));
            Icon = resource.Logo.Url;
        }

        public CurseForgeResource Data { get; private set; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string authors;

        [ObservableProperty]
        private DateTime updateTime;

        [ObservableProperty]
        private string downloadCount;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string icon;

        [RelayCommand]
        public async void Open()
        {
            var dialog = new CurseForgeModDialog();
            dialog.DataContext = this;
            dialog.XamlRoot = MainContainer._XamlRoot;

            await dialog.ShowAsync();
        }
    }

    [ObservableProperty]
    private ObservableCollection<Resource> resources;

    [ObservableProperty]
    private ObservableCollection<Category> curseForgeCategories;

    [ObservableProperty]
    private Category selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> curseForgeVersions;

    [ObservableProperty]
    private string selectedVersion;
}
