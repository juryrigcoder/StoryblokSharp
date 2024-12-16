namespace StoryblokSharp.Models.RichText;

/// <summary>
/// Represents a Storyblok component block
/// </summary>
public class ComponentBlok
{
    /// <summary>
    /// The component type
    /// </summary>
    public required string Component { get; init; }

    /// <summary>
    /// Additional component data
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}