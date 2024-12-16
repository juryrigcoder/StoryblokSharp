using Microsoft.Extensions.Options;
using Moq;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;
using StoryblokSharp.Utilities.RichText;
using Xunit;

namespace StoryblokSharp.Tests.Services.RichText.NodeResolvers;

public abstract class NodeResolverTestBase
{
    protected readonly Mock<IHtmlUtilities> HtmlUtils;
    protected readonly Mock<IAttributeUtilities> AttrUtils;
    protected readonly StringBuilderCache BuilderCache;
    protected readonly RichTextOptions Options;
    protected readonly IOptions<RichTextOptions> OptionsWrapper;

    protected NodeResolverTestBase()
    {
        HtmlUtils = new Mock<IHtmlUtilities>();
        AttrUtils = new Mock<IAttributeUtilities>();
        BuilderCache = new StringBuilderCache();
        Options = new RichTextOptions();
        OptionsWrapper = Microsoft.Extensions.Options.Options.Create(Options);

        // Common setup
        HtmlUtils.Setup(x => x.Encode(It.IsAny<string>()))
            .Returns<string>(s => s);
        HtmlUtils.Setup(x => x.EncodeAttribute(It.IsAny<string>()))
            .Returns<string>(s => s);

        // Setup default behavior for IAttributeUtilities methods
        AttrUtils.Setup(x => x.FormatAttributes(It.IsAny<IDictionary<string, string>>()))
            .Returns<IDictionary<string, string>>(attrs => 
                string.Join(" ", attrs?.Select(a => $"{a.Key}=\"{a.Value}\"") ?? Array.Empty<string>()));

        AttrUtils.Setup(x => x.BuildStyleString(It.IsAny<IDictionary<string, object>>()))
            .Returns<IDictionary<string, object>>(attrs => 
                string.Join("; ", attrs?.Select(a => $"{a.Key}: {a.Value}") ?? Array.Empty<string>()));

        AttrUtils.Setup(x => x.MergeAttributes(It.IsAny<IDictionary<string, string>[]>()))
            .Returns<IDictionary<string, string>[]>(attrSets =>
                {
                    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var attrs in attrSets.Where(a => a != null))
                    {
                        foreach (var attr in attrs)
                        {
                            if (!string.IsNullOrWhiteSpace(attr.Key))
                                result[attr.Key] = attr.Value;
                        }
                    }
                    return result;
                });

        AttrUtils.Setup(x => x.MergeAttributes(
                It.IsAny<IDictionary<string, string>>(), 
                It.IsAny<IDictionary<string, object>>()))
            .Returns<IDictionary<string, string>, IDictionary<string, object>>((htmlAttrs, objAttrs) =>
                {
                    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (htmlAttrs != null)
                    {
                        foreach (var attr in htmlAttrs)
                            if (!string.IsNullOrWhiteSpace(attr.Key))
                                result[attr.Key] = attr.Value;
                    }
                    if (objAttrs != null)
                    {
                        foreach (var attr in objAttrs)
                            if (!string.IsNullOrWhiteSpace(attr.Key))
                                result[attr.Key] = attr.Value?.ToString() ?? string.Empty;
                    }
                    return result;
                });

        AttrUtils.Setup(x => x.ParseStyleString(It.IsAny<string>()))
            .Returns<string>(style =>
                {
                    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (string.IsNullOrWhiteSpace(style)) return result;

                    var declarations = style.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var declaration in declarations)
                    {
                        var parts = declaration.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            var name = parts[0].Trim();
                            var value = parts[1].Trim();
                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                                result[name] = value;
                        }
                    }
                    return result;
                });
    }
}
public class TextNodeResolverTests : NodeResolverTestBase
{
    private readonly TextNodeResolver _resolver;
    private readonly MarkNodeResolver _markResolver;

    public TextNodeResolverTests()
    {
        _markResolver = new MarkNodeResolver(HtmlUtils.Object, AttrUtils.Object, OptionsWrapper, BuilderCache);
        _resolver = new TextNodeResolver(HtmlUtils.Object, _markResolver, OptionsWrapper);
    }

    [Fact]
    public void Resolve_PlainText_ReturnsEncodedText()
    {
        // Arrange
        var node = new TextNode 
        { 
            Type = TextTypes.Text.ToString().ToLower(),
            Text = "Hello World",
            TextContent = "Hello World"
        };
        HtmlUtils.Setup(x => x.Encode("Hello World")).Returns("Hello&nbsp;World");

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("Hello&nbsp;World", result);
        HtmlUtils.Verify(x => x.Encode("Hello World"), Times.Once);
    }

    [Fact]
    public void Resolve_TextWithBoldMark_ReturnsStrongElement()
    {
        // Arrange
        var node = new TextNode
        {
            Type = TextTypes.Text.ToString().ToLower(),
            Text = "Bold Text",
            TextContent = "Bold Text",
            Marks = new[]
            {
                new MarkNode
                {
                    Type = "bold",
                    MarkType = MarkTypes.Bold
                }
            }
        };

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<strong>Bold Text</strong>", result);
    }
}

public class BlockNodeResolverTests : NodeResolverTestBase
{
    private readonly BlockNodeResolver _resolver;

    public BlockNodeResolverTests()
    {
        var markResolver = new MarkNodeResolver(HtmlUtils.Object, AttrUtils.Object, OptionsWrapper, BuilderCache);
        var textResolver = new TextNodeResolver(HtmlUtils.Object, markResolver, OptionsWrapper);
        var imageResolver = new ImageNodeResolver(HtmlUtils.Object, AttrUtils.Object, BuilderCache, OptionsWrapper);
        
        _resolver = new BlockNodeResolver(
            HtmlUtils.Object,
            BuilderCache,
            AttrUtils.Object,
            OptionsWrapper,
            textResolver,
            imageResolver,
            markResolver);
    }

