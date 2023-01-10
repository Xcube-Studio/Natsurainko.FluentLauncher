using System.Threading;

namespace Natsurainko.FluentCore.Event;

public class LaunchProgressChangedEventArgs
{
    public float Progress { get; set; }

    public string Message { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public static LaunchProgressChangedEventArgs Create(float progress, string message, CancellationToken token) => new()
    {
        Progress = progress,
        Message = message,
        CancellationToken = token
    };
}
