---
title: Initial Unit Baseline: 2026-09-09
doc_type: report
status: active
last_updated: 2026-09-10
---

# Initial Unit Baseline: 2026-09-09

Command: `pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1`

Outcome: **aborted**, not a completed full-suite verification. The test host consumed
more than eight minutes of CPU without another result and was stopped to release the
shared test slot. An attempted stack diagnostic returned no usable trace. The
stalled test is not identified by this TRX. Future wrapper runs have a two-minute
VSTest hang detector with a sequence artifact and no memory dump.

Recorded TRX results: **2,239 total, 2,238 executed, 2,191 passed, 47 failed,
1 skipped**. The total is the recorded result count, not proof that all tests were
discovered or finished; data rows also differ from distinct test definitions.
There are 2,233 test definitions in the partial TRX. No coverage was collected.

Artifact: `TestResults/tests/vc.Ifx.UnitTests/11dcafa6a2ec4bf99266c30e1e2e44ac/tests.trx`.
This reflects the binaries built during concurrent implementation, not later edits.
The first build attempt stopped on a transient missing Storage.Abstractions README
before discovery. Test builds now disable package generation; no package code was
edited for this baseline. Compiler/analyzer warnings remain a separate global gate.

## Failures To Route

### Authorization.AuthorizationServiceCollectionExtensionsTests.AddMultipleAuthorizationPolicies_ShouldRegisterAll

