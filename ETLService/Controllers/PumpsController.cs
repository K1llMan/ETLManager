using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ETLService.Controllers
{
    [Route("api/[controller]")]
    public class PumpsController : Controller
    {
        // GET api/pumps/registry
        [HttpGet("registry")]
        public IEnumerable<string> GetRegistry()
        {
            return new string[] { "pump1", "pump2" };
        }

        // GET api/pumps/registry/pump1
        [HttpGet("registry/{id}")]
        public IEnumerable<string> GetRegistry(string id)
        {
            return new string[] { "pump1", "pump2" };
        }

        // GET api/pumps/registry/pump1
        [HttpGet("execute/{id}")]
        public void Execute(string id)
        {
           Program.Manager.Execute(id);
        }
    }
}
