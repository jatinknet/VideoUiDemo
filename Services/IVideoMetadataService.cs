using VideoUiDemo.Models;

namespace VideoUiDemo.Services;

/// <summary>
/// Abstraction for fetching display-only video metadata (title, thumbnail,
/// channel, duration, stats). No download/stream method exists here by
/// design — this app shows metadata only.
/// </summary>
public interface IVideoMetadataService
{
    Task<VideoMetadata> GetMetadataAsync(string videoUrl, CancellationToken cancellationToken = default);
}
