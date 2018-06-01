using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json.Linq;

namespace ETLCommon
{
    public static class StringResources
    {
        private static Dictionary<string, Dictionary<string, string>> resources;

        /// <summary>
        /// Загружает строковые данные из ресурсной секции сборки
        /// </summary>
        private static void LoadFromXml(string fileName)
        {
            resources = new Dictionary<string, Dictionary<string, string>>();

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ETLCommon.Logger.StringResources.json");
            StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(1251));
            JObject data = JObject.Parse(sr.ReadToEnd());
            foreach (JProperty type in data.Children())
            {
                if (!resources.ContainsKey(type.Name))
                    resources[type.Name] = new Dictionary<string, string>();

                foreach (JProperty value in type.Value.Children())
                {
                    if (!resources[type.Name].ContainsKey(value.Name))
                        resources[type.Name][value.Name] = value.Value.ToString();
                }
            }
        }

        /// <summary>
        /// Получить значение строковой константы
        /// </summary>
        public static string GetLine<T>(T value)
       {
            if (resources == null)
                LoadFromXml("");

            string typeName = value.GetType().Name;
            if (!resources.ContainsKey(typeName))
                typeName = "Constants";

            if (!resources[typeName].ContainsKey(value.ToString()))
                return string.Empty;

            return resources[typeName][value.ToString()];
        }

    }
}
