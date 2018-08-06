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
            int count = Program.Manager.Updates.Count;
            return WebAPI.Success($"Применено {Program.Manager.ApplyUpdates()} из {count}");
        }

        // GET api/pumps/execute/pump1
        [HttpPost("execute/{id}")]
        public object Execute(string id)
        {
            try
            {
                StreamReader reader = new StreamReader(Request.Body);
                // Минимизация строки JSON
                string config = ((JObject)JsonConvert.DeserializeObject(reader.ReadToEnd()))
                    .ToString(Formatting.None);
                return WebAPI.Success(Program.Manager.Execute(id, config));
            }
            catch (Exception ex)
            {
                return WebAPI.Error($"Ошибка при запуске закачки \"{id}\": {ex.Message}");
            }
        }
    }
}
