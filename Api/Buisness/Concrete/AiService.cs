using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Buisness.Abasctraction;
using Api.Controllers;
using Api.Dtos;
using Api.Models;

namespace Api.Buisness.Concrete
{
     public class AiService : IAiService
     {
          private IConfiguration _config;
          private HttpClient   _httpClient;
          private IFindMusicIdAndAddToList findMusicIdAndAddToList;
          public AiService(IConfiguration configuration, HttpClient httpClient, IFindMusicIdAndAddToList findMusicIdAndAddToList)
          {
               _config = configuration;
               _httpClient = httpClient;
               this.findMusicIdAndAddToList = findMusicIdAndAddToList;
               _httpClient.Timeout = TimeSpan.FromSeconds(180); 
          }
   public async Task<object> GetMusicList(PromptDto model)
{
    string apiKey = _config["OpenRouter:ApiKey"];

    var prompt = $"""
        Kullanıcı şu müzik tarzını ve sanatçıları verdi:
        "sanatçılar:{model.Artists} .Müzik tarzı{model.Style}"

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
_httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    if (!_httpClient.DefaultRequestHeaders.Contains("HTTP-Referer"))
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://halilcetin.online"); // zorunlu!

    if (!_httpClient.DefaultRequestHeaders.Contains("X-Title"))
        _httpClient.DefaultRequestHeaders.Add("X-Title", "MusicGPT"); // isteğe bağlı

    try
    {
        var response = await _httpClient.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        if (!response.IsSuccessStatusCode)
        {
            return $"OpenRouter API hatası: {response.StatusCode}";
        }

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

        var musicNamelist = ParseSongLines(songs);
        var findMusicresult = await findMusicIdAndAddToList.FindMusicForId(musicNamelist, model.AccesToken, model.PlayListId);

        return findMusicresult;
    }
    catch (TaskCanceledException ex)
    {
        return $"İstek zaman aşımına uğradı. Hata: {ex.Message}";
    }
    catch (HttpRequestException ex)
    {
        return $"HTTP isteği başarısız oldu: {ex.Message}";
    }
    catch (Exception ex)
    {
        return $"Bilinmeyen bir hata oluştu: {ex.Message}";
    }
}



          private List<SongRequest> ParseSongLines(List<string> rawSongs)
{
    var result = new List<SongRequest>();

    foreach (var line in rawSongs)
    {
        var trimmed = line.Trim();

        var withoutNumber = trimmed.IndexOf('.') > 0
            ? trimmed.Substring(trimmed.IndexOf('.') + 1).Trim()
            : trimmed;

        var parts = withoutNumber.Split(" - ");
        if (parts.Length == 2)
        {
            result.Add(new SongRequest
            {
                Artist = parts[0].Trim(),
                TrackName = parts[1].Trim()
            });
        }
    }

    return result;
}
     }





     
}