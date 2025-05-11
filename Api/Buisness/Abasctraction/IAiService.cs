using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos;

namespace Api.Buisness.Abasctraction
{
    public interface IAiService
    {
        public Task<object> GetMusicList(PromptDto model);
    }
}