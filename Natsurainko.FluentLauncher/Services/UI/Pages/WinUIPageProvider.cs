using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Pages;
public class WinUIPageProvider : PageProvider<Page>
{
    // Factory pattern
    public static PageProviderBuilder<WinUIPageProvider, Page> GetBuilder(IServiceProvider windowProvider)
    {
        return new PageProviderBuilder<WinUIPageProvider, Page>(windowProvider)
            .WithPageProviderFactory((r, p) => new WinUIPageProvider(r, p));
    }

    public WinUIPageProvider(IReadOnlyDictionary<string, PageDescriptor> registeredPages, IServiceProvider pageProvider)
        : base(registeredPages, pageProvider) { }

    protected override void ConfigureViewModel(Page page, object viewModel)
        => page.DataContext = viewModel;
}
