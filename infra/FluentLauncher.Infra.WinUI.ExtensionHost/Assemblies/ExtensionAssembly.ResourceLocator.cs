using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentLauncher.Infra.WinUI.ExtensionHost.Assemblies;

public partial class ExtensionAssembly
{
    public Uri LocateResource(object component, [CallerFilePath] string callerFilePath = "")
    {
        return new Uri($"ms-appx:///{LocateResourcePath(component, callerFilePath).Replace('\\', '/')}");
    }

    private string LocateResourcePath(object component, [CallerFilePath] string callerFilePath = "")
    {
        if (component.GetType().Assembly != ForeignAssembly)
            throw new InvalidProgramException();

        string resourceName = Path.GetFileName(callerFilePath)[..^3];
        TryEnableHotReload();

        string[] pathParts = callerFilePath.Split('\\')[..^1];
        string pathCandidate = Path.Join([.. pathParts[^1..].Append(resourceName).Prepend(ForeignAssemblyName)]);

        FileInfo sourceResource = new(Path.Combine(ForeignAssemblyDir, pathCandidate));
        FileInfo colocatedResource = new(Path.Combine(HostingProcessDir, pathCandidate));

        if (colocatedResource.Exists)
            return pathCandidate;
        if (sourceResource.Exists)
            return sourceResource.FullName;

        throw new FileNotFoundException("Could not find resource", resourceName);
    }
}
