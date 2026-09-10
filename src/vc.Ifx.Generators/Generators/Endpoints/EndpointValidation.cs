using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Generators;

internal static class EndpointValidation
{
    private const string AuthorizationExtensions = "global::Microsoft.AspNetCore.Builder.AuthorizationEndpointConventionBuilderExtensions.";
    private const string OpenApiExtensions = "global::Microsoft.AspNetCore.Http.OpenApiRouteHandlerBuilderExtensions.";

    internal static EndpointDeclaration? Read(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var method = (IMethodSymbol)context.TargetSymbol;
        var syntax = (MethodDeclarationSyntax)context.TargetNode;
        var attribute = context.Attributes[0];
        if (context.Attributes.Length != 1 || attribute.AttributeConstructor == null || attribute.ConstructorArguments.Length != 2
            || attribute.ConstructorArguments.Any(HasCompilerError) || attribute.NamedArguments.Any(pair => HasCompilerError(pair.Value))) { return null; }
        var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(token);
        var location = attributeSyntax.GetLocation();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var named = attribute.NamedArguments.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var route = PrimitiveValue(attribute.ConstructorArguments[0]) as string;
        var methodText = PrimitiveValue(attribute.ConstructorArguments[1]) as string;
        var verb = methodText?.ToUpperInvariant() ?? string.Empty;
        var routeKey = string.Empty;
        var handler = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + Escape(method.Name);
        var name = Text("Name");
        var summary = Text("Summary");
        var description = Text("Description");
        var policy = Text("AuthorizationPolicy");
        var authorize = Flag("RequireAuthorization");
        var anonymous = Flag("AllowAnonymous");
        var tags = new List<string>();

        if (string.IsNullOrWhiteSpace(route) || !route!.StartsWith("/", StringComparison.Ordinal) || route != route.Trim())
        {
            Error(EndpointDiagnostics.Declaration, Argument(0), "Route must be a nonblank absolute template without surrounding whitespace.");
        }
        else
        {
            try { routeKey = RouteKey(route); }
            catch (ArgumentException exception) { Error(EndpointDiagnostics.Declaration, Argument(0), "Invalid route template: " + exception.Message); }
        }
        if (!new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS", "TRACE", "CONNECT" }.Contains(verb))
        {
            Error(EndpointDiagnostics.Declaration, Argument(1), "HTTP method must be one explicit supported method token.");
        }
        if (named.TryGetValue("Tags", out var tagValue))
        {
            if (tagValue.Kind != TypedConstantKind.Array || tagValue.IsNull) { Error(EndpointDiagnostics.Declaration, NamedLocation("Tags"), "Tags must be a non-null array."); }
            else
            {
                foreach (var tag in tagValue.Values)
                {
                    var text = PrimitiveValue(tag) as string;
                    if (string.IsNullOrWhiteSpace(text)) { Error(EndpointDiagnostics.Declaration, NamedLocation("Tags"), "Tags must contain nonblank strings."); }
                    else if (!tags.Contains(text!)) { tags.Add(text!); }
                }
            }
        }

        var hasAuthorize = HasAttribute(method, "Microsoft.AspNetCore.Authorization.IAuthorizeData");
        var hasAnonymous = HasAttribute(method, "Microsoft.AspNetCore.Authorization.IAllowAnonymous");
        if ((anonymous && (authorize || policy != null || hasAuthorize)) || ((authorize || policy != null) && hasAnonymous))
        {
            Error(EndpointDiagnostics.Authorization, location, "Anonymous access conflicts with an authorization declaration on this handler.");
        }
        if (!ValidSignature(method, syntax)) { Error(EndpointDiagnostics.Handler, syntax.Identifier.GetLocation(), "Unsupported static endpoint handler: " + handler); }
        var invalidPayload = method.Parameters.FirstOrDefault(parameter => IsDelegatePayload(parameter.Type) && !HasAttribute(parameter, "Microsoft.AspNetCore.Http.Metadata.IFromServiceMetadata"));
        if (IsDelegatePayload(UnwrapTask(method.ReturnType)) || invalidPayload != null)
        {
            Error(EndpointDiagnostics.Payload, invalidPayload?.Locations[0] ?? syntax.ReturnType.GetLocation(), "Delegate values cannot be bound or serialized as endpoint payloads.");
        }

        var registration = new StringBuilder();
        registration.Append("        {\n            var endpoint = global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapMethods(endpoints, ")
            .Append(Literal(route ?? string.Empty)).Append(", new string[] { ").Append(Literal(verb)).Append(" }, (global::System.Delegate)").Append(handler).Append(");\n");
        Metadata("global::Microsoft.AspNetCore.Builder.RoutingEndpointConventionBuilderExtensions.WithName", name);
        Metadata(OpenApiExtensions + "WithSummary", summary);
        Metadata(OpenApiExtensions + "WithDescription", description);
        if (tags.Count != 0) { registration.Append("            ").Append(OpenApiExtensions).Append("WithTags(endpoint, new string[] { ").Append(string.Join(", ", tags.Select(Literal))).Append(" });\n"); }
        if (policy != null) { registration.Append("            ").Append(AuthorizationExtensions).Append("RequireAuthorization(endpoint, new string[] { ").Append(Literal(policy)).Append(" });\n"); }
        else if (authorize) { registration.Append("            ").Append(AuthorizationExtensions).Append("RequireAuthorization(endpoint);\n"); }
        if (anonymous) { registration.Append("            ").Append(AuthorizationExtensions).Append("AllowAnonymous(endpoint);\n"); }
        if (Flag("ExcludeFromDescription")) { registration.Append("            ").Append(OpenApiExtensions).Append("ExcludeFromDescription(endpoint);\n"); }
        registration.Append("        }\n");
        return new EndpointDeclaration(handler, route ?? string.Empty, verb, routeKey, name, registration.ToString(), location, NamedLocation("Name"), diagnostics.ToImmutable());

        string? Text(string key)
        {
            if (!named.TryGetValue(key, out var value) || value.IsNull) { return null; }
            if (PrimitiveValue(value) is string text && !string.IsNullOrWhiteSpace(text)) { return text; }
            Error(EndpointDiagnostics.Declaration, NamedLocation(key), key + " must be a nonblank string or null.");
            return null;
        }
        bool Flag(string key)
        {
            if (!named.TryGetValue(key, out var value)) { return false; }
            if (PrimitiveValue(value) is bool flag) { return flag; }
            Error(EndpointDiagnostics.Declaration, NamedLocation(key), key + " must be a boolean.");
            return false;
        }
        Location Argument(int index) => attributeSyntax.ArgumentList!.Arguments[index].GetLocation();
        Location NamedLocation(string key) => attributeSyntax.ArgumentList!.Arguments.FirstOrDefault(argument => argument.NameEquals?.Name.Identifier.ValueText == key)?.GetLocation() ?? location;
        void Error(DiagnosticDescriptor descriptor, Location at, string message) => diagnostics.Add(Diagnostic.Create(descriptor, at, message));
        void Metadata(string extension, string? value)
        {
            if (value != null) { registration.Append("            ").Append(extension).Append("(endpoint, ").Append(Literal(value)).Append(");\n"); }
        }
    }

    internal static string RouteKey(string route)
    {
        var parsed = TemplateParser.Parse(ValidationTemplate(route));
        var key = new StringBuilder();
        foreach (var segment in parsed.Segments)
        {
            key.Append('/');
            foreach (var part in segment.Parts)
            {
                if (part.IsParameter)
                {
                    key.Append('{').Append(part.IsCatchAll ? "*" : string.Empty);
                    foreach (var constraint in part.InlineConstraints) { key.Append(':').Append(constraint.Constraint); }
                    key.Append(part.IsOptional ? "?" : string.Empty).Append('}');
                }
                else { key.Append(part.Text.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(part.Text); }
            }
        }
        return key.ToString().TrimEnd('/');
    }

    private static string ValidationTemplate(string route)
    {
        // The netstandard parser predates double catch-alls. They differ only in
        // URL generation, not inbound matching; preserve escaped literal braces.
        var template = new StringBuilder(route.Length);
        for (var index = 0; index < route.Length; index++)
        {
            var character = route[index];
            template.Append(character);
            if (character == '{')
            {
                if (index + 1 < route.Length && route[index + 1] == '{') { template.Append(route[++index]); }
                else if (index + 2 < route.Length && route[index + 1] == '*' && route[index + 2] == '*')
                {
                    if (index + 3 < route.Length && route[index + 3] == '*') { throw new ArgumentException("Catch-all parameters allow at most two leading stars.", nameof(route)); }
                    template.Append('*');
                    index += 2;
                }
            }
        }
        return template.ToString();
    }

    private static object? PrimitiveValue(TypedConstant value) => value.Kind == TypedConstantKind.Primitive ? value.Value : null;

    private static bool HasCompilerError(TypedConstant value) => value.Kind == TypedConstantKind.Error
        || (value.Kind == TypedConstantKind.Array && !value.IsNull && value.Values.Any(HasCompilerError));

    private static bool HasAttribute(ISymbol symbol, string interfaceName) => symbol.GetAttributes().Any(attribute =>
        attribute.AttributeClass != null && attribute.AttributeClass.AllInterfaces.Any(contract => contract.ToDisplayString() == interfaceName));

    private static bool ValidSignature(IMethodSymbol method, MethodDeclarationSyntax syntax)
    {
        if (!method.IsStatic || method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
            || method.IsGenericMethod || method.IsExtensionMethod || method.ReturnsByRef || method.ReturnsByRefReadonly
            || method.Parameters.Length > 16 || syntax.Modifiers.Any(SyntaxKind.PartialKeyword)
            || (syntax.Body == null && syntax.ExpressionBody == null)
            || method.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>().Count() != 1) { return false; }
        for (var container = method.ContainingType; container != null; container = container.ContainingType)
        {
            if (container.IsGenericType || container.IsFileLocal || container.TypeKind == TypeKind.Interface
                || container.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) { return false; }
        }
        if (InvalidType(method.ReturnType) || method.Parameters.Any(parameter => parameter.RefKind != RefKind.None || InvalidType(parameter.Type))) { return false; }
        var result = method.ReturnType;
        if (IsTask(result))
        {
            var named = (INamedTypeSymbol)result;
            if (named.TypeArguments.Length != 1) { return false; }
            result = named.TypeArguments[0];
        }
        return result.SpecialType != SpecialType.System_Void && !IsTask(result)
            && (result is not INamedTypeSymbol namedResult || !namedResult.GetMembers("GetAwaiter").OfType<IMethodSymbol>().Any());
    }

    private static bool InvalidType(ITypeSymbol type) => type.IsRefLikeType || type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.TypeParameter or TypeKind.Error
        || (type is IArrayTypeSymbol array && InvalidType(array.ElementType))
        || (type is INamedTypeSymbol named && named.TypeArguments.Any(InvalidType));
    private static bool IsTask(ITypeSymbol type) => type is INamedTypeSymbol named && named.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" && named.Name is "Task" or "ValueTask";
    private static ITypeSymbol UnwrapTask(ITypeSymbol type) => IsTask(type) && ((INamedTypeSymbol)type).TypeArguments.Length == 1 ? ((INamedTypeSymbol)type).TypeArguments[0] : type;
    private static bool IsDelegatePayload(ITypeSymbol type) => type.TypeKind == TypeKind.Delegate || (type is IArrayTypeSymbol array && IsDelegatePayload(array.ElementType));
    private static string Escape(string identifier) => SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None ? identifier : "@" + identifier;
    private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
}
