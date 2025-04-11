using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace FluentLauncher.Infra.WinUI.ExtensionHost;

internal class ExtensionLoadContext(string assemblyPath) : AssemblyLoadContext(true)
{
    private readonly AssemblyDependencyResolver ParentResolver = new(Assembly.GetEntryAssembly()!.Location);
    private readonly AssemblyDependencyResolver Resolver = new(assemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? defaultAssemblyPath = ParentResolver.ResolveAssemblyToPath(assemblyName);
        if (defaultAssemblyPath != null) 
            return Default.LoadFromAssemblyName(assemblyName);

        string? assemblyPath = Resolver.ResolveAssemblyToPath(assemblyName);

        if (assemblyPath != null)
        {
            Trace.WriteLine($"Loading from ${assemblyPath}");
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = ParentResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        libraryPath ??= Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        if (libraryPath != null)
        {
            Trace.WriteLine($"Loading (unmanaged) from ${libraryPath}");
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
