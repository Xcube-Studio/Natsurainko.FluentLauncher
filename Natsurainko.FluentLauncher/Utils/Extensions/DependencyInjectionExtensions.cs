using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nrk.FluentCore.Resources;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.ApplicationModel;
using Windows.Storage;

#if ENABLE_LOAD_EXTENSIONS
using Windows.Storage;
using FluentLauncher.Infra.ExtensionHost;
using FluentLauncher.Infra.ExtensionHost.Assemblies;
using FluentLauncher.Infra.ExtensionHost.Extensions;
using System.Linq;
#endif

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class DependencyInjectionExtensions
{
    const string CurseForgeClientApiKey = "$2a$10$lf9.hHl3PMJ4d3BisICcAOX91uT/mM9/VPDfzpg7r3C/Y8cXIRTNm";

    public static IServiceCollection UseHttpClient(this IServiceCollection services)
    {
        PackageVersion v = Package.Current.Id.Version;
        ProductInfoHeaderValue userAgent = new("Natsurainko.FluentLauncher", v.GetVersionString());

        services.AddHttpClient()
            .ConfigureHttpClientDefaults(builder =>
            {
                builder.ConfigureHttpClient(c => c.DefaultRequestHeaders.UserAgent.Add(userAgent));
            });

        return services;
    }

    public static IServiceCollection UseResourceClients(this IServiceCollection services)
    {
        services.AddSingleton<ModrinthClient>();
        services.AddSingleton(p =>
        {
            HttpClient httpClient = p.GetRequiredService<HttpClient>();
            return new CurseForgeClient(CurseForgeClientApiKey, httpClient);
        });

        return services;
    }

    public static void UseSerilog(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog(c =>
        {
            c.WriteTo.File
            (
                Path.Combine(ApplicationData.Current.LocalFolder.Path, "launcher-logs/log-.txt"),
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 1000000,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}][{Level:u3}] <{SourceContext}>: {Message:lj}{NewLine}{Exception}"
            );
        });
    }

#if ENABLE_LOAD_EXTENSIONS
    public static void UseApplicationExtensionHost(this WinUIApplicationBuilder builder)
    {
        ApplicationExtensionHost.Initialize<App>();
        List<IExtension> Instances = [];
        List<IExtensionAssembly> Assemblies = [];

        string extensionsFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Extensions");
        Directory.CreateDirectory(extensionsFolder);

        foreach (string file in Directory.EnumerateFiles(extensionsFolder, "FluentLauncher.Extension.*.dll", new EnumerationOptions() { RecurseSubdirectories = true}))
        {
            var extensionAssembly = ApplicationExtensionHost.Current.GetExtensionAssembly(file);
            Assemblies.Add(extensionAssembly);

            foreach (IExtension instance in extensionAssembly.ForeignAssembly.GetExportedTypes()
                .Where(type => type.IsAssignableTo(typeof(IExtension)))
                .Select(type => Activator.CreateInstance(type) as IExtension)!)
            {
                Instances.Add(instance);

                instance.RegisteredPages.ToList()
                    .ForEach(d => builder.Pages.WithPage(d.Key, d.Value.Item1, d.Value.Item2));
                instance.RegisteredDialogs.ToList()
                    .ForEach(d => builder.Dialogs.WithDialog(d.Key, d.Value.Item1, d.Value.Item2));

                instance.SetExtensionFolder(new FileInfo(file).DirectoryName!);
                builder.ConfigureServices(instance.ConfigureServices);
            }
        }

        builder.Services.AddSingleton(Instances);
        builder.Services.AddSingleton(Assemblies);

        builder.Pages.WithPage<Views.Settings.ExtensionsPage, ViewModels.Settings.ExtensionsViewModel>("Settings/Extensions");
    }
#endif
}
