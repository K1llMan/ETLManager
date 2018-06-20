using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETLService.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class RootController : Controller
    {
        // GET api/modules
        /// <summary>
        /// Возвращает доступные для отображения модули
        /// </summary>
        [HttpGet("modules")]
        public object GetModules()
        {
            if (User.IsInRole("Admin"))
                return new Dictionary<string, Dictionary<string, string>> {
                    { string.Empty, new Dictionary<string, string> {
                            { "displayName", "Admin scenarios" },
                            { "script", "scenarios.js" },
                            { "template", "scenarios.tmp" }
                        } 
                    }
                };

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