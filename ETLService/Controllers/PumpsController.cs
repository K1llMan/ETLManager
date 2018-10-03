using System;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using ETLService.Manager;

using Newtonsoft.Json;
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
            return WebAPI.Success(Program.Manager.Pumps.Select(p => p.ConfigData));
        }

        // GET api/pumps/{id}/registry
        [HttpGet("{id}/registry")]
        public object GetRegistry(string id)
        {
            ETLProcess prc = Program.Manager.Pumps.FirstOrDefault(p => p.ProgramID == id);
            if (prc != null)
                return WebAPI.Success(prc.ConfigData);

            return WebAPI.Error("Отсутствует закачка с заданным идентификатором.");
        }

        // POST api/pumps/pump1/execute
        [HttpPost("{id}/execute")]
        public object Execute(string id, [FromBody]JObject data)
        {
            try
            {
                // Минимизация строки JSON
                string config = data.ToString(Formatting.None);
                return WebAPI.Success(Program.Manager.Execute(id, config));
            }
            catch (Exception ex)
            {
                return WebAPI.Error($"Ошибка при запуске закачки \"{id}\": {ex.Message}");
            }
        }

        // POST api/pumps/pump1/terminate
        [HttpGet("{id}/terminate")]
        public void Terminate(string id)
        {
            try
            {
                Program.Manager.Terminate(id);
            }
            catch (Exception ex)
            {
            }
        }

        // GET api/pumps/log
        [HttpGet("log/{sessNo}")]
        public object GetLog(decimal sessNo)
        {
            return WebAPI.Success(Program.Manager.GetLog(sessNo));
        }

        // GET api/pumps/statuses
        [HttpGet("statuses")]
        public object GetStatuses()
        {
            return WebAPI.Success(Program.Manager.Pumps.ToDictionary(
                p => p.ProgramID, 
                p => p.IsExecuting 
                    ? "Running" 
                    :  p.LastStatus.ToString()));
        }
    }
}
