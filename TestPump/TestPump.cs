using System;

using ETLApp;
using ETLCommon;

using ETLProgramCommon;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        public void PumpData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 1");

            Entity ent = new Entity(Context.DB["etl_params"]);
            ent.Select();

            if (Convert.ToBoolean(UserParams["deleteData"]))
                Logger.WriteToTrace(UserParams["deleteData"].ToString());
        }

        public void ProcessData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 2");
            throw new Exception("Ошибка насяльника!");
        }

        public void CloneData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 3", TraceMessageKind.Warning);
        }
    }
}
