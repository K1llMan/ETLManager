using ETLApp;

using ETLCommon;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        public void PumpData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 1");
        }

        public void ProcessData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 2");
        }

        public void CloneData()
        {
            Logger.WriteToTrace("Тестирование метода закачки 3");
        }
    }
}
