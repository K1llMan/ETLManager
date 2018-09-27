using System.Collections.Generic;

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
                            { "script", "Scenarios" }
                        } 
                    }
                };

            return new Dictionary<string, Dictionary<string, string>> {
                { string.Empty, new Dictionary<string, string> {
                        { "displayName", "Scenarios" },
                        { "script", "Scenarios" }
                    }
                },
                { "#history", new Dictionary<string, string> {
                        { "displayName", "History" },
                        { "script", "History" }
                    }
                }
            };
        }

        [HttpGet("info")]
        public object GetVersion()
        {
            return new Dictionary<string, object> {
                { "version", Program.Manager.Context.Version }
            };
        }
    }
}