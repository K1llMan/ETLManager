using ETLApp;
using ETLCommon;

using Newtonsoft.Json.Linq;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        public TestPump(ETLSettings settings, JObject data) : base(settings, data)
        {
        }
    }
}
