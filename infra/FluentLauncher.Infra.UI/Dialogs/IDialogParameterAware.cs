namespace FluentLauncher.Infra.UI.Dialogs;

/// <summary>
/// Interface for a dialog view model that can receive a parameter.
/// </summary>
public interface IDialogParameterAware
{
    /// <summary>
    /// Sets the parameter for the dialog view model.
    /// </summary>
    /// <param name="param">The parameter to set.</param>
    void HandleParameter(object param);
}
