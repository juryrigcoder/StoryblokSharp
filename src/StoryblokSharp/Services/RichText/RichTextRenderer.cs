using Microsoft.Extensions.Options;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;

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
                Content = content.Content.Select(MapContent).ToList()
            };

            var html = ResolveNode(node);
            return _sanitizer.Sanitize(html);
        }
        catch (Exception ex) when (!_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
        {
            return HandleInvalidNode(content?.Type ?? "unknown", content?.Text, ex);
        }
    }

    private RichTextNode MapContent(RichTextContent content)
    {
        var node = new RichTextNode
        {
            Type = content.Type?.ToLowerInvariant() ?? "text",
            Text = content.Text,
            Attrs = content.Attrs,
            Content = content.Content?.Select(MapContent).ToList(),
            Marks = content.Marks?.Select(mark => new MarkNode
            {
                Type = mark.Type?.ToLowerInvariant(),
                MarkType = Enum.Parse<MarkTypes>(mark.Type ?? "Text", ignoreCase: true),
                Attrs = mark.Attrs,
                Text = content.Text
            }).ToList()
        };

        // Handle invalid node types
        if (!IsValidNodeType(node.Type) && _options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Remove))
        {
            return new RichTextNode 
            { 
                Type = "text",
                Text = string.Empty 
            };
        }

        return node;
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
                var result = ResolveComponent(node);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // Handle unknown node types
            if (!IsValidNodeType(node.Type))
            {
                return HandleInvalidNode(node.Type, node.Text);
            }

            // Handle other node types
            return _blockResolver.Resolve(node);
        }
        catch (Exception ex) when (!_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
        {
            return HandleInvalidNode(node.Type, node.Text, ex);
        }
    }

    private string HandleInvalidNode(string nodeType, string? content = null, Exception? error = null)
    {
        return _options.InvalidNodeHandling switch
        {
            Services.RichText.InvalidNodeStrategy.Remove => string.Empty,
            Services.RichText.InvalidNodeStrategy.Replace => $"<!-- Invalid node type: {nodeType} -->",
            Services.RichText.InvalidNodeStrategy.Keep => content ?? string.Empty,
            Services.RichText.InvalidNodeStrategy.Throw => throw new InvalidOperationException(
                $"Invalid node type: {nodeType}", error),
            _ => string.Empty
        };
    }

    private string ResolveComponent(RichTextNode node)
    {
        if (node.Attrs == null || !node.Attrs.TryGetValue("component", out var componentType))
            return string.Empty;

        var type = componentType?.ToString();
        if (string.IsNullOrEmpty(type))
            return string.Empty;

        // Try each resolver in order
        foreach (var resolver in _componentResolvers)
        {
            if (resolver.SupportsComponent(type))
            {
                try
                {
                    return resolver.ResolveComponent(type, node.Attrs);
                }
                catch (Exception ex)
                {
                    if (_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
                        throw;

                    return HandleInvalidNode($"component:{type}", null, ex);
                }
            }
        }

        // Fall back to basic component resolver if configured
        if (_options.ComponentResolver != null)
        {
            try
            {
                return _options.ComponentResolver(type, node.Attrs);
            }
            catch (Exception ex)
            {
                if (_options.InvalidNodeHandling.Equals(Models.RichText.InvalidNodeStrategy.Throw))
                    throw;

                return HandleInvalidNode($"component:{type}", null, ex);
            }
        }

        return string.Empty;
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

    public void AddNode(string key, Func<Node, object> schema)
    {
        throw new NotImplementedException();
    }

    public void AddMark(string key, Func<Node, object> schema)
    {
        throw new NotImplementedException();
    }

    public void RegisterComponent(string componentType, Type componentClass)
    {
        throw new NotImplementedException();
    }

    public void AddComponentResolver(IComponentResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _componentResolvers.Add(resolver);
    }
}