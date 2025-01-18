# StoryblokSharp

A modern .NET client for the Storyblok Headless CMS API. This project is a C# port of the official [storyblok-js-client](https://github.com/storyblok/storyblok-js-client), maintaining feature parity while providing a strongly-typed interface for .NET applications. It supports both Storyblok's Content Delivery API (v2) and Management API (v1).

## Rationale

As a personal project be kind! This is shared as is and is not for production.

## Acknowledgements

This project is a port of the [storyblok-js-client](https://github.com/storyblok/storyblok-js-client) JavaScript library, created and maintained by the Storyblok team ([@storyblok](https://github.com/storyblok)). We are grateful for their excellent work which serves as the foundation for this .NET implementation.

Additionally, we've included the richtext implementation of [richtext](https://github.com/storyblok/richtext) Javascript library, used to interact with the richtext components.

License: This project, like the original storyblok-js-client, is licensed under MIT.

Original Client Repository: https://github.com/storyblok/storyblok-js-client
Original Richtext Repository: https://github.com/storyblok/richtext

## Heritage

This library is based on the official [storyblok-js-client](https://github.com/storyblok/storyblok-js-client) JavaScript library and follows the same core principles and features. We've tried to carefully translated the JavaScript implementation to idiomatic C#, taking advantage of .NET's type system and modern features while maintaining compatibility with Storyblok's APIs. Key aspects we've preserved include:

- Same configuration options and initialization patterns
- Compatible caching mechanisms
- Equivalent rate limiting and retry logic
- Parallel rich text rendering capabilities
- Similar helper utilities and extension methods

## Features

- 🚀 Built for modern .NET (targets .NET 9.0)
- 💪 Strongly typed story content
- 🔄 Automatic rate limiting and retry handling
- 💾 Configurable caching
- 🔍 Rich text rendering
- 🔒 Thread-safe operations
- 📦 Available as a NuGet package
- 🔗 Dependency injection friendly
- 🏎️ Async/await support

## Also:

- 💡Support for both published and draft content
- 💡Comprehensive story querying and filtering
- 💡Rich text rendering with customizable schema
- 💡Built-in caching with memory and custom providers
- 💡Rate limiting and retry handling
- 💡Blazor component integration
- 💡Image optimization
- 💡HTML sanitization

## Installation

Install via Github packages - NuGet:

```bash
dotnet add package StoryblokSharp
```

The NuGet package is hosted in this GitHub repository's package registry.


## Story Retrieval

### Single Story

Retrieve a single story by its slug:

```csharp
// Get a published story
var story = await _client.GetStoryAsync<HomeContent>("home", new StoryQueryParameters 
{
    Version = "published"
});

// Get a draft story
var draftStory = await _client.GetStoryAsync<HomeContent>("home", new StoryQueryParameters 
{
    Version = "draft"
});

// Get a story in a specific language
var germanStory = await _client.GetStoryAsync<HomeContent>("home", new StoryQueryParameters 
{
    Version = "published",
    Language = "de"
});
```

### Multiple Stories

Retrieve multiple stories with filtering and pagination:

```csharp
// Get all published stories
var stories = await _client.GetStoriesAsync<BlogPost>(new StoryQueryParameters 
{
    Version = "published",
    PerPage = 10,
    Page = 1
});

// Get stories with specific tags
var taggedStories = await _client.GetStoriesAsync<BlogPost>(new StoryQueryParameters 
{
    WithTag = "featured",
    SortBy = "created_at:desc"
});

// Get all stories (handles pagination automatically)
var allStories = await _client.GetAllAsync<BlogPost>(
    "cdn/stories",
    new StoryQueryParameters 
    {
        StartsWith = "blog/"
    }
);
```

### Query Parameters

The `StoryQueryParameters` class provides various filtering and sorting options:

```csharp
var parameters = new StoryQueryParameters
{
    // Version control
    Version = "published", // or "draft"
    
    // Pagination
    PerPage = 10,
    Page = 1,
    
    // Filtering
    StartsWith = "blog/",
    WithTag = "featured",
    SearchTerm = "tutorial",
    ExcludingFields = "body,image",
    
    // Sorting
    SortBy = "created_at:desc",
    
    // Language
    Language = "en",
    FallbackLang = "de",
    
    // Relations
    ResolveLinks = "1",
    ResolveRelations = new[] { "author", "categories" },
    ResolveLevel = 2
};
```

### Working with Content Types

You can create strongly-typed models for your content:

```csharp
public class BlogPost
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public RichTextField Content { get; set; }
    public Asset FeaturedImage { get; set; }
    public DateTime PublishedDate { get; set; }
    public string[] Tags { get; set; }
}

// Use the typed model
var response = await _client.GetStoryAsync<BlogPost>("my-blog-post");
var title = response.Story.Content.Title;
var content = response.Story.Content.Content;
```

## Basic Usage

```csharp
// Configure services
services.AddStoryblokClient(builder => builder
    .WithAccessToken("your_access_token")
    .WithCache(options => options
        .WithType(CacheType.Memory)
        .WithDefaultExpiration(TimeSpan.FromMinutes(5)))
    .WithMaxRetries(3)
    .WithRateLimit(5));

// Inject and use the client
public class MyService
{
    private readonly IStoryblokClient _client;

    public MyService(IStoryblokClient client)
    {
        _client = client;
    }

    public async Task<Story<T>> GetStoryAsync<T>() where T : class
    {
        var parameters = new StoryQueryParameters
        {
            Version = "published",
            Language = "en"
        };

        var response = await _client.GetStoryAsync<T>("home", parameters);
        return response.Story;
    }
}
```

## Advanced Configuration

The client can be configured with various options:

```csharp
services.AddStoryblokClient(builder => builder
    .WithAccessToken("your_access_token")
    .WithOAuthToken("your_oauth_token") // For management API
    .WithRegion(Region.EU)
    .WithHttps()
    .WithMaxRetries(3)
    .WithTimeout(30)
    .WithRateLimit(5)
    .WithHeaders(new Dictionary<string, string>
    {
        ["Custom-Header"] = "Value"
    })
    .WithCache(options => options
        .WithType(CacheType.Memory)
        .WithDefaultExpiration(TimeSpan.FromMinutes(5)))
    .WithCustomCache(new YourCustomCacheProvider())
    .WithRichTextSchema(new YourCustomSchema())
    .WithComponentResolver((type, props) => $"<div>{type}</div>")
    .WithResponseInterceptor(async response =>
    {
        // Custom response handling
        return response;
    }));
```

## Rich Text Rendering

The library includes a powerful rich text renderer with support for custom resolvers and comprehensive node handling. The rendering system is built around specialized resolvers that handle different types of content:

### Block Node Resolver
Handles structural elements of the rich text content:

```csharp
// Handles block-level elements like paragraphs, lists, and headings
var content = new RichTextField
{
    Type = "doc",
    Content = new[]
    {
        new RichTextNode
        {
            Type = "paragraph",
            Content = new[] { /* child nodes */ }
        },
        new RichTextNode
        {
            Type = "heading",
            Attrs = new Dictionary<string, object> { { "level", 2 } },
            Content = new[] { /* child nodes */ }
        }
    }
};
```

Supported block types include:
- Document (root container)
- Paragraph (p)
- Bullet List (ul)
- Ordered List (ol)
- List Item (li)
- Quote (blockquote)
- Code Block (pre)
- Heading (h1-h6)
- Horizontal Rule (hr)
- Hard Break (br)

### Mark Node Resolver
Handles inline text formatting and styling:

```csharp
// Example of text with multiple marks
var textNode = new TextNode
{
    Text = "Styled text",
    Marks = new[]
    {
        new MarkNode { MarkType = MarkTypes.Bold },
        new MarkNode
        {
            MarkType = MarkTypes.Link,
            Attrs = new Dictionary<string, object>
            {
                { "href", "https://example.com" },
                { "target", "_blank" }
            }
        }
    }
};
```

Supported mark types:
- Bold/Strong
- Italic
- Strike
- Underline
- Code
- Link
- Styled (custom styles)
- Superscript
- Subscript
- Text Style (classes)
- Highlight

### Image Node Resolver
Handles image rendering with optimization capabilities:

```csharp
services.Configure<RichTextOptions>(options =>
{
    options.OptimizeImages = true;
    options.ImageOptions = new ImageOptimizationOptions
    {
        Width = 800,
        Height = 600,
        Loading = "lazy",
        Class = "optimized-image",
        SrcSet = new[]
        {
            new SrcSetEntry { Width = 400 },
            new SrcSetEntry { Width = 800 }
        },
        Sizes = new[]
        {
            "(max-width: 400px) 100vw",
            "800px"
        }
    }
});
```

### Text Node Resolver
Handles basic text content with HTML encoding:

```csharp
// Simple text node
var node = new TextNode
{
    Type = "text",
    Text = "Hello, world!"
};

// Text with marks
var styledNode = new TextNode
{
    Type = "text",
    Text = "Important text",
    Marks = new[]
    {
        new MarkNode { MarkType = MarkTypes.Bold }
    }
};
```

### Configuration Options

Configure the rich text renderer with various options:

```csharp
services.Configure<RichTextOptions>(options =>
{
    options.OptimizeImages = true;
    options.KeyedResolvers = true;
    options.InvalidNodeHandling = InvalidNodeStrategy.Remove;
    options.MarkSortPriority = new[]
    {
        MarkTypes.Bold,
        MarkTypes.Italic,
        MarkTypes.Link
    };
});
```

The resolver system is extensible, allowing you to create custom resolvers for specific content types:
```

## Blazor Integration

StoryblokSharp provides seamless integration with Blazor components:

```csharp
// Register Blazor component resolver
services.AddBlazorComponentResolver();

// Register a component
services.AddStoryblokComponent<HeroComponent>("hero");

// Create a Blazor component
public class HeroComponent : ComponentBase, IComponent
{
    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public string Subtitle { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "hero");
        
        builder.OpenElement(2, "h1");
        builder.AddContent(3, Title);
        builder.CloseElement();
        
        builder.OpenElement(4, "p");
        builder.AddContent(5, Subtitle);
        builder.CloseElement();
        
        builder.CloseElement();
    }
}

// Use in Storyblok content
@{
    var story = await StoryblokClient.GetStoryAsync<dynamic>("home");
    var component = story.Story.Content.hero;
}

<StoryblokComponent Type="hero" Props="@component" />
```

## Image Optimization

Configure image optimization options:

```csharp
services.Configure<RichTextOptions>(options =>
{
    options.OptimizeImages = true;
    options.ImageOptions = new ImageOptimizationOptions
    {
        Width = 800,
        Height = 600,
        Loading = "lazy",
        Class = "optimized-image",
        SrcSet = new[]
        {
            new SrcSetEntry { Width = 400 },
            new SrcSetEntry { Width = 800 },
            new SrcSetEntry { Width = 1200 }
        },
        Sizes = new[]
        {
            "(max-width: 400px) 100vw",
            "(max-width: 800px) 50vw",
            "800px"
        },
        Filters = new ImageFilters
        {
            Quality = 80,
            Format = "webp",
            Grayscale = false
        }
    }
});
```

## Security

The library includes built-in HTML sanitization:

```csharp
services.Configure<HtmlSanitizerOptions>(options =>
{
    options.AllowedTags.Add("custom-tag");
    options.AllowedAttributes["a"].Add("rel");
    options.AllowedProtocols.Add("tel");
});
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

You can customize story queries with various parameters:

```csharp
var parameters = new StoryQueryParameters
{
    Version = "draft",                    // 'draft' or 'published'
    Language = "en",                      // Language code
    ResolveLinks = "story",              // How to resolve links
    ResolveRelations = new[] { "author" },// Relations to resolve
    ExcludingFields = "long_text",       // Fields to exclude
    Sort_by = "position:desc",           // Sort order
    StartsWith = "blog/",                // Filter by path
    WithTag = "featured",                // Filter by tag
    Page = 1,                            // Page number
    PerPage = 10                         // Items per page
};

var stories = await _storyblok.GetStoriesAsync<BlogPostContent>(parameters);
```


## Security

The library includes built-in HTML sanitization:

```csharp
services.Configure<HtmlSanitizerOptions>(options =>
{
    options.AllowedTags.Add("custom-tag");
    options.AllowedAttributes["a"].Add("rel");
    options.AllowedProtocols.Add("tel");
});
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Documentation](https://github.com/juryrigcoder/storybloksharp/wiki)
- [Issue Tracker](https://github.com/juryrigcoder/storybloksharp/issues)
- [Storyblok API Reference](https://www.storyblok.com/docs/api/content-delivery)