using System.Linq;

using Microsoft.AspNetCore.Mvc;

using ETLService.Manager;

namespace ETLService.Controllers
{
    [Route("api/[controller]")]
    public class PumpsController : Controller
    {
        // GET api/pumps/registry
        [HttpGet("registry")]
        public object GetRegistry()
        {
            return WebAPI.OK(Program.Manager.Pumps.Select(p => p.ConfigData));
        }

        // GET api/pumps/registry/pump1
        [HttpGet("registry/{id}")]
        public object GetRegistry(string id)
        {
            ETLProcess prc = Program.Manager.Pumps.FirstOrDefault(p => p.ProgramID == id);
            if (prc != null)
                return WebAPI.OK(prc.ConfigData);

            return WebAPI.Error("Отсутствует закачка с заданным идентификатором.");
        }

        // PUT api/pumps/registry
        [HttpPut("registry")]
        public object UpdateRegistry()
        {
            int count = Program.Manager.Updates.Count;
            return WebAPI.OK($"Применено {Program.Manager.ApplyUpdates()} из {count}");
        }

        // GET api/pumps/execute/pump1
        [HttpGet("execute/{id}")]
        public object Execute(string id)
        {
            int pumpID = Program.Manager.Execute(id);

            return pumpID == -1 
                ? WebAPI.Error($"Ошибка при запуске закачки \"{id}\".") 
                : WebAPI.OK(pumpID);
        }
    }
}
