using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace vc.Ifx.Analyzers.Helpers;

/// <summary>Traverses declared dependency surfaces for the legacy VBD policies.</summary>
internal static class TypeDependencies
{
    internal static IEnumerable<(ISymbol Source, ITypeSymbol Type)> Enumerate(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var dependency in Members(type, cancellationToken))
        {
            foreach (ITypeSymbol referenced in Expand(dependency.Type, cancellationToken))
            {
                if (visited.Add(referenced))
                {
                    yield return (dependency.Source, referenced);
                }
            }
        }
    }

    internal static IEnumerable<ITypeSymbol> Expand(ITypeSymbol type, CancellationToken cancellationToken)
    {
        var pending = new Stack<ITypeSymbol>();
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        pending.Push(type);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ITypeSymbol current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            yield return current;
            switch (current)
            {
                case IArrayTypeSymbol array:
                    pending.Push(array.ElementType);
                    break;
                case IPointerTypeSymbol pointer:
                    pending.Push(pointer.PointedAtType);
                    break;
                case INamedTypeSymbol named:
                    for (int index = named.TypeArguments.Length - 1; index >= 0; index--)
                    {
                        pending.Push(named.TypeArguments[index]);
                    }
                    break;
            }
        }
    }

    private static IEnumerable<(ISymbol Source, ITypeSymbol Type)> Members(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (type.BaseType is { SpecialType: not SpecialType.System_Object } baseType)
        {
            yield return (type, baseType);
        }
        foreach (INamedTypeSymbol contract in type.Interfaces)
        {
            yield return (type, contract);
        }
        foreach (ISymbol member in type.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (member)
            {
                case IFieldSymbol field:
                    yield return (field, field.Type);
                    break;
                case IPropertySymbol property:
                    yield return (property, property.Type);
                    foreach (IParameterSymbol parameter in property.Parameters)
                    {
                        yield return (property, parameter.Type);
                    }
                    break;
                case IMethodSymbol method:
                    yield return (method, method.ReturnType);
                    foreach (IParameterSymbol parameter in method.Parameters)
                    {
                        yield return (method, parameter.Type);
                    }
                    foreach (ITypeParameterSymbol parameter in method.TypeParameters)
                    {
                        foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
                        {
                            yield return (method, constraint);
                        }
                    }
                    break;
                case IEventSymbol eventSymbol:
                    yield return (eventSymbol, eventSymbol.Type);
                    break;
            }
        }
    }
}
