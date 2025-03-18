using Microsoft.Extensions.DependencyInjection;
using Nrk.FluentCore.Resources;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.ApplicationModel;

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
}
