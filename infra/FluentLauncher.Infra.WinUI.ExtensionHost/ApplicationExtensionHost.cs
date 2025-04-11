using FluentLauncher.Infra.WinUI.ExtensionHost.Assemblies;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace FluentLauncher.Infra.WinUI.ExtensionHost;

public partial class ApplicationExtensionHost<TApplication>() 
    : IApplicationExtensionHost where TApplication : Application
{
    private readonly ConcurrentDictionary<string, IExtensionAssembly> AssembliesByPath = new();
    private readonly ConcurrentDictionary<string, IExtensionAssembly> AssembliesByAssemblyName = new();

    public Assembly EntryAssembly { get; } = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly not found");

    public string HostingProcessDir { get; } = Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException("Hosting process directory not found");

    public TApplication? Application { get; private set; }

    public void Initialize(object application)
    {
        Application = (TApplication?)application;
    }

    public Uri LocateResource(object component, [CallerFilePath] string callerFilePath = "")
    {
        IExtensionAssembly extensionAsm = GetExtensionAssembly(component.GetType().Assembly.GetName());
        return extensionAsm.LocateResource(component, callerFilePath);
    }

    private IExtensionAssembly GetExtensionAssembly(AssemblyName assemblyName)
    {
        return !AssembliesByAssemblyName.TryGetValue(assemblyName.FullName, out IExtensionAssembly? extensionAssembly)
            ? throw new EntryPointNotFoundException()
            : extensionAssembly;
    }

    public IExtensionAssembly GetExtensionAssembly(string pathToAssembly)
    {
        FileInfo fi = new(pathToAssembly);
        IExtensionAssembly asm = AssembliesByPath.GetOrAdd(fi.FullName, asm => new ExtensionAssembly(pathToAssembly));
        _ = AssembliesByAssemblyName.AddOrUpdate(asm.ForeignAssembly.GetName().FullName, asm, (_, _) => asm);
        return asm;
    }
}

public static class ApplicationExtensionHost
{
    private static IApplicationExtensionHost? _Current;

    public static bool IsHotReloadEnabled => Environment.GetEnvironmentVariable("ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO") == "1";

    public static IApplicationExtensionHost Current => _Current ?? throw new InvalidOperationException("ApplicationExtensionHost is not initialized");

    public static void Initialize<TApplication>() where TApplication : Application
    {
        if (_Current != null)
            throw new InvalidOperationException("Cannot initialize application twice");

        _Current = new ApplicationExtensionHost<TApplication>();
    }

    /// <summary>
    /// Gets the default resource map for the specified assembly, or the caller's executing assembly if not provided.
    /// </summary>
    /// <param name="assembly">Assembly for which to load the default resource map</param>
    /// <returns>A ResourceMap if one is found, otherwise null</returns>
    public static ResourceMap? GetResourceMapForAssembly(Assembly? assembly = default)
    {
        assembly ??= Assembly.GetCallingAssembly();
        string assemblyName = assembly.GetName().Name!;

        if (assemblyName == null)
            return null;

        return !ResourceManager.Current.AllResourceMaps.TryGetValue(assemblyName, out ResourceMap? map)
            ? null
            : map.GetSubtree($"{assemblyName}/Resources");
    }
}
