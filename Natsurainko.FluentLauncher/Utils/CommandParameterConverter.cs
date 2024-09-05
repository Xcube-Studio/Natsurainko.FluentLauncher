using System;

namespace Natsurainko.FluentLauncher.Utils;

public static class CommandParameterConverter
{
    public static (TSender sender, TArgs args) As<TSender, TArgs>(this object parameter)
    {
        var sender = parameter.GetType().GetField("Item1")?.GetValue(parameter);
        var args = parameter.GetType().GetField("Item2")?.GetValue(parameter);

        if (sender == null || args == null)
            throw new InvalidCastException("Invalid parameter type.");

        return ((TSender)sender, (TArgs)args);
    }
}
