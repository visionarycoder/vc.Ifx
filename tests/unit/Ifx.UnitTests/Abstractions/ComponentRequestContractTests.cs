using Ifx.Component;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ifx.Tests.Abstractions;

[TestClass]
public sealed class ComponentRequestContractTests
{
    [TestMethod]
    public void GenericComponentRequestRemainsAnInheritableLoggerBackedMarker()
    {
        var request = new ComponentRequest<string>(NullLogger<ComponentRequest>.Instance);

        request.Should().BeAssignableTo<ComponentRequest>();
        request.Should().NotBeNull();
    }
}
