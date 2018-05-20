using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ETLCommon
{
    /// <summary>
    /// Предоставляет набор методов расширения для работы с JSON
    /// </summary>
    public static class JsonCommon
    {
        public static JObject Load(string fileName)
        {
            FileStream fs = null;
            StreamReader sr = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open);
                sr = new StreamReader(fs);
                return JObject.Parse(sr.ReadToEnd());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                sr?.Close();
                fs?.Close();
            }
        }
    }
}
