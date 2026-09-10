---
title: Writing MSTest Tests Examples
description: Detailed examples for MSTest assertions, data-driven tests, lifecycle usage, and analyzer-friendly patterns.
doc_type: reference
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1280
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_docs:
  - .github/skills/writing-mstest-tests/SKILL.md
tags:
  - mstest
  - examples
  - dotnet
---
# Writing MSTest Tests Examples

Agent reads this file when the user asks for fuller MSTest examples than the main skill can hold.

## Compact Example Table

| Topic | Example |
|---|---|
| AAA structure | ```csharp\n[TestMethod]\npublic void CalculateTotal_WithDiscount_ReturnsReducedPrice()\n{\n    var sut = new OrderService();\n    var order = new Order { Price = 100m, DiscountPercent = 10 };\n\n    var total = sut.CalculateTotal(order);\n\n    Assert.AreEqual(90m, total);\n}\n``` |
| Exception assertion | ```csharp\n[TestMethod]\npublic void Process_WhenInputIsNull_Throws()\n{\n    var sut = new Processor();\n\n    var ex = Assert.ThrowsExactly<ArgumentNullException>(() => sut.Process(null!));\n\n    Assert.AreEqual("input", ex.ParamName);\n}\n``` |
| Async exception assertion | ```csharp\n[TestMethod]\npublic async Task ProcessAsync_WhenStateIsInvalid_Throws()\n{\n    var sut = new Processor();\n\n    await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => sut.ProcessAsync());\n}\n``` |
| `DataRow` | ```csharp\n[TestMethod]\n[DataRow(1, 2, 3)]\n[DataRow(-1, 1, 0)]\npublic void Add_ReturnsExpectedSum(int a, int b, int expected)\n{\n    Assert.AreEqual(expected, Calculator.Add(a, b));\n}\n``` |
| `DynamicData` tuple | ```csharp\n[TestMethod]\n[DynamicData(nameof(Cases))]\npublic void ApplyDiscount_ReturnsExpectedPrice(decimal price, int percent, decimal expected)\n{\n    Assert.AreEqual(expected, PriceCalculator.ApplyDiscount(price, percent));\n}\n\npublic static IEnumerable<(decimal price, int percent, decimal expected)> Cases =>\n[\n    (100m, 10, 90m),\n    (50m, 0, 50m),\n];\n``` |
| Cancellation-aware async test | ```csharp\n[TestMethod]\n[Timeout(5000)]\npublic async Task FetchAsync_ReturnsData(CancellationToken ct = default)\n{\n    var testContext = TestContext;\n    var result = await client.FetchAsync(testContext.CancellationToken);\n    Assert.IsNotNull(result);\n}\n``` |

## Lifecycle Patterns

| Need | Example |
|---|---|
| Constructor plus async init | ```csharp\n[TestClass]\npublic sealed class RepositoryTests\n{\n    private readonly FakeDatabase db;\n\n    public RepositoryTests()\n    {\n        db = new FakeDatabase();\n    }\n\n    [TestInitialize]\n    public async Task InitializeAsync()\n    {\n        await db.SeedAsync();\n    }\n}\n``` |
| Cleanup | ```csharp\n[TestCleanup]\npublic void Cleanup()\n{\n    db.Reset();\n}\n``` |

## Assertion Upgrade Table

| Weak form | Strong form |
|---|---|
| `Assert.IsTrue(list.Count > 0)` | `Assert.IsNotEmpty(list)` |
| `Assert.IsTrue(result != null)` | `Assert.IsNotNull(result)` |
| `Assert.IsTrue(items.Contains(expected))` | `Assert.Contains(expected, items)` |
| `Assert.ThrowsException<InvalidOperationException>(...)` | `Assert.ThrowsExactly<InvalidOperationException>(...)` when exact type matters |

## Analyzer-Friendly Patterns

| Diagnostic shape | Example fix |
|---|---|
| Swapped `AreEqual` args | `Assert.AreEqual(expected, actual);` |
| Old `DataTestMethod` | `TestMethod` plus `DataRow` |
| Duplicate attributes | Keep one `[TestMethod]` or one `[DataRow]` instance |

## Moq Example

```csharp
[TestMethod]
public async Task SendAsync_WhenGatewayAcceptsRequest_ReturnsReceipt()
{
    var gatewayMock = new Mock<IPaymentGateway>();
    gatewayMock
        .Setup(x => x.SendAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaymentReceipt("ok"));

    var sut = new PaymentService(gatewayMock.Object);

    var result = await sut.SendAsync(new PaymentRequest("123"), CancellationToken.None);

    Assert.AreEqual("ok", result.Code);
    gatewayMock.Verify(x => x.SendAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Once);
}
```
