using Nrk.FluentCore.Interfaces;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Parse;

namespace Nrk.FluentCore.Components.Launch;

public abstract class BaseArgumentsBuilder<TBuilder> : IArgumentsBuilder where TBuilder : IArgumentsBuilder
{
    public GameInfo GameInfo { get; init; }

    public BaseArgumentsBuilder(GameInfo gameInfo)
    {
        GameInfo = gameInfo ?? throw new ArgumentNullException(nameof(gameInfo));
    }

    public abstract IEnumerable<string> Build();

    public abstract TBuilder SetJavaSettings(string javaPath, int maxMemory, int minMemory);

    public abstract TBuilder SetLibraries(IEnumerable<LibraryElement> libraryElements);

    public abstract TBuilder SetAccountSettings(Account account, bool enableDemoUser);

    public abstract TBuilder SetGameDirectory(string directory);

    public abstract TBuilder AddExtraParameters(IEnumerable<string> extraVmParameters = null, IEnumerable<string> extraGameParameters = null);
}
