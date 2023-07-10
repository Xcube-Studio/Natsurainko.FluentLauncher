using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentLauncher.ViewModels.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class NotificationService
{
    private StackPanel _itemsContainer;
    private Grid _shadowReceiver;
    private ThemeShadow _themeShadow;

    public void InitContainer(StackPanel itemsContainer, Grid shadowReceiver)
    {
        _itemsContainer = itemsContainer;
        _shadowReceiver = shadowReceiver;

        _themeShadow = new ThemeShadow();
        _themeShadow.Receivers.Add(shadowReceiver);
    }

    public void NotifyWithSpecialContent(string title, string contentTemplateKey, object contentDataContext, string icon = "\uE7E7", int delay = 5000)
    {
        _itemsContainer.Children.Add(CreateNotifyPresenter(
            title, contentTemplateKey, contentDataContext, icon, delay));
    }
    
    private ContentPresenter CreateNotifyPresenter(string title, string contentTemplateKey, object contentDataContext, string icon = "\uE7E7", int delay = 5000)
    {
        var translateTransform = new TranslateTransform() { X = 500 };
        var popupAnimation = CreatePopupAnimation(translateTransform);

        var contentPresenter = new ContentPresenter
        {
            CornerRadius = new CornerRadius(8),
            ContentTemplate = (DataTemplate)App.Current.Resources["NotifyPresenterTemplate"],
            Shadow = _themeShadow,
            RenderTransform = translateTransform
        };

        contentPresenter.Translation += new System.Numerics.Vector3(0, 0, 48);

        async void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            popupAnimation.Begin();

            var notifyPresenterViewModel = new NotifyPresenterViewModel
            {
                Icon = icon,
                NotifyTitile = title,
                NotifyContent = CreateNotifyContent(contentTemplateKey, contentDataContext),
                Remove = () => _itemsContainer.Children.Remove(contentPresenter),
                RetractAnimation = CreateRetractAnimation(translateTransform)
            };
            contentPresenter.DataContext = notifyPresenterViewModel;

            await Task.Delay(delay);
            await notifyPresenterViewModel.Close();

            contentPresenter.Loaded -= ContentPresenter_Loaded;
        }
        contentPresenter.Loaded += ContentPresenter_Loaded; 

        return contentPresenter;
    }

    private ContentPresenter CreateNotifyContent(string templateKey, object dataContext)
    {
        var contentPresenter = new ContentPresenter
        {
            ContentTemplate = (DataTemplate)App.Current.Resources[templateKey],
        };

        void ContentPresenter_Loaded(object s, RoutedEventArgs _)
        {
            contentPresenter.DataContext = dataContext;
            contentPresenter.Loaded -= ContentPresenter_Loaded;
        }
        contentPresenter.Loaded += ContentPresenter_Loaded;

        return contentPresenter;
    }

    private Storyboard CreatePopupAnimation(TranslateTransform transform)
    {
        var storyboard = new Storyboard();
        var doubleAnimation = new DoubleAnimation();
        doubleAnimation.To = 0;
        doubleAnimation.EasingFunction = new CircleEase();

        storyboard.Children.Add(doubleAnimation);
        Storyboard.SetTarget(doubleAnimation, transform);
        Storyboard.SetTargetProperty(storyboard, "X");

        return storyboard;
    }

    private Storyboard CreateRetractAnimation(TranslateTransform transform)
    {
        var storyboard = new Storyboard();
        var doubleAnimation = new DoubleAnimation();
        doubleAnimation.To = 500;
        doubleAnimation.EasingFunction = new CircleEase();

        storyboard.Children.Add(doubleAnimation);
        Storyboard.SetTarget(doubleAnimation, transform);
        Storyboard.SetTargetProperty(storyboard, "X");

        return storyboard;
    }
}
