using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils.Xaml;

internal static class StoryboardExtensions
{
    public static Task BeginAsync(this Storyboard storyboard)
    {
        if (storyboard == null)
            throw new ArgumentNullException(nameof(storyboard));

        var taskCompletionSource = new TaskCompletionSource();

        void onComplete(object s, object e)
        {
            //storyboard.Stop();
            storyboard.Completed -= onComplete;
            taskCompletionSource.SetResult();
        }

        storyboard.Completed += onComplete;
        storyboard.Begin();

        return taskCompletionSource.Task;
    }
}
