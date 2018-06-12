using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

namespace ETLService.Controllers
{
    [Produces("application/json")]
    [Route("api/modules")]
    public class RootController : Controller
    {
        // GET api/modules
        /// <summary>
        /// Возвращает доступные для отображения модули
        /// </summary>
        [HttpGet("")]      
        public object GetModules()
        {
            return new Dictionary<string, Dictionary<string, string>> {
                { string.Empty, new Dictionary<string, string> {
                        { "displayName", "Scenarios" },
                        { "script", "scenarios.js" },
                        { "template", "scenarios.tmp" }
                    } 
                }
            };
        }
    }
}