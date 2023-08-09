using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class JavaMirrorsDialog : ContentDialog
{
    public JavaMirrorsDialog()
    {
        this.InitializeComponent();
    }

    public IEnumerable<object> Sources = new object[]
    {
        new
        {
            Url = "https://adoptium.net/",
            Name = "Adoptium"
        },
        new
        {
            Url = "https://bell-sw.com/pages/downloads/",
            Name = "Bell-sw"
        },
        new
        {
            Url = "https://aws.amazon.com/corretto/",
            Name = "Amazon"
        },
        new
        {
            Url = "https://www.azul.com/downloads/?package=jdk",
            Name = "Azul"
        },
        new
        {
            Url = "https://sap.github.io/SapMachine/",
            Name = "SapMachine"
        },
    };
}
