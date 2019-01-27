using System.Collections.Generic;
using System.Net;

using ETLProgramCommon.DataAccess;

namespace TestPump
{
    public class TestWebService: WebService
    {
        public string GetData()
        {
            Dictionary<string, string> urlParams = new Dictionary<string, string> {
                { "param1", "true" },
                { "param2", "M" },
                { "year", "2018" },
            };

            return GetResponse("yearsData", parameters: urlParams);
        }

        public TestWebService(string url, IWebProxy webProxy = null) : base(url, webProxy)
        {
            
        }
    }
}
