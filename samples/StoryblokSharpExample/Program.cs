using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoryblokSharp.Client;
using StoryblokSharp.Configuration;
using StoryblokSharp.Models.Stories;
using StoryblokSharp.Models.RichText;
using StoryblokSharp.Services.RichText;
using StoryblokSharp.Services.RichText.NodeResolvers;
using StoryblokSharp.Utilities.RichText;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    static async Task<IStoryblokClient> InitializeStoryblokClientAsync(string accessToken)
    {
        // Set up dependency injection
        var services = new ServiceCollection();

        // Register utility services as singletons
        services.AddSingleton<StringBuilderCache>();
        services.AddSingleton<IHtmlUtilities, HtmlUtilities>();
        services.AddSingleton<IAttributeUtilities, AttributeUtilities>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
        services.AddSingleton<IRichTextSchema, DefaultRichTextSchema>();

        // Register node resolvers in correct order
        services.AddScoped<MarkNodeResolver>(sp =>
            new MarkNodeResolver(
                sp.GetRequiredService<IHtmlUtilities>(),
                sp.GetRequiredService<IAttributeUtilities>(),
                sp.GetRequiredService<IOptions<RichTextOptions>>(),
                sp.GetRequiredService<StringBuilderCache>()
            ));

        services.AddScoped<ImageNodeResolver>(sp =>
            new ImageNodeResolver(
                sp.GetRequiredService<IHtmlUtilities>(),
                sp.GetRequiredService<IAttributeUtilities>(),
                sp.GetRequiredService<StringBuilderCache>(),
                sp.GetRequiredService<IOptions<RichTextOptions>>()
            ));

        services.AddScoped<TextNodeResolver>(sp =>
            new TextNodeResolver(
                sp.GetRequiredService<IHtmlUtilities>(),
                sp.GetRequiredService<MarkNodeResolver>(),
                sp.GetRequiredService<IOptions<RichTextOptions>>()
            ));

        services.AddScoped<BlockNodeResolver>(sp =>
            new BlockNodeResolver(
                sp.GetRequiredService<IHtmlUtilities>(),
                sp.GetRequiredService<StringBuilderCache>(),
                sp.GetRequiredService<IAttributeUtilities>(),
                sp.GetRequiredService<IOptions<RichTextOptions>>(),
                sp.GetRequiredService<TextNodeResolver>(),
                sp.GetRequiredService<ImageNodeResolver>(),
                sp.GetRequiredService<MarkNodeResolver>()
            ));

        services.AddScoped<EmojiResolver>(sp =>
            new EmojiResolver(
                sp.GetRequiredService<IAttributeUtilities>()
            ));

        // Add rich text renderer with all its dependencies
        services.AddScoped<IRichTextRenderer, RichTextRenderer>(sp =>
            new RichTextRenderer(
                sp.GetRequiredService<BlockNodeResolver>(),
                sp.GetRequiredService<MarkNodeResolver>(),
                sp.GetRequiredService<TextNodeResolver>(),
                sp.GetRequiredService<ImageNodeResolver>(),
                sp.GetRequiredService<IHtmlSanitizer>(),
                sp.GetRequiredService<IOptions<RichTextOptions>>()
            ));

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
        });

        // Configure HTML sanitizer options
        services.Configure<HtmlSanitizerOptions>(options => { });

        // Configure Storyblok client
        await Task.Run(() => new StoryblokClientBuilder(services)
            .WithAccessToken(accessToken)
            .WithCache(options => options
                .WithType(StoryblokSharp.Models.Cache.CacheType.Memory)
                .WithDefaultExpiration(TimeSpan.FromMinutes(5)))
            .WithMaxRetries(3)
            .WithRateLimit(5)
            .WithHttps()
            .WithResponseInterceptor(async response =>
            {
                Console.WriteLine($"Response Status: {response.StatusCode}");
                await Task.CompletedTask;
                return response;
            })
            .Build());

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IStoryblokClient>();
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
                var richTextField = JsonSerializer.Deserialize<RichTextField>(wysiwygElement.GetRawText());
                if (richTextField != null)
                {
                    var _content = richTextField.ToRichTextContent();
                    OutputContentStructure(_content);
                    var renderedContent = RenderRichTextContent(richTextField);
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

    static string RenderRichTextContent(RichTextField richTextField)
    {
        var serviceCollection = new ServiceCollection();
        
        // Register utility services
        serviceCollection.AddSingleton<StringBuilderCache>();
        serviceCollection.AddSingleton<IHtmlUtilities, HtmlUtilities>();
        serviceCollection.AddSingleton<IAttributeUtilities, AttributeUtilities>();
        serviceCollection.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
        serviceCollection.AddSingleton<IRichTextSchema, DefaultRichTextSchema>();

        // Configure rich text options
        serviceCollection.Configure<RichTextOptions>(options =>
        {
            options.OptimizeImages = true;
            options.KeyedResolvers = true;
            options.InvalidNodeHandling = StoryblokSharp.Services.RichText.InvalidNodeStrategy.Remove;
        });

        // Configure HTML sanitizer options
        serviceCollection.Configure<HtmlSanitizerOptions>(options => { });

        // Register node resolvers
        serviceCollection.AddScoped<MarkNodeResolver>();
        serviceCollection.AddScoped<ImageNodeResolver>();
        serviceCollection.AddScoped<TextNodeResolver>();
        serviceCollection.AddScoped<BlockNodeResolver>();
        serviceCollection.AddScoped<EmojiResolver>();
        serviceCollection.AddScoped<IRichTextRenderer, RichTextRenderer>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<IRichTextRenderer>();

        try
        {
            var content = richTextField.ToRichTextContent();
            if (content == null)
            {
                Console.WriteLine("Warning: Unable to convert rich text field to content");
                return string.Empty;
            }

            var renderedContent = renderer.Render(content);
            return renderedContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering rich text: {ex.Message}");
            return string.Empty;
        }
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