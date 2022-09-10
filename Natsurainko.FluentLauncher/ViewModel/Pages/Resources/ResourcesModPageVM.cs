using Natsurainko.FluentCore.Class.Model.Mod;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Resources;

public class ResourcesModPageVM : ViewModelBase<Page>
{
    public ResourcesModPageVM(Page control) : base(control)
    {
        Loading = Visibility.Visible;

        DispatcherHelper.RunAsync(async () =>
        {
            try
            {
                if (CacheResources.CurseForgeModpackViewDatas == null)
                    await CacheResources.BeginDownloadCurseForgeModpackViewDatas();

                Modpacks = new(CacheResources.CurseForgeModpackViewDatas);
            }
            catch { }

            Loading = Visibility.Collapsed;
        });
    }

    [Reactive]
    public ObservableCollection<CurseForgeModpackViewData> Modpacks { get; set; }

    [Reactive]
    public string SearchFilter { get; set; }

    [Reactive]
    public Visibility Loading { get; set; } = Visibility.Visible;

    [Reactive]
    public string ModLoaderFilter { get; set; }

    [Reactive]
    public List<string> ModLoaderFilters { get; set; } = new() { "Forge", "Fabric", "LiteLoader", "Cauldron" };

    [Reactive]
    public string VersionFilter { get; set; }

    [Reactive]
    public List<string> VersionFilters { get; set; }
        = new()
        {
                "1.19.2", "1.19",
                "1.18.2", "1.18",
                "1.17.1", "1.17",
                "1.16.5", "1.16",
                "1.15.2", "1.15",
                "1.14.4", "1.14",
                "1.13.2", "1.13",
                "1.12.2", "1.12",
                "1.11.2", "1.11",
                "1.10.2", "1.10",
                "1.9",
                "1.8.9",
                "1.7.10",
                "1.6.4"
        };

    public async Task Search()
    {
        Loading = Visibility.Visible;

        Modpacks = null;
        var nodLoaderFilter = (FluentCore.Class.Model.Install.ModLoaderType)(ModLoaderFilter switch
        {
            "Forge" => 1,
            "Cauldron" => 2,
            "LiteLoader" => 3,
            "Fabric" => 4,
            _ => 0
        });

        Modpacks = (await GlobalResources.CurseForgeModpackFinder.SearchModpacksAsync(SearchFilter, nodLoaderFilter, VersionFilter))
            .CreateCollectionViewData<CurseForgeModpack, CurseForgeModpackViewData>();

        Loading = Visibility.Collapsed;
    }
}
