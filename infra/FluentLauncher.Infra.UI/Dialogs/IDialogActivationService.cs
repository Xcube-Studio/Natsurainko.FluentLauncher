using System.Threading.Tasks;

namespace FluentLauncher.Infra.UI.Dialogs;

/// <summary>
/// Service for an object that can show dialogs. For example, a window in a GUI app.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the dialog. For example, Confirmed/Cancelled.</typeparam>
public interface IDialogActivationService<TResult>
{
    /// <summary>
    /// Shows a dialog with the given dialog ID.
    /// </summary>
    /// <param name="key">The key of the dialog to show.</param>
    Task<TResult> ShowAsync(string key);

    /// <summary>
    /// Shows a dialog with the given dialog ID.
    /// </summary>
    /// <param name="key">The key of the dialog to show.</param>
    /// <param name="param">Parameter passed to the dialog.</param>
    Task<TResult> ShowAsync(string key, object param);
}
