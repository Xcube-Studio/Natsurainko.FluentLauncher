using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;

namespace Natsurainko.FluentLauncher.Models;

public partial class LoggerItem : ObservableObject
{
    public LoggerItem(GameLoggerOutput output)
    {
        var richTextBlock = new RichTextBlock();
        LoggerColorLightLanguage.Formatter.FormatRichTextBlock(output.FullData, new LoggerColorLightLanguage(), richTextBlock);
        RichTextBlock = richTextBlock;

        ErrorVisibility = (output.Level == GameLoggerOutputLevel.Error || output.Level == GameLoggerOutputLevel.Fatal)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    [ObservableProperty]
    private RichTextBlock richTextBlock;

    [ObservableProperty]
    private Visibility errorVisibility;
}
