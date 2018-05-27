using System.Collections.Generic;

using ETLService.Manager;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace ETLService.Controllers
{
    [Route("api/[controller]")]
    public class PumpsController : Controller
    {
        // GET api/pumps/registry
        [HttpGet("registry")]
        public object GetRegistry()
        {
            return WebAPI.OK(Program.Manager.Pumps.Values);
        }

        // GET api/pumps/registry/pump1
        [HttpGet("registry/{id}")]
        public object GetRegistry(string id)
        {
            if (Program.Manager.Pumps.ContainsKey(id))
                WebAPI.OK(Program.Manager.Pumps[id]);

            return WebAPI.Error("Отсутствует закачка с заданным идентификатором.");
        }

        // GET api/pumps/registry/pump1
        [HttpGet("execute/{id}")]
        public object Execute(string id)
        {
            Program.Manager.Execute(id);

            return WebAPI.OK("-1");
        }
    }
}
