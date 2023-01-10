using System.Collections.Generic;

namespace Natsurainko.Toolkits.Values
{
    public static class LongExtension
    {
        public static string LengthToMb(this long value) => $"{value / (1024.0 * 1024.0):0.0} Mb";

        public static IEnumerable<(long, long)> SplitIntoRange(this long value, int rangeCount)
        {
            long a = 0;

            while (value > a)
            {
                long add = value / rangeCount;

                if (a + add > value)
                    add = value - a;

                yield return (a, a + add);
                a += add;
            }
        }
    }
}
