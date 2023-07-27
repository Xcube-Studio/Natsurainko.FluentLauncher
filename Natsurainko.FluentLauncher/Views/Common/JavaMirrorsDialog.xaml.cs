using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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
