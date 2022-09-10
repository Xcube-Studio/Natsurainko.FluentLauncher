using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Activities
{
    public class ActivityNewsPageVM : ViewModelBase<Page>
    {
        public ActivityNewsPageVM(Page control) : base(control)
        {
            Loading = Visibility.Visible;

            DispatcherHelper.RunAsync(async () =>
            {
                try
                {
                    if (CacheResources.NewsViewDatas == null)
                        await CacheResources.BeginDownloadNews();

                    NewsModels = CacheResources.NewsViewDatas;
                }
                catch { }

                Loading = Visibility.Collapsed;
            });
        }

        [Reactive]
        public List<NewsViewData> NewsModels { get; set; }

        [Reactive]
        public Visibility Loading { get; set; }
    }
}
