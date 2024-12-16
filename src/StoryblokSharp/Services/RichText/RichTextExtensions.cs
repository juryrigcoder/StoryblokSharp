using StoryblokSharp.Models.Stories;
using StoryblokSharp.Models.RichText;

namespace StoryblokSharp.Services.RichText;

/// <summary>
/// Extension methods for converting between rich text models
/// </summary>
public static class RichTextExtensions
{
    /// <summary>
    /// Converts a Storyblok RichTextField to RichTextContent
    /// </summary>
    public static RichTextContent ToRichTextContent(this RichTextField field)
    {
        return new RichTextContent
        {
            Type = field.Type,
            Content = field.Content?.Select(c => c.ToRichTextContent()).ToArray(),
            Text = field.Text,
            Marks = field.Marks?.Select(m => new Mark
            {
                Type = m.Type,
                Attrs = m.Attrs
            }).ToArray(),
            Attrs = field.Attrs
        };
    }
}