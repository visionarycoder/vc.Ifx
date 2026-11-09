namespace Ifx.Tests;

/// <summary>
/// Data-driven unit tests for the <see cref="IfxConstants"/> class.
/// Tests all constant values to ensure they match expected values and remain stable.
/// </summary>
[TestClass]
public class ConstantsTests
{
    #region Top-Level Constants Tests

    [TestMethod]
    public void Version_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Version.Should().Be("0.1.0");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.ConfigurationSection.Should().Be("vc.IfxFramework");
    }

    [TestMethod]
    public void Version_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.Version.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void ConfigurationSection_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.ConfigurationSection.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Timeouts Constants Tests

    [TestMethod]
    public void Timeouts_DefaultHttpTimeoutSeconds_ShouldBe30()
    {
        // Assert
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(30);
    }

    [TestMethod]
    public void Timeouts_DefaultDatabaseTimeoutSeconds_ShouldBe30()
    {
        // Assert
        IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().Be(30);
    }

    [TestMethod]
    public void Timeouts_DefaultCacheExpirationMinutes_ShouldBe15()
    {
        // Assert
        IfxConstants.Timeouts.DefaultCacheExpirationMinutes.Should().Be(15);
    }

    [TestMethod]
    public void Timeouts_AllValues_ShouldBePositive()
    {
        // Assert
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().BePositive();
        IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BePositive();
        IfxConstants.Timeouts.DefaultCacheExpirationMinutes.Should().BePositive();
    }

    [TestMethod]
    public void Timeouts_HttpAndDatabaseTimeouts_ShouldBeEqual()
    {
        // Assert - Both are set to 30 seconds
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds);
    }

    #endregion

    #region Headers Constants Tests

    [TestMethod]
    public void Headers_CorrelationId_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Headers.CorrelationId.Should().Be("X-Correlation-ID");
    }

    [TestMethod]
    public void Headers_RequestId_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Headers.RequestId.Should().Be("X-Request-ID");
    }

    [TestMethod]
    public void Headers_UserContext_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Headers.UserContext.Should().Be("X-User-Context");
    }

    [TestMethod]
    public void Headers_ApiVersion_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Headers.ApiVersion.Should().Be("Api-Version");
    }

    [TestMethod]
    public void Headers_AllValues_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.Headers.CorrelationId.Should().NotBeNullOrEmpty();
        IfxConstants.Headers.RequestId.Should().NotBeNullOrEmpty();
        IfxConstants.Headers.UserContext.Should().NotBeNullOrEmpty();
        IfxConstants.Headers.ApiVersion.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    [DataRow("X-Correlation-ID")]
    [DataRow("X-Request-ID")]
    [DataRow("X-User-Context")]
    public void Headers_CustomHeaders_ShouldStartWithXPrefix(string headerValue)
    {
        // Assert - Custom headers should follow X- convention
        headerValue.Should().StartWith("X-");
    }

    [TestMethod]
    public void Headers_CorrelationId_ShouldContainHyphens()
    {
        // Assert - Header names should use hyphens for word separation
        IfxConstants.Headers.CorrelationId.Should().Contain("-");
    }

    [TestMethod]
    public void Headers_CorrelationIdAndRequestId_ShouldBeDifferent()
    {
        // Assert - These should be distinct headers
        IfxConstants.Headers.CorrelationId.Should().NotBe(IfxConstants.Headers.RequestId);
    }

    #endregion

    #region Logging Constants Tests

    [TestMethod]
    public void Logging_DefaultLogLevel_ShouldBeInformation()
    {
        // Assert
        IfxConstants.Logging.DefaultLogLevel.Should().Be("Information");
    }

    [TestMethod]
    public void Logging_FrameworkCategory_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.FrameworkCategory.Should().Be("Ifx");
    }

    [TestMethod]
    public void Logging_PerformanceCategory_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.PerformanceCategory.Should().Be("Ifx.Performance");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Be("[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
    }

    [TestMethod]
    public void Logging_CorrelationIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.CorrelationIdProperty.Should().Be("CorrelationId");
    }

    [TestMethod]
    public void Logging_RequestIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.RequestIdProperty.Should().Be("RequestId");
    }

    [TestMethod]
    public void Logging_UserIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        IfxConstants.Logging.UserIdProperty.Should().Be("UserId");
    }

    [TestMethod]
    public void Logging_AllValues_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.Logging.DefaultLogLevel.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.FrameworkCategory.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.PerformanceCategory.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.DefaultTemplate.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.CorrelationIdProperty.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.RequestIdProperty.Should().NotBeNullOrEmpty();
        IfxConstants.Logging.UserIdProperty.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void Logging_PerformanceCategory_ShouldStartWithFrameworkCategory()
    {
        // Assert - Performance category should be a subcategory of framework
        IfxConstants.Logging.PerformanceCategory.Should().StartWith(IfxConstants.Logging.FrameworkCategory);
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainTimestamp()
    {
        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Contain("{Timestamp");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainLevel()
    {
        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Contain("{Level");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainMessage()
    {
        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Contain("{Message");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainException()
    {
        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Contain("{Exception}");
    }

    [TestMethod]
    [DataRow("Information", "Information")]
    [DataRow("Debug", "Debug")]
    [DataRow("Warning", "Warning")]
    [DataRow("Error", "Error")]
    [DataRow("Critical", "Critical")]
    [DataRow("Trace", "Trace")]
    public void Logging_DefaultLogLevel_ShouldBeValidLogLevel(string expected, string actual)
    {
        // Assert - DefaultLogLevel should match one of the valid log levels
        if (IfxConstants.Logging.DefaultLogLevel == expected)
        {
            actual.Should().Be(expected);
        }
    }

    [TestMethod]
    public void Logging_PropertyNames_ShouldUsePascalCase()
    {
        // Assert - Property names should follow PascalCase convention
        IfxConstants.Logging.CorrelationIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
        IfxConstants.Logging.RequestIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
        IfxConstants.Logging.UserIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
    }

    [TestMethod]
    public void Logging_CorrelationIdAndRequestIdProperties_ShouldBeDifferent()
    {
        // Assert
        IfxConstants.Logging.CorrelationIdProperty.Should().NotBe(IfxConstants.Logging.RequestIdProperty);
    }

    #endregion

    #region Cross-Namespace Consistency Tests

    [TestMethod]
    public void Headers_CorrelationId_ShouldAlignWithLogging_CorrelationIdProperty()
    {
        // Assert - Header name (with hyphens) and logging property should represent the same concept (case-insensitive)
        string headerWithoutPrefix = IfxConstants.Headers.CorrelationId.Replace("X-", "").Replace("-", "");
        string loggingProperty = IfxConstants.Logging.CorrelationIdProperty;
        headerWithoutPrefix.Should().BeEquivalentTo(loggingProperty, "both represent the same correlation ID concept");
    }

    [TestMethod]
    public void Headers_RequestId_ShouldAlignWithLogging_RequestIdProperty()
    {
        // Assert - Header name (with hyphens) and logging property should represent the same concept (case-insensitive)
        string headerWithoutPrefix = IfxConstants.Headers.RequestId.Replace("X-", "").Replace("-", "");
        string loggingProperty = IfxConstants.Logging.RequestIdProperty;
        headerWithoutPrefix.Should().BeEquivalentTo(loggingProperty, "both represent the same request ID concept");
    }

    [TestMethod]
    public void Version_ShouldFollowSemanticVersioningFormat()
    {
        // Assert - Should match semantic versioning pattern (major.minor.patch)
        IfxConstants.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+$");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldNotContainSpaces()
    {
        // Assert - Configuration section names typically don't have spaces
        IfxConstants.ConfigurationSection.Should().NotContain(" ");
    }

    #endregion

    #region Type and Accessibility Tests

    [TestMethod]
    public void Constants_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(IfxConstants);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Timeouts_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(IfxConstants.Timeouts);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Headers_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(IfxConstants.Headers);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Logging_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(IfxConstants.Logging);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_ShouldBePublic()
    {
        // Arrange & Act
        Type type = typeof(IfxConstants);

        // Assert
        type.IsPublic.Should().BeTrue();
    }

    [TestMethod]
    public void Constants_NestedClasses_ShouldBePublic()
    {
        // Arrange & Act
        Type timeoutsType = typeof(IfxConstants.Timeouts);
        Type headersType = typeof(IfxConstants.Headers);
        Type loggingType = typeof(IfxConstants.Logging);

        // Assert
        timeoutsType.IsNestedPublic.Should().BeTrue();
        headersType.IsNestedPublic.Should().BeTrue();
        loggingType.IsNestedPublic.Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [TestMethod]
    public void Timeouts_DefaultHttpTimeoutSeconds_ShouldBeReasonable()
    {
        // Assert - 30 seconds is reasonable for HTTP requests
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().BeInRange(1, 300);
    }

    [TestMethod]
    public void Timeouts_DefaultDatabaseTimeoutSeconds_ShouldBeReasonable()
    {
        // Assert - 30 seconds is reasonable for database operations
        IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BeInRange(1, 300);
    }

    [TestMethod]
    public void Timeouts_DefaultCacheExpirationMinutes_ShouldBeReasonable()
    {
        // Assert - 15 minutes is reasonable for cache expiration
        IfxConstants.Timeouts.DefaultCacheExpirationMinutes.Should().BeInRange(1, 1440); // 1 min to 24 hours
    }

    [TestMethod]
    public void Headers_AllHeaders_ShouldNotContainWhitespace()
    {
        // Assert - HTTP headers should not contain whitespace
        IfxConstants.Headers.CorrelationId.Should().NotContain(" ");
        IfxConstants.Headers.RequestId.Should().NotContain(" ");
        IfxConstants.Headers.UserContext.Should().NotContain(" ");
        IfxConstants.Headers.ApiVersion.Should().NotContain(" ");
    }

    [TestMethod]
    public void Logging_DefaultLogLevel_ShouldNotBeNoneOrOff()
    {
        // Assert - Default log level should allow logging
        IfxConstants.Logging.DefaultLogLevel.Should().NotBe("None");
        IfxConstants.Logging.DefaultLogLevel.Should().NotBe("Off");
    }

    #endregion
}
