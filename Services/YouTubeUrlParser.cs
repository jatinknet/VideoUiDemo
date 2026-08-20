using System.Text.RegularExpressions;

namespace VideoUiDemo.Services;

/// <summary>
/// Parses a YouTube video ID out of the common URL shapes:
/// watch?v=ID, youtu.be/ID, /shorts/ID, /embed/ID.
/// Returns null if the input doesn't look like a YouTube video URL.
/// </summary>
public static class YouTubeUrlParser
{
    private static readonly Regex VideoIdPattern = new(@"^[a-zA-Z0-9_-]{11}$", RegexOptions.Compiled);

    public static string? ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        var host = uri.Host.Replace("www.", string.Empty, StringComparison.OrdinalIgnoreCase);

        // youtu.be/VIDEOID
        if (host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            var id = uri.AbsolutePath.Trim('/');
            return IsValidId(id) ? id : null;
        }

        if (host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("m.youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            // youtube.com/watch?v=VIDEOID
            var vParam = GetQueryParam(uri.Query, "v");
            if (IsValidId(vParam))
                return vParam;

            // youtube.com/shorts/VIDEOID or /embed/VIDEOID or /live/VIDEOID
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 &&
                (segments[0] is "shorts" or "embed" or "live"))
            {
                var id = segments[1];
                return IsValidId(id) ? id : null;
            }
        }

        return null;
    }

    private static bool IsValidId(string? id) =>
        !string.IsNullOrEmpty(id) && VideoIdPattern.IsMatch(id);

    private static string? GetQueryParam(string query, string key)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        // query starts with '?'; split into key=value pairs.
        var pairs = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == key)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
