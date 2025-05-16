using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FluentLauncher.Infra.WinUI.SourceGenerators.Helpers;

internal static class GeneratorHelper
{
    private static readonly SymbolDisplayFormat format =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    /// 返回带global::前缀的全名
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static string GetFullyQualifiedName(this ISymbol symbol) => symbol.ToDisplayString(format);

    public static string GetExtensionParameter(this IMethodSymbol methodSymbol, out string parameterName)
    {
        var firstParameter = methodSymbol.Parameters.First();
        parameterName = firstParameter.Name;

        return $"this {firstParameter.Type.GetFullyQualifiedName()} {firstParameter.Name}";
    }

    public static string GetMethodParameters(this IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters
            .Select(p => $"{p.Type.GetFullyQualifiedName()} {p.Name}");

        return methodSymbol.IsExtensionMethod 
            ?  "this " + string.Join(", ", parameters)
            : string.Join(", ", parameters);
    }

    public static Dictionary<string, string> GetProperties(this AttributeData attribute)
    {
        Dictionary<string, string> keyValuePairs = [];

        foreach (var argument in attribute.NamedArguments)
        {
            string qualifiedName = argument.Value.Type?.GetFullyQualifiedName() ?? string.Empty;
            string value = qualifiedName switch
            {
                "double" => $"double.Parse(\"{argument.Value.Value}\")",
                _ => argument.Value.Value?.ToString() ?? "null"
            };

            keyValuePairs.Add(argument.Key, value);
        }

        return keyValuePairs;
    }
}
