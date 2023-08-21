using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Notification;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class NotificationService
{
    private StackPanel _itemsContainer;
    private Grid _shadowReceiver;
    private ThemeShadow _themeShadow;
    private int _nextItemZIndex = 1;

    public void InitContainer(StackPanel itemsContainer, Grid shadowReceiver)
    {
        _itemsContainer = itemsContainer;
        _shadowReceiver = shadowReceiver;

        _themeShadow = new ThemeShadow();
        _themeShadow.Receivers.Add(shadowReceiver);
    }

    public void NotifyException(string errorTitleKey, Exception exception, string errorDescriptionKey = "")
    {
        var exceptionViewModel = new NotifyExceptionViewModel
        {
            Exception = exception
        };

        if (!string.IsNullOrEmpty(errorDescriptionKey))
            exceptionViewModel.Description = ResourceUtils.GetValue("Notifications", errorDescriptionKey);

        NotifyWithSpecialContent(ResourceUtils.GetValue("Notifications", errorTitleKey), "ExceptionNotifyTemplate", exceptionViewModel, "\uE711", 20 * 1000);
    }

    public void NotifyMessage(string title, string text, string description = "", string icon = "\uE7E7", int delay = 5000)
    {
        NotifyWithSpecialContent(title, "NormalMessageNotifyTemplate", new
        {
            Text = text,
            Description = description
        }, icon, delay);
    }

    public void NotifyWithoutContent(string title, string icon = "\uE7E7", int delay = 5000) => App.DispatcherQueue.TryEnqueue
        (() => _itemsContainer.Children.Insert(0, CreateNotifyPresenterWithoutContent(title, icon, delay)));

    public void NotifyWithSpecialContent(string title, string contentTemplateKey, object contentDataContext, string icon = "\uE7E7", int delay = 5000)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            _itemsContainer.Children.Insert(0, CreateNotifyPresenter(
                title, contentTemplateKey, contentDataContext, icon, delay));
        });
    }

    private ContentPresenter CreateNotifyPresenterWithoutContent(string title, string icon = "\uE7E7", int delay = 5000)
    {
        var translateTransform = new TranslateTransform() { Y = -125 };
        var popupAnimation = CreatePopupAnimation(translateTransform);

        var contentPresenter = new ContentPresenter
        {
            CornerRadius = new CornerRadius(8),
            ContentTemplate = (DataTemplate)App.Current.Resources["NotifyPresenterWithoutContentTemplate"],
            Shadow = _themeShadow,
            RenderTransform = translateTransform,
            MaxWidth = 500,
        };

        Canvas.SetZIndex(contentPresenter, _nextItemZIndex);
        _nextItemZIndex++;

        contentPresenter.Translation += new System.Numerics.Vector3(0, 0, 48);

        async void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            contentPresenter.Loaded -= ContentPresenter_Loaded;
            popupAnimation.Begin();

            var notifyPresenterViewModel = new NotifyPresenterViewModel
            {
                Icon = icon,
                NotifyTitile = title,
                NotifyContent = null,
                CreateRetractAnimationAction = () => CreateRetractAnimation(translateTransform),
                Remove = () =>
                {
                    _itemsContainer.Children.Remove(contentPresenter);
                    OnRemovedItem();
                },
            };
            contentPresenter.DataContext = notifyPresenterViewModel;

            await Task.Delay(delay);

            if (!notifyPresenterViewModel._removed)
                await notifyPresenterViewModel.Close();
        }

        contentPresenter.Loaded += ContentPresenter_Loaded;
        return contentPresenter;
    }

    private ContentPresenter CreateNotifyPresenter(string title, string contentTemplateKey, object contentDataContext, string icon = "\uE7E7", int delay = 5000)
    {
        var translateTransform = new TranslateTransform() { Y = -125 };
        var popupAnimation = CreatePopupAnimation(translateTransform);

        var contentPresenter = new ContentPresenter
        {
            CornerRadius = new CornerRadius(8),
            ContentTemplate = (DataTemplate)App.Current.Resources["NotifyPresenterTemplate"],
            Shadow = _themeShadow,
            RenderTransform = translateTransform,
            MaxWidth = 500,
        };

        Canvas.SetZIndex(contentPresenter, _nextItemZIndex);
        _nextItemZIndex++;

        contentPresenter.Translation += new System.Numerics.Vector3(0, 0, 48);

        async void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            contentPresenter.Loaded -= ContentPresenter_Loaded;
            popupAnimation.Begin();

            var notifyPresenterViewModel = new NotifyPresenterViewModel
            {
                Icon = icon,
                NotifyTitile = title,
                NotifyContent = CreateNotifyContent(contentTemplateKey, contentDataContext),
                CreateRetractAnimationAction = () => CreateRetractAnimation(translateTransform),
                Remove = () =>
                {
                    _itemsContainer.Children.Remove(contentPresenter);
                    OnRemovedItem();
                },
            };
            contentPresenter.DataContext = notifyPresenterViewModel;

            await Task.Delay(delay);

            if (!notifyPresenterViewModel._removed)
                await notifyPresenterViewModel.Close();
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
        Storyboard.SetTargetProperty(storyboard, "Y");

        return storyboard;
    }

    private Storyboard CreateRetractAnimation(TranslateTransform transform)
    {
        var storyboard = new Storyboard();
        var doubleAnimation = new DoubleAnimation();
        doubleAnimation.To = -_itemsContainer.ActualHeight - 75;
        doubleAnimation.EasingFunction = new CircleEase();

        storyboard.Children.Add(doubleAnimation);
        Storyboard.SetTarget(doubleAnimation, transform);
        Storyboard.SetTargetProperty(storyboard, "Y");

        return storyboard;
    }

    private void OnRemovedItem()
    {
        if (_itemsContainer.Children.Count == 0)
            _nextItemZIndex = 1;
    }
}
