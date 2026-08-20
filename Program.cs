using VideoUiDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor components + Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Typed HttpClient for calling the YouTube Data API.
builder.Services.AddHttpClient<IVideoMetadataService, YouTubeMetadataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
