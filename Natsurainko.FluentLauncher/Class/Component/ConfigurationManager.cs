using Natsurainko.FluentLauncher.Class.AppData;

namespace Natsurainko.FluentLauncher.Class.Component;

public class ConfigurationManager
{
    public static Configuration<AppSettings> Configuration { get; set; }

    public static AppSettings AppSettings => Configuration.Value;
}
