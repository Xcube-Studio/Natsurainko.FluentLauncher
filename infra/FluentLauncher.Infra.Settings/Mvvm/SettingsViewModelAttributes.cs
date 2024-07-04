using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings.Mvvm;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class SettingsProviderAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class BindToSettingAttribute : Attribute
{
    public required string Path { get; init; }
}