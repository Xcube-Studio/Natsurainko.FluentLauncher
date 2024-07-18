using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class LinqExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        return source.Where(x => x is not null)!;
    }
}
