namespace VideoUiDemo.Models;

/// <summary>
/// Metadata for a video, sourced from the official YouTube Data API v3
/// (videos.list: snippet + contentDetails + statistics + status +
/// topicDetails). This does NOT include download URLs or format/bitrate
/// options — the official API does not expose those, and this app does
/// not attempt to derive them.
/// </summary>
public class VideoMetadata
{
    // --- Overview ---
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string ChannelTitle { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public TimeSpan Duration { get; set; }

    // --- Stats ---
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? CommentCount { get; set; }
    public long? FavoriteCount { get; set; }

    // --- Tags & category ---
    public List<string> Tags { get; set; } = new();
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // --- Technical details (contentDetails) ---
    public string? Definition { get; set; }        // "hd" or "sd"
    public bool HasCaptions { get; set; }
    public bool LicensedContent { get; set; }
    public string? Dimension { get; set; }          // "2d" or "3d"
    public string? Projection { get; set; }         // "rectangular", "360", etc.

    // --- Status ---
    public string? PrivacyStatus { get; set; }      // "public", "unlisted", "private"
    public string? License { get; set; }            // "youtube" or "creativeCommon"
    public bool Embeddable { get; set; }
    public bool MadeForKids { get; set; }

    // --- Topics ---
    public List<string> TopicCategories { get; set; } = new();

    public string DurationDisplay =>
        Duration.TotalHours >= 1
            ? Duration.ToString(@"h\:mm\:ss")
            : Duration.ToString(@"m\:ss");
}

public enum LoadState
{
    Idle,
    Loading,
    Loaded,
    Error
}
