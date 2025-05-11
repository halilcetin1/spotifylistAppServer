using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Buisness.Abasctraction;
using Api.Controllers;
using Api.Models;


namespace Api.Buisness.Concrete
{
     public class FindMusicIdAndAddToList :IFindMusicIdAndAddToList
     {
      private HttpClient _httpClient;
      public FindMusicIdAndAddToList(HttpClient httpClient)
      {
          _httpClient=httpClient;
      }

          public async Task<object> FindMusicForId(List<SongRequest> songs,string AccesToken,string PlayListId)
          {
               var uris = new List<object>();
               
// if (AccesToken.Any(c => c > 127))
// {
//     throw new Exception("Access token içinde ASCII dışı karakter var.");
// }
    foreach (var song in songs)
    {
        var query = $"{song.Artist} {song.TrackName}";
        var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=1";
_httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccesToken.Trim());
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            uris.Add(new { song.Artist, song.TrackName, Uri = (string?)null, Error = "Spotify API error" });
            continue;
        }

        var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("tracks").GetProperty("items");

        if (items.GetArrayLength() == 0)
        {
            uris.Add(new {  Uri = (string?)null, Error = "Not found" });
            continue;
        }

        var uri = items[0].GetProperty("uri").GetString();
        uris.Add(new { Uri = uri });
    }
   var res= await  AddmusicTolist(uris,AccesToken,PlayListId);
return  res;




          }


          private async Task<object> AddmusicTolist(List<object> tracks,string accestoken,string playlistId){



_httpClient.DefaultRequestHeaders.Clear();
_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accestoken);
    var uriList = tracks
        .Select(t => t.GetType().GetProperty("Uri")?.GetValue(t)?.ToString())
        .Where(uri => !string.IsNullOrWhiteSpace(uri))
        .ToList();
    var payload = new
    {                         
        uris = uriList
    };

      var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", content);
    var result = await response.Content.ReadAsStringAsync();
    if(response.IsSuccessStatusCode) return response;
return "hata ";
  

    
    


          }
     }
}