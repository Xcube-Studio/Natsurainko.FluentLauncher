using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Utils;

internal class IntArrayComparer : IComparer<int[]>
{
    public int Compare(int[]? x, int[]? y)
    {
        if (x == null || y == null)
        {
            if (x == null && y == null)
                return 0;

            if (x == null) return -1; 
            if (y == null) return 1;
        }


        if (x.Length != y.Length)
            return x.Length > y.Length ? 1 : -1;

        for (int i = 0; i < x.Length; i++)
            if (x[i] > y[i]) 
                return 1;
            else if (x[i] < y[i]) 
                return -1;

        return 0;
    }
}
