using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideoUiDemo.Services;

// Minimal DTOs covering the fields this app uses from
// GET https://www.googleapis.com/youtube/v3/videos
// and https://www.googleapis.com/youtube/v3/videoCategories

internal sealed class YouTubeVideosResponse
{
    [JsonPropertyName("items")]
    public List<YouTubeVideoItem> Items { get; set; } = new();
}

internal sealed class YouTubeVideoItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("snippet")]
    public YouTubeSnippet? Snippet { get; set; }

    [JsonPropertyName("contentDetails")]
    public YouTubeContentDetails? ContentDetails { get; set; }

    [JsonPropertyName("statistics")]
    public YouTubeStatistics? Statistics { get; set; }

    [JsonPropertyName("status")]
    public YouTubeStatus? Status { get; set; }

    [JsonPropertyName("topicDetails")]
    public YouTubeTopicDetails? TopicDetails { get; set; }
}

internal sealed class YouTubeSnippet
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("channelTitle")]
    public string ChannelTitle { get; set; } = string.Empty;

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnails? Thumbnails { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("categoryId")]
    public string? CategoryId { get; set; }
}

internal sealed class YouTubeThumbnails
{
    [JsonPropertyName("maxres")]
    public YouTubeThumbnail? MaxRes { get; set; }

    [JsonPropertyName("high")]
    public YouTubeThumbnail? High { get; set; }

    [JsonPropertyName("medium")]
    public YouTubeThumbnail? Medium { get; set; }

    [JsonPropertyName("default")]
    public YouTubeThumbnail? Default { get; set; }
}

internal sealed class YouTubeThumbnail
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

internal sealed class YouTubeContentDetails
{
    [JsonPropertyName("duration")]
    public string Duration { get; set; } = string.Empty; // ISO 8601, e.g. "PT12M34S"

    [JsonPropertyName("definition")]
    public string? Definition { get; set; } // "hd" or "sd"

    [JsonPropertyName("caption")]
    public string? Caption { get; set; } // "true" or "false" (string, not bool)

    [JsonPropertyName("licensedContent")]
    public bool LicensedContent { get; set; }

    [JsonPropertyName("dimension")]
    public string? Dimension { get; set; } // "2d" or "3d"

    [JsonPropertyName("projection")]
    public string? Projection { get; set; } // "rectangular", "360", etc.

    [JsonPropertyName("contentRating")]
    public YouTubeContentRating? ContentRating { get; set; }

    [JsonPropertyName("regionRestriction")]
    public YouTubeRegionRestriction? RegionRestriction { get; set; }
}

internal sealed class YouTubeContentRating
{
    // The API returns many possible rating-system keys (mpaaRating, tvpgRating,
    // ytRating, etc.) only when a rating applies. We just grab whichever are
    // present via a dictionary rather than declaring every possible key.
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Ratings { get; set; }
}

internal sealed class YouTubeRegionRestriction
{
    [JsonPropertyName("allowed")]
    public List<string>? Allowed { get; set; }

    [JsonPropertyName("blocked")]
    public List<string>? Blocked { get; set; }
}

internal sealed class YouTubeStatistics
{
    [JsonPropertyName("viewCount")]
    public string? ViewCount { get; set; }

    [JsonPropertyName("likeCount")]
    public string? LikeCount { get; set; }

    [JsonPropertyName("commentCount")]
    public string? CommentCount { get; set; }

    [JsonPropertyName("favoriteCount")]
    public string? FavoriteCount { get; set; }
}

internal sealed class YouTubeStatus
{
    [JsonPropertyName("privacyStatus")]
    public string? PrivacyStatus { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; } // "youtube" or "creativeCommon"

    [JsonPropertyName("embeddable")]
    public bool Embeddable { get; set; }

    [JsonPropertyName("madeForKids")]
    public bool MadeForKids { get; set; }
}

internal sealed class YouTubeTopicDetails
{
    [JsonPropertyName("topicCategories")]
    public List<string>? TopicCategories { get; set; }
}

internal sealed class YouTubeErrorResponse
{
    [JsonPropertyName("error")]
    public YouTubeError? Error { get; set; }
}

internal sealed class YouTubeError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

// --- videoCategories.list (used to resolve categoryId -> a readable name) ---

internal sealed class YouTubeCategoriesResponse
{
    [JsonPropertyName("items")]
    public List<YouTubeCategoryItem> Items { get; set; } = new();
}

internal sealed class YouTubeCategoryItem
{
    [JsonPropertyName("snippet")]
    public YouTubeCategorySnippet? Snippet { get; set; }
}

internal sealed class YouTubeCategorySnippet
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

// --- channels.list (used to fetch full channel details) ---

internal sealed class YouTubeChannelsResponse
{
    [JsonPropertyName("items")]
    public List<YouTubeChannelItem> Items { get; set; } = new();
}

internal sealed class YouTubeChannelItem
{
    [JsonPropertyName("snippet")]
    public YouTubeChannelSnippet? Snippet { get; set; }

    [JsonPropertyName("statistics")]
    public YouTubeChannelStatistics? Statistics { get; set; }
}

internal sealed class YouTubeChannelSnippet
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnails? Thumbnails { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; } // channel creation date
}

internal sealed class YouTubeChannelStatistics
{
    [JsonPropertyName("subscriberCount")]
    public string? SubscriberCount { get; set; }

    [JsonPropertyName("hiddenSubscriberCount")]
    public bool HiddenSubscriberCount { get; set; }

    [JsonPropertyName("videoCount")]
    public string? VideoCount { get; set; }

    [JsonPropertyName("viewCount")]
    public string? ViewCount { get; set; }
}
