using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class TaskExtensions
{
    public static void Forget(this Task _)
    {
        // Intentionally left empty to suppress the warning about unobserved exceptions.
        // This is useful when you want to run a task without awaiting it and don't care about its result or exceptions.
    }
}