```text
Test method VisionaryCoder.Framework.Tests.Authorization.AuthorizationServiceCollectionExtensionsTests.AddMultipleAuthorizationPolicies_ShouldRegisterAll threw exception:
System.InvalidOperationException: Unable to resolve service for type 'System.Collections.Generic.ICollection`1[System.String]' while attempting to activate 'VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies.RoleBasedAuthorizationPolicy'.
```

### Authorization.AuthorizationServiceCollectionExtensionsTests.AddRoleBasedAuthorizationPolicy_ShouldRegisterCorrectly

```text
Test method VisionaryCoder.Framework.Tests.Authorization.AuthorizationServiceCollectionExtensionsTests.AddRoleBasedAuthorizationPolicy_ShouldRegisterCorrectly threw exception:
System.InvalidOperationException: Unable to resolve service for type 'System.Collections.Generic.ICollection`1[System.String]' while attempting to activate 'VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies.RoleBasedAuthorizationPolicy'.
```

### Caching.CachingServiceCollectionExtensionsTests.AddCaching_WithGenericCache_ShouldRegisterSpecifiedCache

```text
Test method VisionaryCoder.Framework.Tests.Caching.CachingServiceCollectionExtensionsTests.AddCaching_WithGenericCache_ShouldRegisterSpecifiedCache threw exception:
System.InvalidOperationException: Unable to resolve service for type 'VisionaryCoder.Framework.Proxy.Interceptors.Caching.CachingOptions' while attempting to activate 'VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.DefaultCachePolicyProvider'.
```

### Caching.CachingServiceCollectionExtensionsTests.AddCaching_WithGenericProviders_ShouldRegisterSpecifiedProviders

```text
Test method VisionaryCoder.Framework.Tests.Caching.CachingServiceCollectionExtensionsTests.AddCaching_WithGenericProviders_ShouldRegisterSpecifiedProviders threw exception:
System.InvalidOperationException: Unable to resolve service for type 'VisionaryCoder.Framework.Proxy.Interceptors.Caching.CachingOptions' while attempting to activate 'VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.DefaultCachePolicyProvider'.
```

### Extensions.CollectionExtensionsTests.TryGetElement_WithNullCollection_ShouldThrowArgumentNullException

```text
Expected a <System.ArgumentNullException> to be thrown, but found <System.NullReferenceException>:
System.NullReferenceException: Object reference not set to an instance of an object.
   at VisionaryCoder.Framework.Extensions.CollectionExtensions.TryGetElement[T](ICollection`1 collection, Int32 index, T& value) in C:\dev\a\vc.Ifx\src\vc.Ifx\Extensions\CollectionExtensions.cs:line 50
   at VisionaryCoder.Framework.Tests.Extensions.CollectionExtensionsTests.<>c__DisplayClass17_0.<TryGetElement_WithNullCollection_ShouldThrowArgumentNullException>b__0() in C:\dev\a\vc.Ifx\tests\unit\vc.Ifx.UnitTests\Extensions\CollectionExtensionsTests.cs:line 264
   at FluentAssertions.Specialized.FunctionAssertions`1.InvokeSubject()
   at FluentAssertions.Specialized.DelegateAssertions`2.InvokeSubjectWithInterception().
```

### Extensions.DictionaryExtensionsTests.AddOrUpdate_WithValueFactory_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() =>
            dictionary!.AddOrUpdate("key", addValueFactory, updateValueFactory)'.
```

### Extensions.DictionaryExtensionsTests.AddToList_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.AddToList("key", 1)'.
```

### Extensions.DictionaryExtensionsTests.ForEach_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.ForEach(action)'.
```

### Extensions.DictionaryExtensionsTests.IncrementValue_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.IncrementValue("key")'.
```

### Extensions.DictionaryExtensionsTests.Invert_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.Invert()'.
```

### Extensions.DictionaryExtensionsTests.Merge_WithNullFirst_ShouldThrowArgumentNullException

```text
Expected exception.ParamName to be a match with the expectation, but it differs at index 0:
   ↓ (actual)
  "dictionary"
  "first"
   ↑ (expected)
```

### Extensions.DictionaryExtensionsTests.RemoveRange_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.RemoveRange(keys)'.
```

### Extensions.DictionaryExtensionsTests.ToImmutableDictionary_WithNullDictionary_ShouldThrowArgumentNullException

```text
Expected exception.ParamName to be a match with the expectation, but it differs at index 0:
   ↓ (actual)
  "source"
  "dictionary"
   ↑ (expected)
```

### Extensions.DictionaryExtensionsTests.TransformValues_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.TransformValues(valueSelector)'.
```

### Extensions.DictionaryExtensionsTests.TryRemove_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.TryRemove("key", out _)'.
```

### Extensions.DictionaryExtensionsTests.TryUpdate_WithNullDictionary_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => dictionary!.TryUpdate("key", 1)'.
```

### Extensions.DictionaryExtensionsTests.Where_WithNullDictionary_ShouldThrowArgumentNullException

```text
Expected exception.ParamName to be a match with the expectation, but it differs at index 0:
   ↓ (actual)
  "source"
  "dictionary"
   ↑ (expected)
```

### Extensions.EnumerableExtensionsTests.Batch_WithNullSource_ShouldThrowArgumentNullException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.ArgumentNullException>. Actual exception type:<System.NullReferenceException>. 'action' expression: '() => source!.Batch(2).ToList()'.
```

### Extensions.EnumerableExtensionsTests.ToDelimitedString_WithNullSource_ShouldThrowArgumentNullException

```text
Expected exception.ParamName to be "source", but "values" differs near "val" (index 0).
```

### Extensions.MonthExtensionsTests.MonthExtensions_YearBoundaryNavigation_ShouldWorkCorrectly

```text
Expected january.Name to be "Unknown", but "January" differs near "Jan" (index 0).
```

### Extensions.MonthExtensionsTests.Previous_WithJanuary_ShouldReturnDecember

```text
Expected result.Name to be "Unknown" with a length of 7, but "December" has a length of 8, differs near "Dec" (index 0).
```

### Extensions.MonthExtensionsTests.Previous_WithUnknown_ShouldReturnDecember

```text
Expected result.Name to be "December" with a length of 8, but "Unknown" has a length of 7, differs near "Unk" (index 0).
```

### Extensions.MonthTests.Constants_ShouldHaveCorrectValues

```text
Expected Month.Unknown to be "???", but found Unknown.
```

### Extensions.ReflectionExtensionsTests.InvokeMethod_WithMethodThatThrows_ShouldPropagateException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.Reflection.TargetInvocationException>. Actual exception type:<System.MissingMethodException>. 'action' expression: '() => obj.InvokeMethod(methodName)'.
```

### Extensions.ReflectionExtensionsTests.InvokeMethod_WithOverloadedMethod_ShouldThrowAmbiguousMatchException

```text
Assert.ThrowsExactly failed. Expected exception type:<System.Reflection.AmbiguousMatchException>. Actual exception type:<System.MissingMethodException>. 'action' expression: '() => obj.InvokeMethod(methodName)'.
```

### Extensions.ReflectionExtensionsTests.InvokeMethod_WithStaticLikeInstance_ShouldWork

```text
Test method VisionaryCoder.Framework.Tests.Extensions.ReflectionExtensionsTests.InvokeMethod_WithStaticLikeInstance_ShouldWork threw exception:
System.MissingMethodException: GetValue
```

### Extensions.ReflectionExtensionsTests.ReflectionExtensions_ComplexScenario_ShouldWorkTogether

```text
Expected implementsDisposable to be True, but found False.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAllWithComplexPredicate_ShouldCreateCollectionCondition

```text
Test method VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.Translate_WithAllWithComplexPredicate_ShouldCreateCollectionCondition threw exception:
System.NotSupportedException: Expression 'e => e.Children.All(c => (c.IsActive AndAlso (c.Value >= 0)))' is not supported.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAllWithPredicate_ShouldCreateCollectionCondition

```text
Test method VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.Translate_WithAllWithPredicate_ShouldCreateCollectionCondition threw exception:
System.NotSupportedException: Expression 'e => e.Children.All(c => (c.Value > 0))' is not supported.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAndAlsoExpression_ShouldCreateFilterGroup

```text
Expected group.Children to contain 2 item(s), but found 1: {
    VisionaryCoder.Framework.Filtering.Abstractions.FilterCondition
    {
        Operator = FilterOperation.GreaterThan {value: 2},
        Path = "Age",
        Value = "18"
    }
}.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithComplexPredicate_ShouldCreateCollectionCondition

```text
Test method VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithComplexPredicate_ShouldCreateCollectionCondition threw exception:
System.NotSupportedException: Expression 'e => e.Children.Any(c => ((c.Value > 5) AndAlso c.IsActive))' is not supported.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithSimplePredicate_ShouldCreateCollectionCondition

```text
Test method VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithSimplePredicate_ShouldCreateCollectionCondition threw exception:
System.NotSupportedException: Expression 'e => e.Children.Any(c => (c.Value > 10))' is not supported.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithStringPredicate_ShouldCreateCollectionCondition

```text
Test method VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.Translate_WithAnyWithStringPredicate_ShouldCreateCollectionCondition threw exception:
System.NotSupportedException: Expression 'e => e.Children.Any(c => c.Name.Contains("test"))' is not supported.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithCombinedAnyAndComparison_ShouldCreateFilterGroup

```text
Expected group.Children to contain 2 item(s), but found 1: {
    VisionaryCoder.Framework.Filtering.Abstractions.FilterCondition
    {
        Operator = FilterOperation.GreaterThan {value: 2},
        Path = "Age",
        Value = "18"
    }
}.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithMultipleAnyConditions_ShouldCreateFilterGroup

```text
Expected group.Children to contain 2 item(s), but found 0: {empty}.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithNestedCollectionOperations_ShouldCreateComplexStructure

```text
Expected group.Children to contain 3 item(s), but found 2: {
    VisionaryCoder.Framework.Filtering.Abstractions.FilterCondition
    {
        Operator = FilterOperation.Contains {value: 6},
        Path = "Name",
        Value = "test"
    },
    VisionaryCoder.Framework.Filtering.Abstractions.FilterCollectionCondition
    {
        Operator = FilterOperation.HasElements {value: 11},
        Path = "Tags",
        Predicate = <null>
    }
}.
```

### Filtering.ExpressionToFilterNodeTests.Translate_WithNullExpression_ShouldThrowException

```text
Expected a <System.ArgumentNullException> to be thrown, but found <System.NullReferenceException>:
System.NullReferenceException: Object reference not set to an instance of an object.
   at VisionaryCoder.Framework.Filtering.ExpressionToFilterNode.Translate[T](Expression`1 expression) in C:\dev\a\vc.Ifx\src\vc.Ifx.Filtering\ExpressionToFilterNode.cs:line 29
   at VisionaryCoder.Framework.Tests.Filtering.ExpressionToFilterNodeTests.<>c__DisplayClass22_0.<Translate_WithNullExpression_ShouldThrowException>b__0() in C:\dev\a\vc.Ifx\tests\unit\vc.Ifx.UnitTests\Filtering\ExpressionToFilterNodeTests.cs:line 436
   at FluentAssertions.Specialized.ActionAssertions.InvokeSubject()
   at FluentAssertions.Specialized.DelegateAssertions`2.InvokeSubjectWithInterception().
```

