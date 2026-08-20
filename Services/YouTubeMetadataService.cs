using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Configuration;
using VideoUiDemo.Models;

namespace VideoUiDemo.Services;

/// <summary>
/// Fetches video metadata from the official YouTube Data API v3
/// (videos.list, parts: snippet, contentDetails, statistics, status,
/// topicDetails; plus a small follow-up call to videoCategories.list to
/// resolve the category name). This only reads metadata YouTube's public
/// API exposes for display — it never fetches or constructs any
/// download/stream URL.
///
/// Requires a YouTube Data API v3 key. Configure it via one of:
///   - appsettings.json:      "YouTube": { "ApiKey": "..." }
///   - dotnet user-secrets:   dotnet user-secrets set "YouTube:ApiKey" "..."
///   - environment variable:  YouTube__ApiKey
/// See README.md for setup steps.
/// </summary>
public class YouTubeMetadataService : IVideoMetadataService
{
    private const string VideosUrl = "https://www.googleapis.com/youtube/v3/videos";
    private const string CategoriesUrl = "https://www.googleapis.com/youtube/v3/videoCategories";

    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public YouTubeMetadataService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["YouTube:ApiKey"];
    }

    public async Task<VideoMetadata> GetMetadataAsync(string videoUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("A video URL is required.", nameof(videoUrl));

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "No YouTube API key configured. Set YouTube:ApiKey in appsettings.json, " +
                "via 'dotnet user-secrets set \"YouTube:ApiKey\" \"YOUR_KEY\"', or the " +
                "YouTube__ApiKey environment variable.");
        }

        var videoId = YouTubeUrlParser.ExtractVideoId(videoUrl);
        if (videoId is null)
        {
            throw new FormatException("That doesn't look like a valid YouTube video URL.");
        }

        var requestUrl = $"{VideosUrl}?id={Uri.EscapeDataString(videoId)}" +
                          "&part=snippet,contentDetails,statistics,status,topicDetails" +
                          $"&key={Uri.EscapeDataString(_apiKey)}";

        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = TryExtractErrorMessage(body) ??
                           $"YouTube API request failed ({(int)response.StatusCode}).";
            throw new InvalidOperationException(message);
        }

        var parsed = JsonSerializer.Deserialize<YouTubeVideosResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var item = parsed?.Items.FirstOrDefault();
        if (item is null)
        {
            throw new InvalidOperationException("Video not found. It may be private, deleted, or region-restricted.");
        }

        var metadata = MapToMetadata(item);

        // Best-effort: resolve the numeric category ID into a readable name.
        // If this fails for any reason, we still show everything else.
        if (!string.IsNullOrEmpty(metadata.CategoryId))
        {
            metadata.CategoryName = await TryResolveCategoryNameAsync(metadata.CategoryId, cancellationToken);
        }

        return metadata;
    }

    private static VideoMetadata MapToMetadata(YouTubeVideoItem item)
    {
        var snippet = item.Snippet;
        var thumbnail =
            snippet?.Thumbnails?.MaxRes?.Url ??
            snippet?.Thumbnails?.High?.Url ??
            snippet?.Thumbnails?.Medium?.Url ??
            snippet?.Thumbnails?.Default?.Url ??
            string.Empty;

        return new VideoMetadata
        {
            VideoId = item.Id,
            Title = snippet?.Title ?? "(untitled)",
            Description = snippet?.Description ?? string.Empty,
            ThumbnailUrl = thumbnail,
            ChannelTitle = snippet?.ChannelTitle ?? string.Empty,
            PublishedAt = snippet?.PublishedAt,
            Duration = ParseIso8601Duration(item.ContentDetails?.Duration),

            ViewCount = ParseCount(item.Statistics?.ViewCount),
            LikeCount = ParseCount(item.Statistics?.LikeCount),
            CommentCount = ParseCount(item.Statistics?.CommentCount),
            FavoriteCount = ParseCount(item.Statistics?.FavoriteCount),

            Tags = snippet?.Tags ?? new List<string>(),
            CategoryId = snippet?.CategoryId,

            Definition = item.ContentDetails?.Definition,
            HasCaptions = string.Equals(item.ContentDetails?.Caption, "true", StringComparison.OrdinalIgnoreCase),
            LicensedContent = item.ContentDetails?.LicensedContent ?? false,
            Dimension = item.ContentDetails?.Dimension,
            Projection = item.ContentDetails?.Projection,

            PrivacyStatus = item.Status?.PrivacyStatus,
            License = item.Status?.License,
            Embeddable = item.Status?.Embeddable ?? false,
            MadeForKids = item.Status?.MadeForKids ?? false,

            TopicCategories = item.TopicDetails?.TopicCategories ?? new List<string>(),
        };
    }

    private async Task<string?> TryResolveCategoryNameAsync(string categoryId, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{CategoriesUrl}?id={Uri.EscapeDataString(categoryId)}" +
                      $"&part=snippet&key={Uri.EscapeDataString(_apiKey!)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<YouTubeCategoriesResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return parsed?.Items.FirstOrDefault()?.Snippet?.Title;
        }
        catch
        {
            // Category name is a nice-to-have; never let it break the main result.
            return null;
        }
    }

    private static long? ParseCount(string? raw) =>
        long.TryParse(raw, out var value) ? value : null;

    private static TimeSpan ParseIso8601Duration(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso))
            return TimeSpan.Zero;

        try
        {
            return XmlConvert.ToTimeSpan(iso);
        }
        catch (FormatException)
        {
            return TimeSpan.Zero;
        }
    }

    private static string? TryExtractErrorMessage(string body)
    {
        try
        {
            var error = JsonSerializer.Deserialize<YouTubeErrorResponse>(body);
            return error?.Error?.Message;
        }
        catch
        {
            return null;
        }
    }
}
