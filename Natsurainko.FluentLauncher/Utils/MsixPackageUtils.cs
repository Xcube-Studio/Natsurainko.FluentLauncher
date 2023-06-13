using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;
internal static class MsixPackageUtils
{
    public static bool IsPackaged { get; }

    static MsixPackageUtils() 
    {
        IsPackaged = isPackaged();
    }

    private static bool isPackaged()
    {
        try
        {
            var package = Windows.ApplicationModel.Package.Current;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
