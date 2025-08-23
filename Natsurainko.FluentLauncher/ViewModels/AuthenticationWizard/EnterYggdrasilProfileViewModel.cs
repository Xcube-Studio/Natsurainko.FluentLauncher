using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class EnterYggdrasilProfileViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly HttpClient _httpClient;

    public override bool CanNext => !(!IsValidServer || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password));

    private CancellationTokenSource _cancellationTokenSource;
    private const int UrlChangeDebounceDelayMillis = 1000;

    public EnterYggdrasilProfileViewModel()
    {
        XamlPageType = typeof(EnterYggdrasilProfilePage);

        _authenticationService = App.GetService<AuthenticationService>();
        _httpClient = App.GetService<HttpClient>();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Url { get; set; }

    [ObservableProperty]
    public partial bool IsValidServer { get; set; }

    [ObservableProperty]
    public partial bool ValidatingServer { get; set; }

    [ObservableProperty]
    public partial string ValidationResultIcon { get; set; } = "\ue711";

    [ObservableProperty]
    public partial string ServerName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Email { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Password { get; set; }

    public override WizardViewModelBase GetNextViewModel()
    {
        return new ConfirmProfileViewModel(async cancellationToken => await _authenticationService.LoginYggdrasilAsync(Url, Email, Password, ServerName, cancellationToken))
        { LoadingProgressText = "Authenticating with Yggdrasil Server" };
    }

    partial void OnUrlChanged(string value)
    {
        IsValidServer = false;
        DebounceRequestCheckServerUri();
    }

    private void DebounceRequestCheckServerUri()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        ValidatingServer = true;

        var token = _cancellationTokenSource.Token;

        Task.Delay(UrlChangeDebounceDelayMillis).ContinueWith(async (task) =>
        {
            if (task.IsCanceled)
                return;

            await RequestCheckServerUri(token);
        }, token);
    }

    private async Task RequestCheckServerUri(CancellationToken cancellationToken)
    {
        try
        {
            var metaDataJson = JsonNode.Parse(await _httpClient.GetStringAsync(Url, cancellationToken));
            var name = metaDataJson["meta"]?["serverName"]?.GetValue<string>();

            if (cancellationToken.IsCancellationRequested)
                return;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ServerName = name;
                IsValidServer = !string.IsNullOrEmpty(name);
                ValidationResultIcon = IsValidServer ? "\ue73e" : "\ue711";
            });
        }
        catch
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ValidationResultIcon = "\ue711";
                IsValidServer = false;
            });
        }
        finally
        {
            App.DispatcherQueue.TryEnqueue(() => ValidatingServer = false);
        }
    }
}
