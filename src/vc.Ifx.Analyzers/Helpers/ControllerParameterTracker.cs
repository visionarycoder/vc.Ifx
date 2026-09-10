using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Analyzers.Helpers
{

    /// <summary>
    /// Tracks tainted (untrusted) parameters through method execution to detect
    /// if they flow into unsafe SQL operations without proper sanitization.
    /// </summary>
    internal class ControllerParameterTracker
    {
        private readonly Dictionary<string, (bool isTainted, bool isSanitized)> parameterStates = [];

        /// <summary>
        /// Binding attributes that introduce tainted data into controllers.
        /// </summary>
        private static readonly string[] taintedBindingAttributes = ["FromBody", "FromQuery", "FromRoute", "FromHeader", "FromForm"];

        /// <summary>
        /// Initialize tracker with method parameters, marking those with tainted binding attributes.
        /// </summary>
        public ControllerParameterTracker(MethodDeclarationSyntax method)
        {
            foreach (var parameter in method.ParameterList.Parameters)
            {
                var isTainted = parameter.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => taintedBindingAttributes.Contains(GetAttributeName(attr)));

                parameterStates[parameter.Identifier.Text] = (isTainted, false);
            }
        }

        /// <summary>
        /// Track an assignment statement, updating parameter taint state.
        /// </summary>
        public void TrackAssignment(AssignmentExpressionSyntax assignment)
        {
            var leftIdentifier = (assignment.Left as IdentifierNameSyntax)?.Identifier.Text;
            if (leftIdentifier == null)
                return;

            var rightIdentifier = (assignment.Right as IdentifierNameSyntax)?.Identifier.Text;
            if (rightIdentifier != null && parameterStates.TryGetValue(rightIdentifier, out var rightState))
            {
                parameterStates[leftIdentifier] = rightState;
            }
        }

        /// <summary>
        /// Query if an identifier is currently tainted.
        /// </summary>
        public bool IsTainted(string identifier)
        {
            return parameterStates.TryGetValue(identifier, out var state) && state is { isTainted: true, isSanitized: false };
        }

        /// <summary>
        /// Query if an identifier has been sanitized.
        /// </summary>
        public bool IsSanitized(string identifier)
        {
            return parameterStates.TryGetValue(identifier, out var state) && state.isSanitized;
        }

        /// <summary>
        /// Mark an identifier as sanitized after passing through a sanitizing method.
        /// </summary>
        public void MarkSanitized(string identifier)
        {
            if (parameterStates.TryGetValue(identifier, out var state))
            {
                parameterStates[identifier] = (state.isTainted, true);
            }
        }

        /// <summary>
        /// Analyze an expression for SQL injection vectors using tainted parameters.
        /// Returns true if a vulnerability is detected.
        /// </summary>
        public bool FindSqlInjectionVectorsInExpression(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => IsTainted(identifier.Identifier.Text),
                BinaryExpressionSyntax binary => ContainsTaintedComponent(binary),
                InvocationExpressionSyntax invocation => ContainsTaintedComponent(invocation),
                _ => false
            };
        }

        /// <summary>
        /// Analyze an expression for tainted parameters that flow to logging or activity sinks.
        /// Returns true if unsanitized tainted data is detected.
        /// </summary>
        public bool FindLoggingInjectionVectorsInExpression(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => IsTainted(identifier.Identifier.Text),
                BinaryExpressionSyntax binary => ContainsTaintedComponent(binary),
                _ => false
            };
        }

        private bool ContainsTaintedComponent(BinaryExpressionSyntax binary)
        {
            var components = SqlOperationDetector.GetConcatenationComponents(binary);
            return components.Any(FindSqlInjectionVectorsInExpression);
        }

        private bool ContainsTaintedComponent(InvocationExpressionSyntax invocation)
        {
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (FindSqlInjectionVectorsInExpression(arg.Expression))
                    return true;
            }

            return false;
        }

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
