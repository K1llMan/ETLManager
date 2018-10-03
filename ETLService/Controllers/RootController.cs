using System.Collections.Generic;

using ETLService.Manager;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

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

        // GET api/info
        [HttpGet("info")]
        public object GetVersion()
        {
            return new Dictionary<string, object> {
                { "version", Program.Manager.Context.Version }
            };
        }

        // GET api/updates
        [HttpGet("updates")]
        public object GetUpdates()
        {
            return WebAPI.Success(Program.Manager.UpdateManager.Updates);
        }

        // PUT api/updates
        [HttpPut("updates")]
        public object UpdateRegistry([FromBody]string[] updates)
        {
            int count = Program.Manager.UpdateManager.Updates.Count;
            return WebAPI.Success($"Применено {Program.Manager.ApplyUpdates(updates)} из {count}");
        }

    }
}