### FrameworkInfoProviderTests.GetCompilationTime_ShouldReturnAssemblyCreationTime

```text
Expected actualTime.DateTime to be within 1s from <2026-09-09 21:05:04.7107549>, but <2026-09-09 21:05:02.9227456> was off by 1s, 788ms and 9.3µs.
```

### Proxy.AuditRecordTests.Constructor_ShouldInitializeWithDefaults

```text
Expected default, but found <null>.
```

### Proxy.DefaultProxyPipelineTests.SendAsync_WithAttributeBasedOrder_ShouldRespectOrderAttribute

```text
Expected callOrder {"High", "Low"} to contain items {"Low", "High"} in order, but "High" (index 1) did not appear (in the right order).
```

### Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_FirstCall_ShouldCacheMiss

```text
Test method VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_FirstCall_ShouldCacheMiss threw exception:
System.Collections.Generic.KeyNotFoundException: The given key 'CacheHit' was not present in the dictionary.
```

### Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_SecondCall_ShouldCacheHit

```text
Expected callCount to be 1, but found 2.
```

### Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_WithCustomCacheDuration_ShouldUseCustomDuration

```text
Test method VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_WithCustomCacheDuration_ShouldUseCustomDuration threw exception:
System.Collections.Generic.KeyNotFoundException: The given key 'CacheHit' was not present in the dictionary.
```

