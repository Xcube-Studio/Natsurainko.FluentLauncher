using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI;

/// <summary>
/// Provides a hierarchy of <seealso cref="IServiceScope"/> instances. This should be added as a scoped service.
/// </summary>
public interface IServiceScopeHierarchy
{
    /// <summary>
    /// Parent scope of the current service scope. If this is the root scope, this property will be <see langword="null"/>.
    /// </summary>
    IServiceScope? ParentScope { get; }

    /// <summary>
    /// Current service scope.
    /// </summary>
    IServiceScope CurrentScope { get; }

    /// <summary>
    /// Initialize this service when a child scope is created.
    /// </summary>
    /// <param name="parentScope">Parent scope of this service scope.</param>
    void Initialize(IServiceScope? parentScope, IServiceScope currentScope);
}

/// <summary>
/// Default implementation of <see cref="IServiceScopeHierarchy"/>
/// </summary>
public class ServiceScopeHierarchy : IServiceScopeHierarchy
{
    /// <inheritdoc/>
    public IServiceScope? ParentScope { get; private set; }

    public IServiceScope CurrentScope { get; private set; } = null!;

    /// <inheritdoc/>
    public void Initialize(IServiceScope? parentScope, IServiceScope currentScope)
    {
        ParentScope = parentScope;
        CurrentScope = currentScope;
    }
}

public static class ServiceScopeExtensions
{
    /// <summary>
    /// Get the parent scope of the given service scope.
    /// </summary>
    /// <param name="scope">The service scope to get the parent scope of.</param>
    /// <returns>The parent <see cref="IServiceScope"/> of <paramref name="scope"/></returns>
    public static IServiceScope? GetParentScope(this IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<IServiceScopeHierarchy>().ParentScope;
    }

    /// <summary>
    /// Get the root scope of the given service scope.
    /// </summary>
    /// <param name="scope">The service scope to get the root scope of.</param>
    /// <returns>The root <see cref="IServiceScope"/> of <paramref name="scope"/>.</returns>
    public static IServiceScope GetRootScope(this IServiceScope scope)
    {
        while (scope.GetParentScope() is IServiceScope parentScope)
        {
            scope = parentScope;
        }
        return scope;
    }

    /// <summary>
    /// Create a child scope of the given service scope.
    /// </summary>
    /// <param name="scope">The service scope to create a child scope of.</param>
    /// <returns>The child <see cref="IServiceScope"/> created from <paramref name="scope"/>.</returns>
    public static IServiceScope CreateChildScope(this IServiceScope scope)
    {
        IServiceScope childScope = scope.ServiceProvider.CreateScope();

        // Initialize the IParentScopeProvider in the child scope
        childScope.ServiceProvider.GetRequiredService<IServiceScopeHierarchy>().Initialize(scope, childScope);

        return childScope;
    }
}