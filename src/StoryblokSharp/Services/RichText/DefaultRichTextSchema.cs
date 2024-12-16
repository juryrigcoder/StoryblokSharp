using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Services.RichText;

/// <summary>
/// Default rich text schema implementation
/// </summary>
public class DefaultRichTextSchema : RichTextSchema
{
    /// <summary>
    /// Initializes a new instance of DefaultRichTextSchema
    /// </summary>
    public DefaultRichTextSchema()
    {
        InitializeDefaultNodes();
        InitializeDefaultMarks();
    }

    private void InitializeDefaultNodes()
    {
        Nodes["horizontal_rule"] = _ => new SchemaResult { SingleTag = "hr" };
        
        Nodes["blockquote"] = _ => new SchemaResult { Tag = ["blockquote"] };
        
        Nodes["bullet_list"] = _ => new SchemaResult { Tag = ["ul"] };
        
        Nodes["code_block"] = node => new SchemaResult 
        { 
            Tag = ["pre", "code"],
            Attrs = node.Attrs
        };
        
        Nodes["hard_break"] = _ => new SchemaResult { SingleTag = "br" };
        
        Nodes["heading"] = node => new SchemaResult 
        { 
            Tag = [$"h{node.Attrs?.GetValueOrDefault("level", "1") ?? "1"}"] 
        };
        
        Nodes["list_item"] = _ => new SchemaResult { Tag = ["li"] };
        
        Nodes["ordered_list"] = _ => new SchemaResult { Tag = ["ol"] };
        
        Nodes["paragraph"] = _ => new SchemaResult { Tag = ["p"] };
    }

    private void InitializeDefaultMarks()
    {
        Marks["bold"] = _ => new SchemaResult { Tag = ["b"] };
        
        Marks["strike"] = _ => new SchemaResult { Tag = ["s"] };
        
        Marks["underline"] = _ => new SchemaResult { Tag = ["u"] };
        
        Marks["strong"] = _ => new SchemaResult { Tag = ["strong"] };
        
        Marks["code"] = _ => new SchemaResult { Tag = ["code"] };
        
        Marks["italic"] = _ => new SchemaResult { Tag = ["i"] };
        
        Marks["link"] = node => new SchemaResult 
        { 
            Tag = ["a"],
            Attrs = GetLinkAttributes(node).ToDictionary(x => x.Key, x => (object)x.Value)
        };
    }

    private static Dictionary<string, string> GetLinkAttributes(Node node)
    {
        var attrs = new Dictionary<string, string>();
        
        if (node.Attrs.TryGetValue("href", out var href))
            attrs["href"] = href.ToString()!;

        var linkType = node.Attrs.GetValueOrDefault("linktype", "url").ToString();
        
        if (linkType == "email" && attrs.TryGetValue("href", out var emailHref))
            attrs["href"] = $"mailto:{emailHref}";

        if (node.Attrs.TryGetValue("anchor", out var anchor))
        {
            attrs["href"] = $"{attrs.GetValueOrDefault("href", "")}#{anchor}";
        }

        if (node.Attrs.TryGetValue("target", out var target))
            attrs["target"] = target.ToString()!;

        return attrs;
    }
}