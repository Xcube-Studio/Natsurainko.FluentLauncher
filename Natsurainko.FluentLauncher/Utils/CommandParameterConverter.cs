using System;

namespace Natsurainko.FluentLauncher.Utils;

public static class CommandParameterConverter
{
    public static (TSender sender, TArgs args) As<TSender, TArgs>(this object parameter)
    {
        var sender = parameter.GetType().GetField("Item1").GetValue(parameter);
        var args = parameter.GetType().GetField("Item2").GetValue(parameter);

        return ((TSender)sender, (TArgs)args);
    }

    public static void As<TSender, TArgs>(this object parameter, Action<(TSender sender, TArgs args)> action)
    {
        var sender = parameter.GetType().GetField("Item1").GetValue(parameter);
        var args = parameter.GetType().GetField("Item2").GetValue(parameter);
        action(((TSender)sender, (TArgs)args));
    }
}
