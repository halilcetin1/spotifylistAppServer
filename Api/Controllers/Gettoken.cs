using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SpotifyController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public SpotifyController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _http = httpClientFactory.CreateClient();
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] string code)
    {
        var clientId = _config["Spotify:clientId"];
        var clientSecret = _config["Spotify:clientSecretKey"];
        var redirectUri = "https://spotifyapp.halilcetin.online/callback";

    
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        });

        var response = await _http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest("Token alınamadı: " + content);

        var tokenObj = JsonDocument.Parse(content).RootElement;
        var accessToken = tokenObj.GetProperty("access_token").GetString();

        // 2. Kullanıcı bilgilerini al
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var userRes = await _http.GetAsync("https://api.spotify.com/v1/me");
        var userJson = await userRes.Content.ReadAsStringAsync();
        var userId = JsonDocument.Parse(userJson).RootElement.GetProperty("id").GetString();

        // 3. Playlist oluştur
        var playlistContent = new StringContent(JsonSerializer.Serialize(new
        {
            name = "AI Önerilen Şarkılar",
            description = "Yapay zeka tarafından önerilen müzik listesi",
            @public = false
        }), Encoding.UTF8, "application/json");

        var playlistRes = await _http.PostAsync($"https://api.spotify.com/v1/users/{userId}/playlists", playlistContent);
        var playlistJson = await playlistRes.Content.ReadAsStringAsync();
        var playlistId = JsonDocument.Parse(playlistJson).RootElement.GetProperty("id").GetString();

        return Ok(new
        {
            access_token = accessToken,
            playlist_id = playlistId
        });
    }
}