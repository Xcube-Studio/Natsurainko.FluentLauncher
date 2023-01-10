using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Interface;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Installer;

public abstract class InstallerBase : IInstaller
{
    public event EventHandler<(string, float)> ProgressChanged;

    public IGameCoreLocator GameCoreLocator { get; private set; }

    public string CustomId { get; private set; }

    public InstallerBase(IGameCoreLocator coreLocator, string customId = default)
    {
        this.GameCoreLocator = coreLocator;
        this.CustomId = customId;
    }

    public InstallerResponse Install() => InstallAsync().GetAwaiter().GetResult();

    public virtual Task<InstallerResponse> InstallAsync()
    {
        throw new NotImplementedException();
    }

    protected virtual async Task CheckInheritedCore(float startProgress, float endProgress, string inheritedId)
    {
        OnProgressChanged("Checking Inherited Core", startProgress);

        if (GameCoreLocator.GetGameCore(inheritedId) == null)
        {
            var installer = new MinecraftVanlliaInstaller(GameCoreLocator, inheritedId);
            installer.ProgressChanged += (object sender, (string, float) e) => OnProgressChanged($"Downloading Inherited Core - {e.Item1}", startProgress + (endProgress - startProgress) * e.Item2);

            var installerResponse = await installer.InstallAsync();
        }
    }

    protected virtual void OnProgressChanged(string message, float progress) => ProgressChanged?.Invoke(this, (message, progress));
}