    [Fact]
    public void Resolve_Paragraph_RendersParagraphElement()
    {
        // Arrange
        var node = new RichTextNode
        {
            Type = "paragraph",
            Content = new[]
            {
                new RichTextNode
                {
                    Type = "text",
                    Text = "Test paragraph"
                }
            }
        };

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<p>Test paragraph</p>", result);
    }

    [Fact]
    public void Resolve_HeadingWithLevel_RendersHeadingElement()
    {
        // Arrange
        var node = new RichTextNode
        {
            Type = "heading",
            Attrs = new Dictionary<string, object> { { "level", 2 } },
            Content = new[]
            {
                new RichTextNode
                {
                    Type = "text",
                    Text = "Test heading"
                }
            }
        };

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<h2>Test heading</h2>", result);
    }
}

public class MarkNodeResolverTests : NodeResolverTestBase
{
    private readonly MarkNodeResolver _resolver;

    public MarkNodeResolverTests()
    {
        _resolver = new MarkNodeResolver(HtmlUtils.Object, AttrUtils.Object, OptionsWrapper, BuilderCache);
    }

    [Fact]
    public void Resolve_BoldMark_ReturnsStrongElement()
    {
        // Arrange
        var node = new MarkNode
        {
            Type = MarkTypes.Bold.ToString().ToLower(),
            MarkType = MarkTypes.Bold,
            Text = "Bold text"
        };

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<strong>Bold text</strong>", result);
    }

    [Fact]
    public void Resolve_Link_ReturnsAnchorElement()
    {
        // Arrange
        var node = new MarkNode
        {
            Type = MarkTypes.Link.ToString().ToLower(),
            MarkType = MarkTypes.Link,
            Text = "Link text",
            Attrs = new Dictionary<string, object>
            {
                { "href", "https://example.com" },
                { "target", "_blank" }
            }
        };

        AttrUtils.Setup(x => x.FormatAttributes(It.IsAny<Dictionary<string, string>>()))
            .Returns("href=\"https://example.com\" target=\"_blank\"");

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<a href=\"https://example.com\" target=\"_blank\">Link text</a>", result);
    }
}

public class ImageNodeResolverTests : NodeResolverTestBase
{
    private readonly ImageNodeResolver _resolver;

    public ImageNodeResolverTests()
    {
        _resolver = new ImageNodeResolver(HtmlUtils.Object, AttrUtils.Object, BuilderCache, OptionsWrapper);
    }

    [Fact]
    public void Resolve_BasicImage_ReturnsImgElement()
    {
        // Arrange
        var node = new RichTextNode
        {
            Type = "image",
            Attrs = new Dictionary<string, object>
            {
                { "src", "https://example.com/image.jpg" },
                { "alt", "Test image" }
            }
        };

        AttrUtils.Setup(x => x.FormatAttributes(It.IsAny<Dictionary<string, string>>()))
            .Returns("src=\"https://example.com/image.jpg\" alt=\"Test image\"");

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal("<img src=\"https://example.com/image.jpg\" alt=\"Test image\">", result);
    }

    [Fact]
    public void Resolve_OptimizedImage_ReturnsOptimizedImgElement()
    {
        // Arrange
        Options.OptimizeImages = true;
        Options.ImageOptions = new ImageOptimizationOptions
        {
            Width = 800,
            Height = 600,
            Loading = "lazy"
        };

        var node = new RichTextNode
        {
            Type = "image",
            Attrs = new Dictionary<string, object>
            {
                { "src", "https://a.storyblok.com/f/12345/image.jpg" },
                { "alt", "Test image" }
            }
        };

        AttrUtils.Setup(x => x.FormatAttributes(It.IsAny<Dictionary<string, string>>()))
            .Returns("src=\"https://a.storyblok.com/f/12345/image.jpg/m/800x600/\" alt=\"Test image\" width=\"800\" height=\"600\" loading=\"lazy\"");

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Contains("/m/800x600/", result);
        Assert.Contains("loading=\"lazy\"", result);
    }
}

public class EmojiResolverTests : NodeResolverTestBase
{
    private readonly EmojiResolver _resolver;

    public EmojiResolverTests()
    {
        _resolver = new EmojiResolver(AttrUtils.Object);
    }

    [Fact]
    public void Resolve_ValidEmoji_ReturnsFormattedElement()
    {
        // Arrange
        var node = new RichTextNode
        {
            Type = "emoji",
            Attrs = new Dictionary<string, object>
            {
                { "emoji", "👋" },
                { "name", "wave" },
                { "fallbackImage", "wave.png" }
            }
        };

        AttrUtils.Setup(x => x.FormatAttributes(It.Is<Dictionary<string, string>>(d => d.ContainsKey("data-emoji"))))
            .Returns("data-type=\"emoji\" data-name=\"wave\" data-emoji=\"👋\"");
        
        AttrUtils.Setup(x => x.FormatAttributes(It.Is<Dictionary<string, string>>(d => d.ContainsKey("src"))))
            .Returns("src=\"wave.png\" alt=\"wave\" style=\"width: 1.25em; height: 1.25em; vertical-align: text-top\" draggable=\"false\" loading=\"lazy\"");

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Contains("<span data-type=\"emoji\" data-name=\"wave\" data-emoji=\"👋\">", result);
        Assert.Contains("<img src=\"wave.png\"", result);
    }

    [Fact]
    public void Resolve_InvalidEmoji_ReturnsEmpty()
    {
        // Arrange
        var node = new RichTextNode
        {
            Type = "emoji",
            Attrs = new Dictionary<string, object>()
        };

        // Act
        var result = _resolver.Resolve(node);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}