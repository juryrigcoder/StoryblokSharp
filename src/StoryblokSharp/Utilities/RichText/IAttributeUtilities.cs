namespace StoryblokSharp.Utilities.RichText;

/// <summary>
/// Interface for HTML attribute utilities
/// </summary>
public interface IAttributeUtilities
{
    string FormatAttributes(IDictionary<string, string>? attrs);
    string BuildStyleString(IDictionary<string, object>? attrs);
    Dictionary<string, string> MergeAttributes(params IDictionary<string, string>?[] attrSets);
    Dictionary<string, string> MergeAttributes(IDictionary<string, string>? htmlAttrs, IDictionary<string, object>? objAttrs);
    Dictionary<string, string> ParseStyleString(string? style);
}
