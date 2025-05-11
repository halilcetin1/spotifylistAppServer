using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Buisness.Abasctraction;
using Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunnAppController : ControllerBase
    {
        private IAiService aiService;
        public RunnAppController(IAiService aiService)
        {
            this.aiService=aiService;
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunApp(PromptDto model){
            var res=await aiService.GetMusicList(model);
            return Ok(res);
        }
    }
}