using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using vc.Ifx.Analyzers.Helpers;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class SecurityHelperTests
{
    [TestMethod]
    [DataRow("db.FromSqlRaw(sql)", "FromSqlRaw", true, false, false)]
    [DataRow("ExecuteSqlRaw(sql)", "ExecuteSqlRaw", true, false, false)]
    [DataRow("FromSql<int>(sql)", "FromSql", false, true, false)]
    [DataRow("FromSqlInterpolated(sql)", "FromSqlInterpolated", false, true, false)]
    [DataRow("RemoveInjectionVectors(sql)", "RemoveInjectionVectors", false, false, true)]
    [DataRow("Other()", "Other", false, false, false)]
    [DataRow("(factory())()", null, false, false, false)]
    public void SqlMethodsAreRecognizedWithArgumentExtraction(string source, string? name, bool raw, bool safe, bool sanitizer)
    {
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(source);
        Assert.AreEqual(name, SqlOperationDetector.GetMethodName(invocation));
        Assert.AreEqual(raw, SqlOperationDetector.IsUnsafeSqlOperation(invocation));
        Assert.AreEqual(safe, SqlOperationDetector.IsSafeSqlOperation(invocation));
        Assert.AreEqual(sanitizer, SqlOperationDetector.IsSanitizingMethod(invocation));
        Assert.AreEqual(source.Contains("sql", StringComparison.Ordinal) ? "sql" : null, SqlOperationDetector.GetSqlCommandArgument(invocation)?.ToString());
    }

    [TestMethod]
    public void ConcatenationIncludesLeftmostOperandInSourceOrder()
    {
        var expression = (BinaryExpressionSyntax)SyntaxFactory.ParseExpression("input + \" suffix\" + other");
        CollectionAssert.AreEqual(new[] { "input", "\" suffix\"", "other" }, SqlOperationDetector.GetConcatenationComponents(expression).Select(node => node.ToString()).ToArray());
    }

    [TestMethod]
    [DataRow("class C { void Run() {} }", false)]
    [DataRow("interface I { void Run(); }", false)]
    [DataRow("[ApiController] class C { void Run() {} }", true)]
    [DataRow("class C { [HttpGet] void Run() {} }", true)]
    [DataRow("[Mvc.HttpPost] class C { void Run() {} }", true)]
    [DataRow("[global::Other] class C { void Run() {} }", false)]
    public void DetectsControllerMethodSyntax(string source, bool expected)
    {
        var method = SyntaxFactory.ParseCompilationUnit(source).DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Assert.AreEqual(expected, ControllerSecurityHelpers.IsControllerMethod(method));
    }

    [TestMethod]
    [DataRow("logger.LogInformation(input)", "LogInformation", true)]
    [DataRow("SetTag<int>(input)", "SetTag", true)]
    [DataRow("Other(input)", "Other", false)]
    [DataRow("(factory())()", null, false)]
    public void LoggingNameSyntaxIsBounded(string source, string? name, bool expected)
    {
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(source);
        Assert.AreEqual(name, ControllerSecurityHelpers.GetLoggingMethodName(invocation));
        Assert.AreEqual(expected, ControllerSecurityHelpers.IsLoggingOrActivityMethod(invocation));
        Assert.AreEqual("input", ControllerSecurityHelpers.ExtractIdentifierName(SyntaxFactory.ParseExpression("input")));
        Assert.IsNull(ControllerSecurityHelpers.ExtractIdentifierName(SyntaxFactory.ParseExpression("1")));
    }

    [TestMethod]
    public void TracksTaintedAliasesAndSanitizationState()
    {
        var method = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("void Run([FromQuery] string input, [Mvc.FromBody] string body, [global::Other] string clean) {}")!;
        var tracker = new ControllerParameterTracker(method);
        Assert.IsTrue(tracker.IsTainted("input"));
        Assert.IsTrue(tracker.IsTainted("body"));
        Assert.IsFalse(tracker.IsTainted("clean"));
        Assert.IsFalse(tracker.IsTainted("unknown"));
        Assert.IsFalse(tracker.IsSanitized("unknown"));
        tracker.TrackAssignment((AssignmentExpressionSyntax)SyntaxFactory.ParseExpression("alias = input"));
        tracker.TrackAssignment((AssignmentExpressionSyntax)SyntaxFactory.ParseExpression("obj.Value = input"));
        tracker.TrackAssignment((AssignmentExpressionSyntax)SyntaxFactory.ParseExpression("other = missing"));
        tracker.TrackAssignment((AssignmentExpressionSyntax)SyntaxFactory.ParseExpression("another = 1"));
        Assert.IsTrue(tracker.IsTainted("alias"));
        Assert.IsFalse(tracker.IsTainted("other"));
        foreach (string source in new[] { "input", "input + \"end\"", "Call(1, input)" })
            Assert.IsTrue(tracker.FindSqlInjectionVectorsInExpression(SyntaxFactory.ParseExpression(source)));
        foreach (string source in new[] { "clean", "1", "Call(1)", "1 + 2" })
            Assert.IsFalse(tracker.FindSqlInjectionVectorsInExpression(SyntaxFactory.ParseExpression(source)));
        Assert.IsTrue(tracker.FindLoggingInjectionVectorsInExpression(SyntaxFactory.ParseExpression("input")));
        Assert.IsTrue(tracker.FindLoggingInjectionVectorsInExpression(SyntaxFactory.ParseExpression("\"prefix\" + input")));
        Assert.IsFalse(tracker.FindLoggingInjectionVectorsInExpression(SyntaxFactory.ParseExpression("Call(input)")));
        tracker.MarkSanitized("input");
        tracker.MarkSanitized("unknown");
        Assert.IsTrue(tracker.IsSanitized("input"));
        Assert.IsFalse(tracker.IsTainted("input"));
        Assert.IsTrue(tracker.IsTainted("alias"));
    }
}
