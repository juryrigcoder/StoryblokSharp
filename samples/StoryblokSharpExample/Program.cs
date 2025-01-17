using Microsoft.Extensions.DependencyInjection;
using StoryblokSharp.Client;
using StoryblokSharp.Configuration;
using StoryblokSharp.Models.Stories;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;
using StoryblokSharp.Utilities.RichText;
using StoryblokSharp.Components;
using System.Text.Json;
using System.Text.Json.Serialization;
using StoryblokSharp.Models.Cache;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        {
            Console.WriteLine("Please provide your Storyblok API token as a command line argument.");
            return;
        }

        var accessToken = args[0];
        var client = await InitializeStoryblokClientAsync(accessToken);

        await DemonstrateFeaturesAsync(client);
    }
    private static IServiceProvider? _serviceProvider;
    static async Task<IStoryblokClient> InitializeStoryblokClientAsync(string accessToken)
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        
        // Configure all services
        ConfigureServices(services);

        // Configure Storyblok client
        var clientBuilder = new StoryblokClientBuilder(services)
            .WithAccessToken(accessToken)
            .WithCache(options => options
                .WithType(CacheType.Memory)
                .WithDefaultExpiration(TimeSpan.FromMinutes(5)))
            .WithMaxRetries(3)
            .WithRateLimit(5)
            .WithHttps()
            .WithResponseInterceptor(async response =>
            {
                Console.WriteLine($"Response Status: {response.StatusCode}");
                await Task.CompletedTask;
                return response;
            });

        // Build the client first to register its services
        await Task.Run(() => clientBuilder.Build());

        // Now build the service provider after all services are registered
        _serviceProvider = services.BuildServiceProvider();

        // Get the client from the service provider
        return _serviceProvider.GetRequiredService<IStoryblokClient>();
    }
    static async Task DemonstrateFeaturesAsync(IStoryblokClient client)
    {
        try
        {
            var parameters = new StoryQueryParameters
            {
                Version = "published"
            };

            var story = await client.GetStoryAsync<StoryContent>("home", parameters);

            if (story?.Story?.Content?.Body != null)
            {
                Console.WriteLine("\nProcessing rich text content from all components:");
                foreach (var component in story.Story.Content.Body)
                {
                    ProcessComponent(component);
                }
            }
            else
            {
                Console.WriteLine("No content found in the story.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
        }
    }
    static void ProcessComponent(JsonElement component)
    {
        try
        {
            var componentType = component.GetProperty("component").GetString();
            Console.WriteLine($"\nProcessing component: {componentType}");

            if (component.TryGetProperty("wysiwyg", out var wysiwygElement))
            {
                Console.WriteLine($"Found rich text content in {componentType}:");

                var rawJson = wysiwygElement.GetRawText();
                Console.WriteLine("Raw JSON structure:");
                Console.WriteLine(rawJson);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var richTextContent = JsonSerializer.Deserialize<RichTextContent>(rawJson, options);
                if (richTextContent != null)
                {
                    OutputContentStructure(richTextContent);
                    var renderedContent = RenderRichTextContent(richTextContent);
                    Console.WriteLine("\nRendered content:");
                    Console.WriteLine(renderedContent);
                    Console.WriteLine("-------------------");
                }
            }

            // Process nested components
            if (component.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    ProcessComponent(item);
                }
            }

            // Process content arrays
            if (component.TryGetProperty("content", out var content))
            {
                foreach (var contentItem in content.EnumerateArray())
                {
                    ProcessComponent(contentItem);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing component: {ex.Message}");
        }
    }
    static string RenderRichTextContent(RichTextContent content)
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider has not been initialized.");
        }

        var renderer = _serviceProvider.GetRequiredService<IRichTextRenderer>();

        try
        {
            if (content == null)
            {
                Console.WriteLine("Warning: Rich text content is null");
                return string.Empty;
            }

            var rendered = renderer.Render(content);
            return rendered;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering rich text: {ex.Message}");
            return string.Empty;
        }
    }
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register utility services as singletons
        services.AddSingleton<StringBuilderCache>();
        services.AddSingleton<IHtmlUtilities, HtmlUtilities>();
        services.AddSingleton<IAttributeUtilities, AttributeUtilities>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
        services.AddSingleton<IRichTextSchema, DefaultRichTextSchema>();

        // Register node resolvers in correct order
        services.AddScoped<MarkNodeResolver>();
        services.AddScoped<ImageNodeResolver>();
        services.AddScoped<TextNodeResolver>();
        services.AddScoped<BlockNodeResolver>();
        services.AddScoped<EmojiResolver>();

        // Add rich text renderer
        services.AddScoped<IRichTextRenderer, RichTextRenderer>();

        // Configure rich text options
        services.Configure<RichTextOptions>(options =>
        {
            options.OptimizeImages = true;
            options.KeyedResolvers = true;
            options.InvalidNodeHandling = StoryblokSharp.Services.RichText.InvalidNodeStrategy.Remove;
            options.ImageOptions = new ImageOptimizationOptions
            {
                Loading = "lazy",
                Width = 800,
                Height = 600
            };

            // Setup custom resolver for links
            options.CustomResolvers = new Dictionary<string, Func<IRichTextNode, string>>
            {
                ["link"] = node =>
                {
                    if (node.Attrs?.TryGetValue("href", out var href) == true)
                    {
                        var target = node.Attrs.TryGetValue("target", out var t) ? t?.ToString() : "_self";
                        return $"<a href=\"{href}\" target=\"{target}\">{node.Text}</a>";
                    }
                    return node.Text ?? string.Empty;
                }
            };
        });

        // Configure HTML sanitizer options
        services.Configure<HtmlSanitizerOptions>(options =>
        {
            if (options.AllowedTags == null)
                options.AllowedTags = new HashSet<string>();

            options.AllowedTags.Add("a");

            if (options.AllowedAttributes == null)
                options.AllowedAttributes = new Dictionary<string, HashSet<string>>();

            options.AllowedAttributes["a"] = new HashSet<string> { "href", "target" };
        });
    }
    static void OutputContentStructure(RichTextContent content, int level = 0)
    {
        var indent = new string(' ', level * 2);
        if (content == null)
        {
            Console.WriteLine($"{indent}Content is null");
            return;
        }

        Console.WriteLine($"{indent}Type: {content.Type}");

        if (content.Content != null)
        {
            Console.WriteLine($"{indent}Children: {content.Content.Count()}");
            foreach (var child in content.Content)
            {
                OutputContentStructure(child, level + 1);
            }
        }

        if (!string.IsNullOrEmpty(content.Text))
        {
            Console.WriteLine($"{indent}Text: {content.Text}");
        }

        if (content.Marks != null)
        {
            Console.WriteLine($"{indent}Marks: {string.Join(", ", content.Marks.Select(m => m.Type))}");
        }

        if (content.Attrs != null)
        {
            Console.WriteLine($"{indent}Attributes: {string.Join(", ", content.Attrs.Select(a => $"{a.Key}={a.Value}"))}");
        }
    }
}

public class StoryContent
{
    [JsonPropertyName("_uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("body")]
    public JsonElement[]? Body { get; set; }

    [JsonPropertyName("component")]
    public string? Component { get; set; }
}