### Proxy.Interceptors.Caching.CachingInterceptorTests.InvokeAsync_WithCustomKeyGenerator_ShouldUseCustomKey

```text
Expected callCount to be 1, but found 2.
```

### Proxy.Interceptors.Correlation.CorrelationInterceptorTests.InvokeAsync_WithoutCorrelationId_ShouldGenerateNew

```text
Test method VisionaryCoder.Framework.Tests.Proxy.Interceptors.Correlation.CorrelationInterceptorTests.InvokeAsync_WithoutCorrelationId_ShouldGenerateNew threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: c => c.SetCorrelationId("generated-456")

Performed invocations:

   Mock<ICorrelationContext:6> (c):

      ICorrelationContext.CorrelationId
      ICorrelationContext.CorrelationId = "generated-456"
```

### Proxy.Interceptors.Logging.TimingInterceptorTests.InvokeAsync_WithSlowOperation_ShouldLogWarning

```text
Test method VisionaryCoder.Framework.Tests.Proxy.Interceptors.Logging.TimingInterceptorTests.InvokeAsync_WithSlowOperation_ShouldLogWarning threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: x => x.Log<It.IsAnyType>(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Slow proxy operation")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>())

Performed invocations:

   Mock<ILogger<TimingInterceptor>:3> (x):

      ILogger.Log<FormattedLogValues>(LogLevel.Warning, 0, Slow performance: Proxy operation 'SlowOp' completed successfully in 1174ms (>= 1000ms). Correlation ID: 'corr-456', null, Func<FormattedLogValues, Exception, string>)
```

### Querying.Serialization.QueryFilterSchemaTests.Schema_ShouldBeAccessible

```text
Expected schema "{
  "json.schemas": [
    {
      "fileMatch": [
        "**/queries/*.json",
        "**/filters/*.json"
      ],
      "url": "./.json/schemas/queryfilter.schema.json"
    }
  ]
}
" to contain "$schema".
```
