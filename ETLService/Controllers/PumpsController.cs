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

        // GET api/pumps/registry/{id}
        [HttpGet("registry/{id}")]
        public object GetRegistry(string id)
        {
            ETLProcess prc = Program.Manager.Pumps.FirstOrDefault(p => p.ProgramID == id);
            if (prc != null)
                return WebAPI.Success(prc.ConfigData);

            return WebAPI.Error("Отсутствует закачка с заданным идентификатором.");
        }

        // PUT api/pumps/registry
        [HttpPut("registry")]
        public object UpdateRegistry()
        {
            int count = Program.Manager.UpdateManager.Updates.Count;
            return WebAPI.Success($"Применено {Program.Manager.ApplyUpdates()} из {count}");
        }

        // POST api/pumps/execute/pump1
        [HttpPost("execute/{id}")]
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

        // GET api/pumps/updates
        [HttpGet("updates")]
        public object GetUpdates()
        {
            return WebAPI.Success(Program.Manager.UpdateManager.Updates);
        }
    }
}
