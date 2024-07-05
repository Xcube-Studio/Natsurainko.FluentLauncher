using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher;
using System;
using WinRT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using FluentLauncher.Infra.WinUI.AppHost;
using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.Views;


var builder = WinUIApplication.CreateBuilder(() => new App());

//builder.Configuration.AddJsonFile("appsettings.json", optional: true);
//builder.Configuration.AddCommandLine(args);

//builder.Services.Add(...);

//builder.Logging...

builder.UseWinUIExtensionServices();

builder.ConfigurePages((pages) =>
{
    pages.Add(typeof(ShellPage));
});

var app = builder.Build();

await app.RunAsync();
