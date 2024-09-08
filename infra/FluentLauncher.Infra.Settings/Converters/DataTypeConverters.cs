using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings.Converters;

/// <summary>
/// ConvertFrom is used when converting an object from the displayed type to the stored type. <br/>
/// Convert is used when converting an object from the stored type to the displayed type.
/// </summary>
/// <typeparam name="TSource">Type stored in the storage</typeparam>
/// <typeparam name="TTarget">Type used in the application</typeparam>
public interface IDataTypeConverter<TSource, TTarget>
{
    TTarget Convert(TSource source);
    TSource ConvertFrom(TTarget target);

    static abstract IDataTypeConverter<TSource, TTarget> Instance { get; }
}
