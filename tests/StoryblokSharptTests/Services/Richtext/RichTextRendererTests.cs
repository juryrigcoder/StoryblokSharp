using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;
using StoryblokSharp.Components;

namespace StoryblokSharp.Services.RichText;

public sealed class RichTextRenderer : IRichTextRenderer
{
    private readonly BlockNodeResolver _blockResolver;
    private readonly MarkNodeResolver _markResolver;
    private readonly TextNodeResolver _textResolver;
    private readonly ImageNodeResolver _imageResolver;
    private readonly IHtmlSanitizer _sanitizer;
    private readonly RichTextOptions _options;
    private readonly List<IComponentResolver> _componentResolvers;

    public RichTextRenderer(
        BlockNodeResolver blockResolver,
        MarkNodeResolver markResolver,
        TextNodeResolver textResolver,
        ImageNodeResolver imageResolver,
        IHtmlSanitizer sanitizer,
        IOptions<RichTextOptions> options)
    {
        _blockResolver = blockResolver ?? throw new ArgumentNullException(nameof(blockResolver));
        _markResolver = markResolver ?? throw new ArgumentNullException(nameof(markResolver));
        _textResolver = textResolver ?? throw new ArgumentNullException(nameof(textResolver));
        _imageResolver = imageResolver ?? throw new ArgumentNullException(nameof(imageResolver));
        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _componentResolvers = new List<IComponentResolver>();
    }

    public string Render(RichTextContent? content, RenderOptions? options = null)
    {
        try
        {
            if (content?.Content == null || !content.Content.Any())
            {
                return string.Empty;
            }

            var node = new RichTextNode
            {
                Type = "doc",
                Content = content.Content.Select(MapContent).Where(n => n != null).ToList()
            };

            var html = ResolveNode(node);
            return _sanitizer.Sanitize(html);
        }
        catch (Exception ex) when (!_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
        {
            return HandleInvalidContent();
        }
    }

    private RichTextNode? MapContent(RichTextContent content)
    {
        var nodeType = content.Type?.ToLowerInvariant() ?? "text";

        // Handle invalid node types
        if (!IsValidNodeType(nodeType))
        {
            if (_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Remove))
            {
                return null;
            }
            if (_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Replace))
            {
                return new RichTextNode { Type = "text", Text = string.Empty };
            }
        }

        return new RichTextNode
        {
            Type = nodeType,
            Text = content.Text,
            Attrs = content.Attrs,
            Content = content.Content?.Select(MapContent).Where(n => n != null).ToList(),
            Marks = content.Marks?.Select(mark => new MarkNode
            {
                Type = mark.Type?.ToLowerInvariant(),
                MarkType = Enum.Parse<MarkTypes>(mark.Type ?? "Text", ignoreCase: true),
                Attrs = mark.Attrs,
                Text = content.Text
            }).ToList()
        };
    }

    private string ResolveNode(RichTextNode node)
    {
        try
        {
            // Handle text nodes with marks first
            if (node.Type == "text")
            {
                var content = _textResolver.Resolve(node);
                if (node.Marks?.Any() == true)
                {
                    return node.Marks.Aggregate(content, (current, mark) =>
                        _markResolver.Resolve(mark with { Text = current }));
                }
                return content;
            }

            // Handle component nodes
            if (node.Type == "blok")
            {
                if (node.Attrs?.TryGetValue("component", out var componentType) == true && 
                    _options.ComponentResolver != null)
                {
                    var result = _options.ComponentResolver(componentType?.ToString() ?? "", node.Attrs);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
                return HandleInvalidContent();
            }

            // Handle unknown node types
            if (!IsValidNodeType(node.Type))
            {
                return HandleInvalidContent();
            }

            // Handle other node types
            return _blockResolver.Resolve(node);
        }
        catch (Exception ex) when (!_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
        {
            return HandleInvalidContent();
        }
    }

    private string HandleInvalidContent()
    {
        return _options.InvalidNodeHandling switch
        {
            StoryblokSharp.Services.RichText.InvalidNodeStrategy.Remove => string.Empty,
            StoryblokSharp.Services.RichText.InvalidNodeStrategy.Replace => "<!-- Invalid content -->",
            StoryblokSharp.Services.RichText.InvalidNodeStrategy.Keep => string.Empty,
            _ => string.Empty
        };
    }

    private bool IsValidNodeType(string? nodeType)
    {
        if (string.IsNullOrEmpty(nodeType)) return false;

        return nodeType.ToLowerInvariant() switch
        {
            "doc" or "text" or "paragraph" or "heading" or
            "blockquote" or "code_block" or "bullet_list" or
            "ordered_list" or "list_item" or "horizontal_rule" or
            "hard_break" or "image" or "emoji" or "blok" => true,
            _ => false
        };
    }

    /// <summary>
    /// Registers a custom node resolver
    /// </summary>
    public void AddNode(string key, Func<Node, object> schema)
    {
        // Custom node resolvers should be handled by BlockNodeResolver
        throw new NotImplementedException();
    }

    /// <summary>
    /// Registers a custom mark resolver
    /// </summary>
    public void AddMark(string key, Func<Node, object> schema)
    {
        // Custom mark resolvers should be handled by MarkNodeResolver
        throw new NotImplementedException();
    }

    /// <summary>
    /// Registers a custom component type with its corresponding class
    /// </summary>
    public void RegisterComponent(string componentType, Type componentClass)
    {
        // Component registration should be handled through options
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds a component resolver to handle component resolution
    /// </summary>
    public void AddComponentResolver(IComponentResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _componentResolvers.Add(resolver);
    }
}