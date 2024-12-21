using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI;

/// <summary>
/// Provides the parent scope of this service scope.
/// </summary>
public interface IParentScopeProvider
{
    /// <summary>
    /// Parent scope of this service scope. If this is the root scope, this property will be <see langword="null"/>.
    /// </summary>
    IServiceScope? ParentScope { get; }

    /// <summary>
    /// Initialize this service when a child scope is created.
    /// </summary>
    /// <param name="parentScope">Parent scope of this service scope.</param>
    void Initialize(IServiceScope parentScope);
}

/// <summary>
/// Default implementation of <see cref="IParentScopeProvider"/>
/// </summary>
public class ParentScopeProvider : IParentScopeProvider
{
    /// <inheritdoc/>
    public IServiceScope? ParentScope { get; private set; }

    /// <inheritdoc/>
    [MemberNotNull(nameof(ParentScope))]
    public void Initialize(IServiceScope parentScope)
    {
        ParentScope = parentScope;
    }
}

public static class ServiceScopeHierarchyExtensions
{
    /// <summary>
    /// Get the parent scope of the given service scope.
    /// </summary>
    /// <param name="scope">The service scope to get the parent scope of.</param>
    /// <returns>The parent <see cref="IServiceScope"/> of <paramref name="scope"/></returns>
    public static IServiceScope? GetParentScope(this IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<IParentScopeProvider>().ParentScope;
    }

    /// <summary>
    /// Get the root scope of the given service scope.
    /// </summary>
    /// <param name="scope">The service scope to get the root scope of.</param>
    /// <returns>The root <see cref="IServiceScope"/> of <paramref name="scope"/></returns>
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
        childScope.ServiceProvider.GetRequiredService<IParentScopeProvider>().Initialize(scope);

        return childScope;
    }
}