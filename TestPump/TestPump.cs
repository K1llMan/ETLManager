using System;
using System.Threading;

using ETLApp;
using ETLCommon;

using ETLProgramCommon;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        public void PumpData()
        {
            Thread.Sleep(1000);
            Logger.WriteToTrace("Тестирование метода закачки 1");
            Logger.WriteToTrace(RootInDir.ToString());

            Entity ent = new Entity(Context.DB["etl_params"]);
            ent.Select();

            if (Convert.ToBoolean(UserParams["deleteData"]))
                Logger.WriteToTrace(UserParams["deleteData"].ToString());
        }

        public void ProcessData()
        {
            Thread.Sleep(3000);
            Logger.WriteToTrace("Тестирование метода закачки 2");
            throw new Exception("Ошибка насяльника!");
        }

        public void CloneData()
        {
            Thread.Sleep(2000);
            Logger.WriteToTrace("Тестирование метода закачки 3", TraceMessageKind.Warning);
            Logger.WriteToTrace(RootOutDir.ToString());
        }
    }
}
