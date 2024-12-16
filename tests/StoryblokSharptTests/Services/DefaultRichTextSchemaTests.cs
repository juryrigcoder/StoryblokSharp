using Xunit;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Tests.Services.RichText;

public class DefaultRichTextSchemaTests
{
    private readonly DefaultRichTextSchema _schema;

    public DefaultRichTextSchemaTests()
    {
        _schema = new DefaultRichTextSchema();
    }

    [Fact]
    public void Nodes_ContainsDefaultImplementations()
    {
        // Assert
        Assert.NotNull(_schema.Nodes);
        Assert.Contains("horizontal_rule", _schema.Nodes.Keys);
        Assert.Contains("blockquote", _schema.Nodes.Keys);
        Assert.Contains("bullet_list", _schema.Nodes.Keys);
        Assert.Contains("code_block", _schema.Nodes.Keys);
        Assert.Contains("hard_break", _schema.Nodes.Keys);
        Assert.Contains("heading", _schema.Nodes.Keys);
        Assert.Contains("list_item", _schema.Nodes.Keys);
        Assert.Contains("ordered_list", _schema.Nodes.Keys);
        Assert.Contains("paragraph", _schema.Nodes.Keys);
    }

    [Fact]
    public void Marks_ContainsDefaultImplementations()
    {
        // Assert
        Assert.NotNull(_schema.Marks);
        Assert.Contains("bold", _schema.Marks.Keys);
        Assert.Contains("strike", _schema.Marks.Keys);
        Assert.Contains("underline", _schema.Marks.Keys);
        Assert.Contains("strong", _schema.Marks.Keys);
        Assert.Contains("code", _schema.Marks.Keys);
        Assert.Contains("italic", _schema.Marks.Keys);
        Assert.Contains("link", _schema.Marks.Keys);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Heading_ReturnsCorrectLevel(int level)
    {
        // Arrange
        var node = new Node
        {
            Type = "heading",
            Attrs = new Dictionary<string, object>
            {
                ["level"] = level
            }
        };

        // Act
        var result = _schema.Nodes["heading"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tag);
        Assert.Single(result.Tag);
        Assert.Equal($"h{level}", result.Tag[0]);
    }

    [Fact]
    public void Link_WithHref_GeneratesCorrectAttributes()
    {
        // Arrange
        var node = new Node
        {
            Type = "link",
            Attrs = new Dictionary<string, object>
            {
                ["href"] = "https://example.com",
                ["target"] = "_blank"
            }
        };

        // Act
        var result = _schema.Marks["link"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Attrs);
        Assert.Equal("https://example.com", result.Attrs["href"]);
        Assert.Equal("_blank", result.Attrs["target"]);
    }

    [Fact]
    public void Link_WithEmailType_PrependsMailto()
    {
        // Arrange
        var node = new Node
        {
            Type = "link",
            Attrs = new Dictionary<string, object>
            {
                ["href"] = "test@example.com",
                ["linktype"] = "email"
            }
        };

        // Act
        var result = _schema.Marks["link"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Attrs);
        Assert.Equal("mailto:test@example.com", result.Attrs["href"]);
    }

    [Fact]
    public void Link_WithAnchor_AppendsToHref()
    {
        // Arrange
        var node = new Node
        {
            Type = "link",
            Attrs = new Dictionary<string, object>
            {
                ["href"] = "https://example.com",
                ["anchor"] = "section1"
            }
        };

        // Act
        var result = _schema.Marks["link"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Attrs);
        Assert.Equal("https://example.com#section1", result.Attrs["href"]);
    }

    [Fact]
    public void CodeBlock_PreservesAttributes()
    {
        // Arrange
        var node = new Node
        {
            Type = "code_block",
            Attrs = new Dictionary<string, object>
            {
                ["language"] = "csharp",
                ["highlightLines"] = "1,3-5"
            }
        };

        // Act
        var result = _schema.Nodes["code_block"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tag);
        Assert.Equal(2, result.Tag.Length);
        Assert.Equal("pre", result.Tag[0]);
        Assert.Equal("code", result.Tag[1]);
        Assert.NotNull(result.Attrs);
        Assert.Equal("csharp", result.Attrs["language"]);
        Assert.Equal("1,3-5", result.Attrs["highlightLines"]);
    }

    [Fact]
    public void HorizontalRule_ReturnsSingleTag()
    {
        // Arrange
        var node = new Node
        {
            Type = "horizontal_rule",
            Attrs = new Dictionary<string, object>()
        };

        // Act
        var result = _schema.Nodes["horizontal_rule"](node);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("hr", result.SingleTag);
    }

    [Fact]
    public void BulletList_ReturnsUnorderedListTag()
    {
        // Arrange
        var node = new Node
        {
            Type = "bullet_list",
            Attrs = new Dictionary<string, object>()
        };

        // Act
        var result = _schema.Nodes["bullet_list"](node);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tag);
        Assert.Single(result.Tag);
        Assert.Equal("ul", result.Tag[0]);
    }
}