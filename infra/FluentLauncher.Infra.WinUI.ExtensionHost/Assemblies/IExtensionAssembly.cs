using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.ExtensionHost.Assemblies;

public interface IExtensionAssembly : IDisposable
{
    Assembly ForeignAssembly { get; }

    Task LoadAsync();

    bool TryEnableHotReload();

    Uri LocateResource(object component, [CallerFilePath] string callerFilePath = "");
}
