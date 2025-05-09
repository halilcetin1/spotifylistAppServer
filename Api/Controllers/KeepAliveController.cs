using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KeepAliveController : ControllerBase
    {
        [HttpGet("run")]
        public IActionResult get(){
            return  Ok("app is run");
        }
    }
}