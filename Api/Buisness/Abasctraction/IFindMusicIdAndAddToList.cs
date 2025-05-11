

using Api.Controllers;
using Api.Models;

namespace Api.Buisness.Abasctraction
{
    
    public interface IFindMusicIdAndAddToList 
    {
        
        public Task<object> FindMusicForId(List<SongRequest> musicnames,string AccesToken,string PlayListId);
    }
}