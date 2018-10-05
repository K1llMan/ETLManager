using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ETLCommon
{
    #region Настройки реестра

    public class ETLRegistrySettings
    {
        #region Свойства

        public string RegistryPath { get; set; }

        public string ModulesPath { get; set; }

        public string LogsPath { get; set; }

        public string ProgramsPath { get; set; }

        public string UpdatesPath { get; set; }

        public string MigrationsPath { get; set; }

        public string InputPath { get; set; }

        public string OutputPath { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private string GetRelativePath(JToken path)
        {
            string dir = Path.Combine(RegistryPath, path.ToString());
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }

        private void UpdateSettings(JObject data)
        {
            ModulesPath = GetRelativePath(data["modules"]);
            LogsPath = GetRelativePath(data["logs"]);
            ProgramsPath = GetRelativePath(data["programs"]);
            UpdatesPath = GetRelativePath(data["updates"]);
            MigrationsPath = GetRelativePath(data["migrations"]);
            InputPath = GetRelativePath(data["in"]);
            OutputPath = GetRelativePath(data["out"]);
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public ETLRegistrySettings(string path)
        {
            RegistryPath = path;

            string registry = Path.Combine(RegistryPath, "registry.json");
            UpdateSettings(JsonCommon.Load(registry));
        }

        #endregion Основные функции        
    }

    #endregion Настройки реестра

    #region Настройки

    public class ETLSettings
    {
        #region Свойства

        public ETLRegistrySettings Registry { get; set; }

        public string JWTKey { get; }

        public string ConnectString { get; }

        public ETLProxy Proxy { get; }


        #endregion Свойства

        #region Основные функции

        public ETLSettings(string settings)
        {
            JObject data = JsonCommon.Load(settings);
            string path = data["RegistryPath"].ToString();
            path = string.IsNullOrEmpty(path) || !Directory.Exists(path)
                ? Path.Combine(AppContext.BaseDirectory, "Registry")
                : path;

            Registry = new ETLRegistrySettings(path);
            JWTKey = data["JWTKey"].ToString();
            ConnectString = data["Database"].ToString();
            Proxy = JsonConvert.DeserializeObject<ETLProxy>(data["Proxy"].ToString());
        }

        #endregion Основные функции
    }

    #endregion Настройки
}
