using System;

using ETLApp;
using ETLCommon;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        public void PumpData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 1");
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
            Logger.WriteToTrace("Тестирование метода закачки 3");
        }
    }
}
