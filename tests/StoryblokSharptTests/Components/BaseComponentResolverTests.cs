using System.Text.Json;
using Xunit;
using StoryblokSharp.Components;

namespace StoryblokSharp.Tests.Components;

// Test component classes
public class TestComponent
{
    [RequiredProp]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Count { get; set; }
    public double? Price { get; set; }
    public string[]? Tags { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}

public class SimpleComponent
{
    public string Content { get; set; } = string.Empty;
}

// Test component with complex properties
public class ComplexComponent
{
    [RequiredProp]
    public TestComponent NestedComponent { get; set; } = new();
    public IList<string>? Items { get; set; }
}

// Concrete implementation for testing
public class TestComponentResolver : BaseComponentResolver
{
    private readonly string _renderPrefix;

    public TestComponentResolver(string renderPrefix = "Rendered: ")
    {
        _renderPrefix = renderPrefix;
    }

    protected override string RenderComponent(string componentType, IDictionary<string, object> props)
    {
        var json = JsonSerializer.Serialize(props);
        return $"{_renderPrefix}{componentType} - {json}";
    }
}

public class BaseComponentResolverTests
{
    private readonly TestComponentResolver _resolver;

    public BaseComponentResolverTests()
    {
        _resolver = new TestComponentResolver();
        _resolver.RegisterComponent("test", typeof(TestComponent));
        _resolver.RegisterComponent("simple", typeof(SimpleComponent));
        _resolver.RegisterComponent("complex", typeof(ComplexComponent));
    }

    [Fact]
    public void RegisterComponent_ValidInput_RegistersSuccessfully()
    {
        // Arrange
        var resolver = new TestComponentResolver();

        // Act
        resolver.RegisterComponent("myComponent", typeof(TestComponent));

        // Assert
        Assert.True(resolver.SupportsComponent("myComponent"));
        Assert.Equal(typeof(TestComponent), resolver.GetComponentType("myComponent"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RegisterComponent_NullOrEmptyComponentType_ThrowsArgumentNullException(string? componentType)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _resolver.RegisterComponent(componentType!, typeof(TestComponent)));
    }

    [Fact]
    public void RegisterComponent_NullComponentClass_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _resolver.RegisterComponent("test", null!));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("TEST")]
    [InlineData("Test")]
    public void SupportsComponent_RegisteredComponent_ReturnsTrue(string componentType)
    {
        // Act & Assert
        Assert.True(_resolver.SupportsComponent(componentType));
    }

    [Fact]
    public void SupportsComponent_UnregisteredComponent_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_resolver.SupportsComponent("unknown"));
    }

    [Fact]
    public void ResolveComponent_UnknownComponent_ReturnsEmptyString()
    {
        // Arrange
        var props = new Dictionary<string, object> { { "content", "test" } };

        // Act
        var result = _resolver.ResolveComponent("unknown", props);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveComponent_ValidComponent_RendersSuccessfully()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test Title" },
            { "Description", "Test Description" }
        };

        // Act
        var result = _resolver.ResolveComponent("test", props);

        // Assert
        Assert.Contains("Rendered: test", result);
        Assert.Contains("\"Title\":\"Test Title\"", result);
        Assert.Contains("\"Description\":\"Test Description\"", result);
    }

    [Fact]
    public void ValidateProps_MissingRequiredProp_ThrowsArgumentException()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Description", "Test" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _resolver.ValidateProps("test", props));
        Assert.Contains("Required prop 'Title' missing", exception.Message);
    }

    [Theory]
    [InlineData("Count", "not-a-number")]
    [InlineData("Price", "invalid-price")]
    public void ValidateProps_InvalidPropType_ThrowsArgumentException(string propName, object invalidValue)
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test" },
            { propName, invalidValue }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _resolver.ValidateProps("test", props));
        Assert.Contains($"Invalid type for prop '{propName}'", exception.Message);
    }

    [Fact]
    public void ValidateProps_ValidNumericConversions_Succeeds()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test" },
            { "Count", 42.0 },  // Double should be convertible to int
            { "Price", 19.99f }  // Float should be convertible to double
        };

        // Act & Assert
        var exception = Record.Exception(() => _resolver.ValidateProps("test", props));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateProps_ValidArrayAndDictionaryTypes_Succeeds()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test" },
            { "Tags", new[] { "tag1", "tag2" } },
            { "Metadata", new Dictionary<string, string> { { "key", "value" } } }
        };

        // Act & Assert
        var exception = Record.Exception(() => _resolver.ValidateProps("test", props));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateProps_ComplexComponent_ValidatesNestedProperties()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "NestedComponent", new TestComponent 
                {
                    Title = "Nested Title",
                    Description = "Nested Description"
                }
            },
            { "Items", new[] { "item1", "item2" } }
        };

        // Act & Assert
        var exception = Record.Exception(() => _resolver.ValidateProps("complex", props));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateProps_NullableProperties_Succeeds()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test" },
            { "Description", null! },
            { "Price", null! }
        };

        // Act & Assert
        var exception = Record.Exception(() => _resolver.ValidateProps("test", props));
        Assert.Null(exception);
    }

    [Fact]
    public void GetComponentType_RegisteredComponent_ReturnsType()
    {
        // Act
        var type = _resolver.GetComponentType("test");

        // Assert
        Assert.NotNull(type);
        Assert.Equal(typeof(TestComponent), type);
    }

    [Fact]
    public void GetComponentType_UnregisteredComponent_ReturnsNull()
    {
        // Act
        var type = _resolver.GetComponentType("unknown");

        // Assert
        Assert.Null(type);
    }

    [Fact]
    public void ValidateProps_UnknownComponent_DoesNotValidate()
    {
        // Arrange
        var props = new Dictionary<string, object>();

        // Act
        var exception = Record.Exception(() => _resolver.ValidateProps("unknown", props));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateProps_UnknownProps_IgnoresUnknownProps()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Title", "Test" },  // Known prop
            { "UnknownProp", "value" }  // Unknown prop
        };

        // Act
        var exception = Record.Exception(() => _resolver.ValidateProps("test", props));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateProps_MultipleValidationErrors_ReportsFirstError()
    {
        // Arrange
        var props = new Dictionary<string, object>
        {
            { "Count", "invalid" },
            { "Price", "also-invalid" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _resolver.ValidateProps("test", props));
        Assert.Contains("Required prop 'Title' missing", exception.Message);
    }
}