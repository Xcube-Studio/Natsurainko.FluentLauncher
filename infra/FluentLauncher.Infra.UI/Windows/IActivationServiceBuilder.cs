using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Windows;

public interface IActivationServiceBuilder
{
    IDictionary<string, WindowDescriptor> RegisteredWindows { get; }
    IActivationService Build();
    IActivationServiceBuilder WithMultiInstanceWindow(string key, Type windowType);
    IActivationServiceBuilder WithMultiInstanceWindow<TWindow>(string key);
    IActivationServiceBuilder WithSingleInstanceWindow(string key, Type windowType);
    IActivationServiceBuilder WithSingleInstanceWindow<TWindow>(string key);
    IActivationServiceBuilder WithWindow(string key, Type windowType, bool multiInstance);
}