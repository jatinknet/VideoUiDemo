# Video Metadata Viewer (Blazor Server + YouTube Data API)

Paste a YouTube URL, see its real title, thumbnail, channel, duration,
view/like/comment counts, and description — fetched live from the
**official YouTube Data API v3**. There is **no download or file-conversion
logic anywhere in this project**; it only reads metadata the API exposes
for display.

## 1. Get a YouTube Data API key

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
2. Create a project (or pick an existing one).
3. Go to **APIs & Services → Library**, search for **YouTube Data API v3**,
   and click **Enable**.
4. Go to **APIs & Services → Credentials → Create Credentials → API key**.
5. Copy the key. (Optional but recommended: restrict it to the YouTube Data
   API v3 and to your IP/app in the key's settings.)

The free quota is 10,000 units/day; each `videos.list` call here costs a
handful of units, so normal testing won't come close to the limit.

## 2. Configure the key

Pick **one** of these — don't commit a real key to source control:

**Option A — user-secrets (recommended for local dev)**
```bash
cd VideoUiDemo
dotnet user-secrets init
dotnet user-secrets set "YouTube:ApiKey" "YOUR_API_KEY_HERE"
```

**Option B — environment variable**
```bash
# macOS/Linux
export YouTube__ApiKey="YOUR_API_KEY_HERE"

# Windows (PowerShell)
$env:YouTube__ApiKey="YOUR_API_KEY_HERE"
```

**Option C — appsettings.json** (fine for quick local testing only)
```json
{
  "YouTube": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

## 3. Run

```bash
cd VideoUiDemo
dotnet restore
dotnet run
```

Open the URL printed in the console, paste a YouTube URL (e.g.
`https://www.youtube.com/watch?v=dQw4w9WgXcQ` or a `youtu.be/...` /
`/shorts/...` link), and click **Start**.

## Project layout

```
VideoUiDemo/
├── Program.cs                    # App startup, DI, typed HttpClient
├── App.razor                     # Router root
├── _Imports.razor
├── Pages/
│   ├── _Host.cshtml              # HTML shell that boots Blazor Server
│   └── Index.razor               # Main page: orchestrates state + components
├── Shared/
│   ├── MainLayout.razor
│   ├── VideoInput.razor          # URL text box + validation + submit
│   ├── VideoPreview.razor        # Thumbnail + duration badge + title/channel
│   └── VideoDetails.razor        # Views/likes/comments/published + description
├── Models/
│   └── VideoModels.cs            # VideoMetadata, LoadState
├── Services/
│   ├── IVideoMetadataService.cs  # Interface
│   ├── YouTubeMetadataService.cs # Real implementation calling the API
│   ├── YouTubeApiModels.cs       # DTOs matching the API's JSON shape
│   └── YouTubeUrlParser.cs       # Extracts a video ID from any YouTube URL shape
└── wwwroot/css/site.css
```

## Data flow

1. User pastes a URL into `VideoInput` → two-way bound to `Index._url`.
2. User submits → `Index.HandleSubmitAsync` runs, creates a fresh
   `CancellationTokenSource` (cancelling any prior in-flight fetch), state
   → `Loading`.
3. `YouTubeMetadataService.GetMetadataAsync`:
   - Parses a video ID out of the URL (`YouTubeUrlParser`).
   - Calls `GET https://www.googleapis.com/youtube/v3/videos` with
     `part=snippet,contentDetails,statistics,status,topicDetails`.
   - Maps the response into `VideoMetadata` — title, thumbnail, channel,
     ISO-8601 duration parsed into a `TimeSpan`, view/like/comment/favorite
     counts, tags, category ID, HD/SD + captions + dimension/projection,
     privacy/license/embeddable/madeForKids status, and topic categories.
   - Makes one more small call to `GET .../videoCategories` to resolve the
     numeric category ID into a readable name (e.g. "Music", "Gaming").
     This is best-effort — if it fails, everything else still renders.
4. On success: state → `Loaded`. `VideoPreview` shows the thumbnail
   (click to swap in YouTube's official embedded player, if the uploader
   allows embedding) plus a link to the channel. `VideoDetails` renders the
   rest in labeled sections: **Channel** (avatar, subscribers, total
   views/videos), **Stats**, **Tags & Category**, **Technical Details**
   (HD/SD, captions, dimension/projection, content ratings, region
   restrictions), **Status**, **Topics**, and **Description**.
5. On failure (bad URL, missing/invalid key, video not found, quota
   exceeded, network error): state → `Error`, an accessible
   (`role="alert"`) message is shown with a specific reason where possible.

## State management & disposal

- All state lives in `Index.razor` — nothing is cached statically or shared
  across users.
- `IVideoMetadataService` is registered via `AddHttpClient<...>`, which is
  scoped-per-request under the hood — no shared mutable state between
  circuits.
- `Index` implements `IDisposable`, cancels/disposes its
  `CancellationTokenSource` on teardown, and cancels any prior fetch before
  starting a new one, to avoid leaked tasks or stale UI updates.

## Scope / what this intentionally does not do

- No downloading, no conversion, no format/bitrate list — the official API
  doesn't expose that, and this app doesn't try to derive or scrape it.
- No storage of fetched metadata beyond the current page view.
