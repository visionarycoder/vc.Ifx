using VisionaryCoder.Framework.Querying;
using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Tests.Querying.Serialization;

[TestClass]
public class QueryFilterSchemaTests
{
    [TestMethod]
    public void Schema_ShouldBeAccessible()
    {
        // Act
        string schema = QueryFilterSchema.Content;

        // Assert
        schema.Should().NotBeNullOrWhiteSpace();
        schema.Should().Contain("$schema");
        schema.Should().Contain("QueryFilter");
    }

    [TestMethod]
    public void Validate_WithValidPropertyFilter_ShouldReturnNoErrors()
    {
        // Arrange
        string json = """
                      {
                        "operator": "Contains",
                        "property": "name",
                        "value": "test"
                      }
                      """;

        // Act
        IReadOnlyList<string> errors = QueryFilterSchemaValidator.Validate(json);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public void Validate_WithValidCompositeFilter_ShouldReturnNoErrors()
    {
        // Arrange
        string json = """
                      {
                        "operator": "And",
                        "children": [
                          {
                            "operator": "Contains",
                            "property": "name",
                            "value": "test"
                          }
                        ]
                      }
                      """;

        // Act
        IReadOnlyList<string> errors = QueryFilterSchemaValidator.Validate(json);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public void Validate_WithMissingProperty_ShouldReturnErrors()
    {
        // Arrange
        string json = """
                      {
                        "operator": "Contains",
                        "value": "test"
                      }
                      """;

        // Act
        IReadOnlyList<string> errors = QueryFilterSchemaValidator.Validate(json);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("property"));
    }

    [TestMethod]
    public void Validate_WithInvalidOperator_ShouldReturnErrors()
    {
        // Arrange
        string json = """
                      {
                        "operator": "InvalidOp",
                        "property": "name",
                        "value": "test"
                      }
                      """;

        // Act
        IReadOnlyList<string> errors = QueryFilterSchemaValidator.Validate(json);

        // Assert
        errors.Should().NotBeEmpty();
    }
}
