using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd200ManagerNoBusinessLogic
    {
        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Vbd200ManagerNoBusinessLogic,
            title: "Managers must not contain business logic", "Manager '{0}' contains business logic", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd200.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclaration)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
                return;

            var assemblyName = methodSymbol.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
                return;

            if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Manager)
                return;

            if (methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
                return;

            var nodes = GetBodyNodes(methodDeclaration).ToList();
            if (nodes.Count == 0)
                return;

            // Control flow (loops and conditional branches) is legitimate orchestration in a Manager �
            // routing over collections and branching on results is how managers coordinate work. What
            // remains a signal of misplaced business logic is computation over domain data: an inline
            // calculation, a comparison between two business values, an aggregation, or a value derived
            // via a conditional projection. See NodeIsBusinessLogic and vbd200.md.
            var containsBusinessLogic = nodes.Any(n => NodeIsBusinessLogic(n, context.SemanticModel));

            if (!containsBusinessLogic)
                return;

            var managerName = methodSymbol.ContainingType?.Name ?? assemblyName!;
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), managerName);
            context.ReportDiagnostic(diagnostic);
        }

        private static readonly HashSet<SyntaxKind> ArithmeticKinds =
        [
            SyntaxKind.AddExpression,
        SyntaxKind.SubtractExpression,
        SyntaxKind.MultiplyExpression,
        SyntaxKind.DivideExpression,
        SyntaxKind.ModuloExpression,
        SyntaxKind.AddAssignmentExpression,
        SyntaxKind.SubtractAssignmentExpression,
        SyntaxKind.MultiplyAssignmentExpression,
        SyntaxKind.DivideAssignmentExpression,
        SyntaxKind.ModuloAssignmentExpression
        ];

        // Relational and equality comparisons. A comparison between two *business* values (both operands
        // touch a domain member) is a business rule; member-vs-literal guards (`count > 0`, `== null`,
        // `status == SomeEnum`) are routing and are not flagged.
        private static readonly HashSet<SyntaxKind> ComparisonKinds =
        [
            SyntaxKind.LessThanExpression,
        SyntaxKind.GreaterThanExpression,
        SyntaxKind.LessThanOrEqualExpression,
        SyntaxKind.GreaterThanOrEqualExpression,
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression
        ];

        // LINQ aggregation methods compute a value over a collection without an arithmetic operator token,
        // so the inline-arithmetic walk cannot see them (e.g. `orders.Sum(o => o.Amount)`).
        private static readonly HashSet<string> AggregationMethods =
            new(System.StringComparer.Ordinal) { "Sum", "Average", "Aggregate", "Min", "Max" };

        // Namespace segments that mark a symbol as cross-cutting infrastructure the Manager may freely
        // leverage. "Ifx" is this codebase's infrastructure layer; "Util*" is reserved for the same role.
        private static readonly HashSet<string> CrossCuttingNamespaceSegments =
            new(System.StringComparer.OrdinalIgnoreCase) { "Ifx", "Util", "Utils", "Utilities" };

        // Classifies a single body node as business logic (computation over domain data) vs orchestration.
        private static bool NodeIsBusinessLogic(SyntaxNode node, SemanticModel semanticModel)
        {
            switch (node)
            {
                // Inline arithmetic � flagged only when it operates on money (decimal) and is not composed
                // purely of infra values. Integer/string/date index & length bookkeeping is plumbing, and
                // string-typed '+' is concatenation � neither is decimal, so neither is flagged.
                case BinaryExpressionSyntax arithmetic when ArithmeticKinds.Contains(arithmetic.Kind()):
                    return IsDecimalArithmetic(arithmetic, semanticModel) && !OnlyTouchesCrossCuttingInfrastructure(arithmetic, semanticModel);
                case AssignmentExpressionSyntax assign when ArithmeticKinds.Contains(assign.Kind()):
                    return IsDecimalArithmetic(assign, semanticModel) && !OnlyTouchesCrossCuttingInfrastructure(assign, semanticModel);

                // A comparison between two business values is a business rule (not a member-vs-literal guard).
                case BinaryExpressionSyntax comparison when ComparisonKinds.Contains(comparison.Kind()):
                    return TouchesBusinessMember(comparison.Left, semanticModel) && TouchesBusinessMember(comparison.Right, semanticModel);

                // Aggregation over, or conditional projection of, domain data.
                case InvocationExpressionSyntax invocation:
                    return IsBusinessAggregation(invocation, semanticModel) || IsComputedProjection(invocation, semanticModel);

                default:
                    return false;
            }
        }

        // True for `Sum/Average/Aggregate/Min/Max` that produce a money (decimal) result and whose selector
        // (or, if arg-less, receiver) touches a business member � an operator-less calculation over domain
        // money. Non-decimal aggregations (max batch number, record counts) are bookkeeping, not business.
        private static bool IsBusinessAggregation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (GetInvokedMethodName(invocation, semanticModel) is not { } name || !AggregationMethods.Contains(name))
                return false;

            if (!ProducesDecimal(invocation, semanticModel))
                return false;

            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count > 0)
                return arguments.Any(argument => TouchesBusinessMember(argument.Expression, semanticModel));

            return invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && TouchesBusinessMember(memberAccess.Expression, semanticModel);
        }

        // True for `Select/SelectMany` whose selector derives a value via a conditional (ternary) over
        // business data. Plain object/anonymous mapping is an allowed Manager responsibility (see vbd200.md).
        private static bool IsComputedProjection(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (GetInvokedMethodName(invocation, semanticModel) is not ("Select" or "SelectMany"))
                return false;

            // A selector that both derives a value via a ternary and references domain data is a computed
            // projection � unlike plain object/anonymous mapping (which has no conditional). The selector
            // is classified as a whole: resolving a bare sub-expression out of its lambda context is unreliable.
            return invocation.ArgumentList.Arguments.Any(argument =>
                argument.Expression.DescendantNodesAndSelf().OfType<ConditionalExpressionSyntax>().Any()
                && TouchesBusinessMember(argument.Expression, semanticModel));
        }

        private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method)
                return method.Name;

            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                _ => null
            };
        }

        // Walks an expression and reports whether it references any cross-cutting infrastructure member
        // (Ifx.*/Util.*) and/or any business member (a domain property/field/method). Operator methods,
        // const fields, and enum members are compile-time plumbing/literals and classify as neither.
        private static (bool touchesInfrastructure, bool touchesBusiness) ClassifyMembers(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var touchesInfrastructure = false;
            var touchesBusiness = false;

            foreach (var node in expression.DescendantNodesAndSelf())
            {
                // Only members (properties, fields, methods) carry business meaning. Locals, parameters,
                // and literals are value plumbing and don't classify the expression either way.
                var symbol = semanticModel.GetSymbolInfo(node).Symbol;
                if (symbol is not (IPropertySymbol or IFieldSymbol or IMethodSymbol))
                    continue;

                // The arithmetic/comparison operators themselves (e.g. decimal's op_Addition) resolve to
                // method symbols on the operand type � they describe the operation, not a business call.
                if (symbol is IMethodSymbol { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator })
                    continue;

                // Const fields and enum members are compile-time literals, not live domain data
                // (e.g. `OrderStatus.Pending`) � comparing against them is routing, not a business rule.
                if (symbol is IFieldSymbol { IsConst: true } or IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum })
                    continue;

                // Framework (BCL) members � `string.Length`, `ICollection.Count`, `IndexOf`, indexers,
                // `DateTime.Today`/`.Date`, etc. � are shape/plumbing, not domain data, so they do not
                // make an expression "business". Domain data lives on the application's own types.
                if (IsFrameworkMember(symbol))
                    continue;

                if (IsCrossCuttingInfrastructure(symbol))
                    touchesInfrastructure = true;
                else
                    touchesBusiness = true;
            }

            return (touchesInfrastructure, touchesBusiness);
        }

        // True when an arithmetic expression is composed *purely* of cross-cutting infrastructure
        // references (Ifx.*/Util.*) and literals/locals � at least one infra member and no business
        // member. Such math is the manager composing infra-provided values, not implementing a calculation.
        private static bool OnlyTouchesCrossCuttingInfrastructure(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var (touchesInfrastructure, touchesBusiness) = ClassifyMembers(expression, semanticModel);
            return touchesInfrastructure && !touchesBusiness;
        }

        // True when an expression references at least one business member (a domain property/field/method).
        private static bool TouchesBusinessMember(ExpressionSyntax expression, SemanticModel semanticModel)
            => ClassifyMembers(expression, semanticModel).touchesBusiness;

        // True when a member is declared in the .NET BCL (a `System.*`/`Microsoft.*` type). Collection,
        // string, and date shape members are plumbing, not domain data.
        private static bool IsFrameworkMember(ISymbol symbol)
        {
            var ns = symbol.ContainingType?.ContainingNamespace ?? symbol.ContainingNamespace;
            while (ns is { IsGlobalNamespace: false, ContainingNamespace.IsGlobalNamespace: false })
                ns = ns.ContainingNamespace;
            return ns?.Name is "System" or "Microsoft";
        }

        // Money in this codebase is `decimal`. Restricting calculations to decimal isolates genuine
        // financial computation from integer/string/date index & length bookkeeping.
        private static bool IsDecimal(ITypeSymbol? type)
            => type?.SpecialType == SpecialType.System_Decimal
               || (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
                   && nullable.TypeArguments[0].SpecialType == SpecialType.System_Decimal);

        // An aggregation produces money when its result is decimal. The aggregating method's own type
        // is checked first; for selector forms (`Sum(x => x.Amount)`) the selector's result type is the
        // reliable signal, since an extension-method invocation's type can resolve via ConvertedType.
        private static bool ProducesDecimal(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var typeInfo = semanticModel.GetTypeInfo(invocation);
            if (IsDecimal(typeInfo.Type) || IsDecimal(typeInfo.ConvertedType))
                return true;

            foreach (var argument in invocation.ArgumentList.Arguments)
                if (argument.Expression is LambdaExpressionSyntax { Body: ExpressionSyntax selectorBody }
                    && IsDecimal(semanticModel.GetTypeInfo(selectorBody).Type))
                    return true;

            return false;
        }

        // An arithmetic expression operates on money when its result or either operand is decimal.
        private static bool IsDecimalArithmetic(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (IsDecimal(semanticModel.GetTypeInfo(expression).Type))
                return true;

            return expression switch
            {
                BinaryExpressionSyntax binary => IsDecimal(semanticModel.GetTypeInfo(binary.Left).Type) || IsDecimal(semanticModel.GetTypeInfo(binary.Right).Type),
                AssignmentExpressionSyntax assignment => IsDecimal(semanticModel.GetTypeInfo(assignment.Left).Type) || IsDecimal(semanticModel.GetTypeInfo(assignment.Right).Type),
                _ => false
            };
        }

        private static bool IsCrossCuttingInfrastructure(ISymbol symbol)
        {
            if (ProjectAnalyzer.GetLayer(symbol.ContainingAssembly?.Name ?? string.Empty) == Layer.Infrastructure)
                return true;

            for (var ns = symbol.ContainingNamespace; ns is { IsGlobalNamespace: false }; ns = ns.ContainingNamespace)
                if (CrossCuttingNamespaceSegments.Contains(ns.Name))
                    return true;

            return false;
        }

        private static IEnumerable<SyntaxNode> GetBodyNodes(MethodDeclarationSyntax method)
        {
            if (method.Body != null)
                foreach (var node in method.Body.DescendantNodes())
                    yield return node;

            if (method.ExpressionBody != null)
            {
                yield return method.ExpressionBody.Expression;
                foreach (var node in method.ExpressionBody.Expression.DescendantNodes())
                    yield return node;
            }
        }
    }
}
