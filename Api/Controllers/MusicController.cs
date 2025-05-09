using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MusicAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MusicController(IHttpClientFactory factory, IConfiguration config)
    {
        _httpClient = factory.CreateClient();
        _config = config;
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] PromptRequest request)
    {
     // 
        string apiKey = _config["OpenRouter:ApiKey"];

        var prompt = $"""
        Kullanıcı şu müzik tarzını ve sanatçıları verdi:
        "{request.Prompt}"

        Bu tanıma uygun 30 şarkı öner. Format şu şekilde olsun:
        1. Şarkıcı - Şarkı Adı
        2. ...
        """;

        var requestBody = new
        {
            model = "openai/gpt-3.5-turbo", 
            messages = new[] {
                new { role = "user", content = prompt }
            },
            temperature = 0.7
        };

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5173"); // zorunlu!
        _httpClient.DefaultRequestHeaders.Add("X-Title", "MusicGPT"); // isteğe bağlı

        var response = await _httpClient.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "OpenRouter API hatası");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json);
        var content = result.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        var songs = content.Split('\n')
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .ToList();

        return Ok(new { songs });
    }
}

public class PromptRequest
{
    public string Prompt { get; set; }
}