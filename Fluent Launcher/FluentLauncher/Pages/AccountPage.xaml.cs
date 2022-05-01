using FluentCore.Service.Network;
using FluentLauncher.Classes;
using FluentLauncher.Models;
using FluentLauncher.Strings;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AccountPage : Page
    {
        public AccountPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Grid
        private void Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var control = (Grid)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Visible;
        }

        private void Item_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var control = (Grid)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Buttons
        private void DeleteAccount(object sender, RoutedEventArgs e)
        {
            ShareResource.MinecraftAccounts.Remove((MinecraftAccount)((Button)sender).DataContext);
            App.Settings.Values["MinecraftAccounts"] = JsonConvert.SerializeObject(ShareResource.MinecraftAccounts);
            if (!ShareResource.MinecraftAccounts.Contains(ShareResource.SelectedAccount))
                ShareResource.SelectedAccount = null;
            UpdateListView();
        }

        private void CancelAddAccount(object sender, RoutedEventArgs e) => AddAccountDialog.Hide();

        private void TryLogin(object sender, RoutedEventArgs e)
        {
            ShareResource.MainContainer.ContentFrame.Navigate(typeof(WebBrowser));
            AddAccountDialog.Hide();
        }

        private async void AddAccount(object sender, RoutedEventArgs e)
        {
            MinecraftAccount newItem = default;
            switch ((string)AccountTypeBox.SelectedItem)
            {
                case "MicrosoftAccount":
                case "微软账户":
                    newItem = new MinecraftAccount
                    {
                        AccessToken = ShareResource.AuthenticationResponse.AccessToken,
                        UserName = ShareResource.AuthenticationResponse.Name,
                        Uuid = ShareResource.AuthenticationResponse.Id,
                        Type = "MicrosoftAccount",
                        ExpiresIn = ShareResource.AuthenticationResponse.ExpiresIn,
                        RefreshToken = ShareResource.AuthenticationResponse.RefreshToken,
                        Time = ShareResource.AuthenticationResponse.Time
                    };

                    ShareResource.AuthenticationResponse = null;
                    break;
                case "OfflineAccount":
                case "离线账户":
                    var req = new OfflineAuthenticationRequest(DisplayNameBox.Text);
                    var res = await App.DesktopBridge.SendAsync<OfflineAuthenticationResponse>(req);

                    newItem = new MinecraftAccount
                    {
                        AccessToken = res.AccessToken,
                        UserName = res.Name,
                        Uuid = res.Id,
                        Type = "OfflineAccount"
                    };

                    break;
                case "Authlib-injector Account":
                case "外置登录账户":
                    var authReq = new YggdrasilAuthenticationRequest(Authlib_InjectorEmailBox.Text, Authlib_InjectorPasswordBox.Password, AuthenticateServerBox.Text);
                    AddAccountButton.IsEnabled = false;
                    var authRes = await App.DesktopBridge.SendAsync<YggdrasilAuthenticationResponse>(authReq);

                    if (authRes == null || authRes.Response == "Failed")
                    {
                        AddAccountDialog.Hide();

                        _ = ShareResource.ShowInfoAsync("Authenticate Failed..", "", 3000, InfoBarSeverity.Error);
                        return;
                    }

                    newItem = new MinecraftAccount
                    {
                        AccessToken = authRes.AccessToken,
                        ClientToken = authRes.ClientToken,
                        YggdrasilServerUrl = AuthenticateServerBox.Text,
                        UserName = authRes.Name,
                        Uuid = authRes.Id,
                        Type = "Authlib-injector Account"
                    };

                    break;
                default:
                    break;
            }

            AddAccountDialog.Hide();
            ShareResource.MinecraftAccounts.AddWithUpdate(newItem);
            UpdateListView();
            ShareResource.SelectedAccount = newItem;
            UpdateListView();

            await ShareResource.ShowInfoAsync("Add New Account Successfully", "", 3000, InfoBarSeverity.Success);
        }

        private async void RefreshButton(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = ((Button)((StackPanel)((Button)sender).Parent).FindName("DeleteAccountButton")).IsEnabled = false;
            var account = ((MinecraftAccount)((Button)sender).DataContext);

            var res = await App.DesktopBridge.SendAsync<RefreshMicrosoftAuthenticationResponse>
                (new RefreshMicrosoftAuthenticationRequest(account.RefreshToken));

            if (res == null || res.Response == "Failed")
            {
                ((Button)sender).IsEnabled = true;
                await ShareResource.ShowInfoAsync("Failed to Refresh Account", "", 3000, InfoBarSeverity.Error);
            }

            var newItem = new MinecraftAccount
            {
                AccessToken = res.AccessToken,
                RefreshToken = res.RefreshToken,
                ExpiresIn = res.ExpiresIn,
                Time = res.Time,
                Type = account.Type,
                UserName = res.Name,
                Uuid = res.Id,
            };

            ShareResource.MinecraftAccounts.Remove(account);
            ShareResource.MinecraftAccounts.AddWithUpdate(newItem);

            if (ShareResource.SelectedAccount.Uuid == newItem.Uuid)
                ShareResource.SelectedAccount = newItem;

            if (!ShareResource.MinecraftAccounts.Contains(ShareResource.SelectedAccount))
                ShareResource.SelectedAccount = null;

            UpdateListView();

            ((Button)sender).IsEnabled = ((Button)((StackPanel)((Button)sender).Parent).FindName("DeleteAccountButton")).IsEnabled = true;
            await ShareResource.ShowInfoAsync("Refresh Account Successfully", "", 3000, InfoBarSeverity.Success);
        }
        #endregion

        #region ListViewItem
        private async void ListViewItem_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            AccountTypeBox.SetItemsSource(ShareResource.AccountTypes);
            DisplayNameBox.Text = "Steve";
            LoginStateText.Text = string.Empty;
            LoginStateButton.Visibility = Visibility.Collapsed;
            AuthenticateServerVerifyText.Text = string.Empty;
            AuthenticateServerBox.Text = string.Empty;
            Authlib_InjectorEmailBox.Text = string.Empty;
            Authlib_InjectorPasswordBox.Password = string.Empty;
            AuthenticateServerLoading.Visibility = Visibility.Collapsed;
            Authlib_InjectorAccountLoginControls.Visibility = Visibility.Collapsed;

            Authlib_InjectorAccountBorder.Visibility = MicrosoftAccountBorder.Visibility = OfflineAccountBorder.Visibility = Visibility.Collapsed;

            await AddAccountDialog.ShowAsync();
        }
        #endregion

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListView();
            ContentListView.SelectionChanged += ContentListView_SelectionChanged;

            if (ShareResource.WebBrowserNavigateBack)
            {
                if (!string.IsNullOrEmpty(ShareResource.WebBrowserLoginCode))
                {
                    LoginStateButton.Visibility = Visibility.Visible;
                    _ = GetMicrosoftAuthenticatorResult();
                    await AddAccountDialog.ShowAsync();
                }

            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentListView.SelectionChanged -= ContentListView_SelectionChanged;
        }
        #endregion

        #region ListView
        private void ContentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ShareResource.SelectedAccount = (MinecraftAccount)ContentListView.SelectedItem;
        #endregion

        #region ComboBox
        private void AccountTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)AccountTypeBox.SelectedItem)
            {
                case "MicrosoftAccount":
                case "微软账户":
                    OfflineAccountBorder.Visibility = Authlib_InjectorAccountBorder.Visibility = Visibility.Collapsed;
                    MicrosoftAccountBorder.Visibility = Visibility.Visible;
                    AddAccountButton.IsEnabled = ShareResource.AuthenticationResponse != null;
                    break;
                case "OfflineAccount":
                case "离线账户":
                    OfflineAccountBorder.Visibility = Visibility.Visible;
                    MicrosoftAccountBorder.Visibility = Authlib_InjectorAccountBorder.Visibility = Visibility.Collapsed;
                    AddAccountButton.IsEnabled = true;
                    break;
                case "Authlib-injector Account":
                case "外置登录账户":
                    Authlib_InjectorAccountBorder.Visibility = Visibility.Visible;
                    OfflineAccountBorder.Visibility = MicrosoftAccountBorder.Visibility = Visibility.Collapsed;
                    AddAccountButton.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region InputBox
        private async void AuthenticateServerBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            AuthenticateServerLoading.Visibility = Visibility.Visible;
            AuthenticateServerVerifyText.Text = string.Empty;

            if (await HttpHelper.VerifyHttpConnect(sender.Text))
            {
                Authlib_InjectorAccountLoginControls.Visibility = Visibility.Visible;
                AddAccountButton.IsEnabled = true;
            }
            else
            {
                Authlib_InjectorAccountLoginControls.Visibility = Visibility.Collapsed;
                AuthenticateServerVerifyText.Text = "Invalid Authentication Server";
                AddAccountButton.IsEnabled = false;
            }

            AuthenticateServerLoading.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Drag/Drop
        private async void UrlDrop(object sender, DragEventArgs e)
        {
            var def = e.GetDeferral();
            var content = await e.DataView.GetTextAsync();

            if (e.DataView.Contains(StandardDataFormats.Text) && content.StartsWith("authlib-injector:yggdrasil-server:"))
            {
                AuthenticateServerBox.Text = System.Net.WebUtility.UrlDecode(content.Replace("authlib-injector:yggdrasil-server:", string.Empty));

                if ((string)AccountTypeBox.SelectedItem != "Authlib-injector Account" || (string)AccountTypeBox.SelectedItem != "外置登录账户")
                    AccountTypeBox.SetSelectedItem(ShareResource.LanguageResource.AccountPage_DragSelectedItem);
            }

            def.Complete();
        }

        private async void UrlDragEnter(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            var content = await e.DataView.GetTextAsync();

            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text) && content.StartsWith("authlib-injector:yggdrasil-server:")) ? DataPackageOperation.Copy : DataPackageOperation.None;

            deferral.Complete();
        }
        #endregion

        private void UpdateListView()
        {
            ContentListView.SetItemsSource(ShareResource.MinecraftAccounts);
            ContentListView.SetSelectedItem(ShareResource.SelectedAccount);
        }

        private async Task GetMicrosoftAuthenticatorResult()
        {
            CancelAddAccountButton.IsEnabled = LoginButton.IsEnabled = AccountTypeBox.IsEnabled = false;
            async void Connection_RequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
            {
                if ((string)args.Request.Message["Header"] == "MicrosoftAuthenticateProcessing")
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                    {
                        LoginStateText.Text = (string)args.Request.Message["Message"];
                    });
            }

            App.DesktopBridge.Connection.RequestReceived += Connection_RequestReceived;

            var res = await App.DesktopBridge.SendAsync<MicrosoftAuthenticationResponse>
                (new MicrosoftAuthenticationRequest(ShareResource.WebBrowserLoginCode));

            if (res == null || res.Response == "Failed")
            {
                AddAccountDialog.Hide();
                AddAccountButton.IsEnabled = false;
                LoginStateText.Text = string.Empty;
                LoginStateButton.Visibility = Visibility.Collapsed;

                _ = ShareResource.ShowInfoAsync("Authenticate Failed..", "", 3000, InfoBarSeverity.Error);
            }
            else
            {
                AddAccountButton.IsEnabled = true;
                ShareResource.AuthenticationResponse = res;
            }

            App.DesktopBridge.Connection.RequestReceived -= Connection_RequestReceived;

            ShareResource.WebBrowserLoginCode = null;
            CancelAddAccountButton.IsEnabled = LoginButton.IsEnabled = AccountTypeBox.IsEnabled = true;
        }
        #endregion
    }
}
