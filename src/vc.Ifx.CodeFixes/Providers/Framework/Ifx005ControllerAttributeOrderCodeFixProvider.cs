using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ifx005ControllerAttributeOrderCodeFixProvider))]
    [Shared]
    public sealed class Ifx005ControllerAttributeOrderCodeFixProvider : CodeFixProvider
    {
        private const string ReorderTitle = "Reorder controller attributes to preferred order";
        private const string RemoveScopeTitle = "Remove RequiredScope when Authorize(Roles=...) is present";

        /// <summary>
        /// Gets the diagnostic identifiers that this provider can fix.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.Ifx005ControllerAttributeOrder);

        /// <summary>
        /// Gets the fix-all provider used to apply this code fix across a document, project, or solution.
        /// </summary>
        /// <returns>
        /// A batch fix-all provider for this code fix.
        /// </returns>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Registers code actions for the reported controller attribute ordering diagnostic.
        /// </summary>
        /// <param name="context">
        /// The context that contains the document, diagnostics, and cancellation token for registration.
        /// </param>
        /// <returns>
        /// A task that completes when all applicable code fixes have been registered.
        /// </returns>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(ReorderTitle, cancellationToken => ReorderAsync(context.Document, root, classDeclaration), ReorderTitle),
                diagnostic);

            if (HasRoleAuthorize(classDeclaration) && HasRequiredScope(classDeclaration))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(RemoveScopeTitle, cancellationToken => RemoveRequiredScopeAsync(context.Document, root, classDeclaration), RemoveScopeTitle),
                    diagnostic);
            }
        }

        /// <summary>
        /// Reorders controller attributes according to the preferred framework-specific ordering.
        /// </summary>
        /// <param name="document">
        ///     The document being updated.
        /// </param>
        /// <param name="root">
        ///     The current syntax root for the document.
        /// </param>
        /// <param name="classDeclaration">
        ///     The controller class declaration whose attributes will be reordered.
        /// </param>
        /// <returns>
        /// A task that produces a document with reordered controller attributes.
        /// </returns>
        private static Task<Document> ReorderAsync(Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration)
        {
            var preferredOrder = new Dictionary<string, int>
            {
                ["Authorize"] = 0,
                ["AllowAnonymous"] = 1,

                ["ApiVersion"] = 10,
                ["Route"] = 20,
                ["ApiController"] = 30,

                ["RequiredScope"] = 40,
                ["RequiredScopeOrAppPermission"] = 41,
                ["FeatureGate"] = 50,

                ["EnableRateLimiting"] = 60,
                ["DisableRateLimiting"] = 61,

                ["EnableCors"] = 70,
                ["DisableCors"] = 71,

                ["Produces"] = 80,
                ["Consumes"] = 81,
                ["ProducesResponseType"] = 82,

                ["ApiExplorerSettings"] = 90
            };

            var attributes = classDeclaration.AttributeLists
               .SelectMany(x => x.Attributes)
               .ToList();

            var sorted = attributes
               .OrderBy(a => preferredOrder.TryGetValue(GetAttributeName(a), out var rank) ? rank : int.MaxValue)
               .ThenBy(a => a.GetLocation().SourceSpan.Start)
               .ToList();

            var reorderedLists = SyntaxFactory.List(sorted.Select(attribute => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))));
            var updatedClass = classDeclaration.WithAttributeLists(reorderedLists);
            var updatedRoot = root.ReplaceNode(classDeclaration, updatedClass);
            return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
        }

        /// <summary>
        /// Removes <c>RequiredScope</c> attributes from the controller when a role-based
        /// <c>Authorize</c> attribute is already present.
        /// </summary>
        /// <param name="document">
        /// The document being updated.
        /// </param>
        /// <param name="root">
        /// The current syntax root for the document.
        /// </param>
        /// <param name="classDeclaration">
        /// The controller class declaration whose attributes will be filtered.
        /// </param>
        /// <returns>
        /// A task that produces a document with redundant <c>RequiredScope</c> attributes removed.
        /// </returns>
        private static Task<Document> RemoveRequiredScopeAsync(Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration)
        {
            var updatedAttributeLists = SyntaxFactory.List(classDeclaration.AttributeLists
               .Select(list =>
                {
                    var filteredAttributes = list.Attributes.Where(a => GetAttributeName(a) != "RequiredScope").ToList();
                    return filteredAttributes.Count == 0 ? null : SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(filteredAttributes));
                })
               .Where(list => list != null)
               .Cast<AttributeListSyntax>());

            var updatedClass = classDeclaration.WithAttributeLists(updatedAttributeLists);
            var updatedRoot = root.ReplaceNode(classDeclaration, updatedClass);
            return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
        }

        /// <summary>
        /// Determines whether the controller declares an <c>Authorize</c> attribute with a named
        /// <c>Roles</c> argument.
        /// </summary>
        /// <param name="classDeclaration">
        /// The controller class declaration to inspect.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when a role-based <c>Authorize</c> attribute is present; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool HasRoleAuthorize(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
               .SelectMany(l => l.Attributes)
               .Any(a => GetAttributeName(a) == "Authorize" && a.ArgumentList?.Arguments.Any(arg => arg.NameEquals?.Name.Identifier.Text == "Roles") == true);
        }

        /// <summary>
        /// Determines whether the controller declares a <c>RequiredScope</c> attribute.
        /// </summary>
        /// <param name="classDeclaration">
        /// The controller class declaration to inspect.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when a <c>RequiredScope</c> attribute is present; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool HasRequiredScope(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
               .SelectMany(l => l.Attributes)
               .Any(a => GetAttributeName(a) == "RequiredScope");
        }

        /// <summary>
        /// Gets the simplified attribute name without namespace qualification or the optional
        /// <c>Attribute</c> suffix.
        /// </summary>
        /// <param name="attribute">
        /// The attribute syntax to normalize.
        /// </param>
        /// <returns>
        /// The normalized attribute type name used for comparisons and ordering.
        /// </returns>
        private static string GetAttributeName(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();

            if (name.EndsWith("Attribute"))
            {
                name = name.Substring(0, name.Length - 9);
            }

            var lastDot = name.LastIndexOf('.');
            if (lastDot >= 0)
            {
                name = name.Substring(lastDot + 1);
            }

            return name;
        }
    }
}