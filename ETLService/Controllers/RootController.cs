using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

namespace ETLService.Controllers
{
    [Produces("application/json")]
    [Route("api/modules")]
    public class RootController : Controller
    {
        // GET api/modules
        [HttpGet("")]
        public object GetModules(string id)
        {
            return new Dictionary<string, Dictionary<string, string>> {
                { string.Empty, new Dictionary<string, string> {
                        { "displayImage", "logo.png" },
                        { "script", "main.js" },
                        { "template", "main.tmp" }
                    } 
                }
            };
        }
    }
}