using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Dtos
{
    public class PromptDto
    {
        public string Artists { get; set; }

        public string Style { get; set; }
        public string AccesToken { get; set; }
        public string PlayListId { get; set; }
    }
}