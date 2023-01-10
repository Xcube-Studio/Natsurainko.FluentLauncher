using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.Toolkits.Values
{
    public static class IEnumerableExtension
    {
        public static void AddNotRepeating<T>(this ICollection<T> source, IEnumerable<T> values)
            => values.Where(x => !source.Where(y => x.Equals(y)).Any()).ToList().ForEach(source.Add);
    }
}
