using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Analyzers.Helpers
{

    /// <summary>
    /// Shared helper methods for controller-level security analysis.
    /// Used by SEC001/SEC002 rules and other controller-focused analyzers.
    /// </summary>
    internal static class ControllerSecurityHelpers
    {
        /// <summary>
        /// Determines if a method is a WebAPI controller method.
        /// Returns true if the method is in a class with [ApiController] attribute
        /// or has an HTTP verb attribute ([HttpGet], [HttpPost], etc.).
        /// </summary>
        public static bool IsControllerMethod(MethodDeclarationSyntax method)
        {
            var classDecl = method.Parent as ClassDeclarationSyntax;
            if (classDecl == null)
                return false;

            var hasApiControllerAttribute = classDecl.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => GetAttributeName(attr) == "ApiController");

            var hasHttpVerb = classDecl.AttributeLists.Concat(method.AttributeLists)
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                {
                    var name = GetAttributeName(attr);
                    return name is "HttpGet" or "HttpPost" or "HttpPut" or "HttpDelete" or "HttpPatch" or "HttpHead" or "HttpOptions";
                });

            return hasApiControllerAttribute || hasHttpVerb;
        }

        /// <summary>
        /// Checks if an invocation is a logging or Activity method that should not receive tainted data.
        /// </summary>
        public static bool IsLoggingOrActivityMethod(InvocationExpressionSyntax invocation)
        {
            var methodName = GetLoggingMethodName(invocation);
            return methodName is
                "LogDebug" or "LogInformation" or "LogWarning" or "LogError" or "LogCritical" or "Log" or
                "SetTag" or "SetStatus" or "AddEvent";
        }

        /// <summary>
        /// Extracts the method name from an invocation expression.
        /// </summary>
        public static string? GetLoggingMethodName(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                GenericNameSyntax genericName => genericName.Identifier.Text,
                IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
                _ => null
            };
        }

        /// <summary>
        /// Extracts the identifier name from an expression for reporting purposes.
        /// </summary>
        public static string? ExtractIdentifierName(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                _ => null
            };
        }

        /// <summary>
        /// Extracts the attribute name from an attribute syntax node.
        /// </summary>
        private static string GetAttributeName(AttributeSyntax attribute)
        {
            return attribute.Name switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
                _ => ""
            };
        }
    }
}
