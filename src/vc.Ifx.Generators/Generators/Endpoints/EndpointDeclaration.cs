using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace vc.Ifx.Generators;

internal sealed class EndpointDeclaration
{
    internal EndpointDeclaration(string handler, string route, string method, string routeKey,
        string? name, string registration, Location location, Location nameLocation, ImmutableArray<Diagnostic> diagnostics)
    {
        Handler = handler;
        Route = route;
        Method = method;
        RouteKey = routeKey;
        Name = name;
        Registration = registration;
        Location = location;
        NameLocation = nameLocation;
        Diagnostics = diagnostics;
    }

    internal string Handler { get; }
    internal string Route { get; }
    internal string Method { get; }
    internal string RouteKey { get; }
    internal string? Name { get; }
    internal string Registration { get; }
    internal Location Location { get; }
    internal Location NameLocation { get; }
    internal ImmutableArray<Diagnostic> Diagnostics { get; }
}
