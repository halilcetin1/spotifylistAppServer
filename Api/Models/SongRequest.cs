using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class SongRequest
    {
        public string Artist { get; set; }
        public string TrackName { get; set; }
    }
}