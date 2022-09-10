using Natsurainko.FluentLauncher.Class.AppData;
using System;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class Language
{
    public string Code { get; set; }

    public string DisplayName { get; set; }

    public async void SetCurrentLanguage()
    {
        ApplicationLanguages.PrimaryLanguageOverride = this.Code;

        Window.Current.Content = new Frame();
        var rootFrame = Window.Current.Content as Frame;

        rootFrame.Content = new Microsoft.UI.Xaml.Controls.ProgressRing
        {
            IsActive = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 96,
            Height = 96
        };

        await Task.Delay(1000);
        rootFrame.Navigate(typeof(MainContainer));
    }

    public string GetString(string key) => GlobalResources.ResourceLoader.GetString($"LSS_{key}");
}

public class LanguageViewData : ViewDataBase<Language>
{
    public LanguageViewData(Language data) : base(data)
    {
    }

    public void SetCurrentLanguage() => this.Data.SetCurrentLanguage();

    public string GetString(string key) => this.Data.GetString(key);

    public override int GetHashCode()
        => this.Data.Code.GetHashCode() ^ this.Data.DisplayName.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        var item = (LanguageViewData)obj;

        return this.Data.Code == item.Data.Code
            && this.Data.DisplayName == item.Data.DisplayName;
    }
}
