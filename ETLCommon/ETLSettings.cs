using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace ETLCommon
{
    #region Настройки реестра

    public class ETLRegistrySettings
    {
        #region Свойства

        public DirectoryInfo RegistryPath { get; set; }

        public DirectoryInfo LibsPath { get; set; }

        public DirectoryInfo LogsPath { get; set; }

        public DirectoryInfo ProgramsPath { get; set; }

        public DirectoryInfo UpdatesPath { get; set; }

        public DirectoryInfo InputPath { get; set; }

        public DirectoryInfo OutputPath { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private DirectoryInfo GetRelativePath(JToken path)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(RegistryPath.FullName, path.ToString()));
            if (!dir.Exists)
                dir.Create();

            return dir;
        }

        private void UpdateSettings(JObject data)
        {
            LibsPath = GetRelativePath(data["libs"]);
            LogsPath = GetRelativePath(data["logs"]);
            ProgramsPath = GetRelativePath(data["programs"]);
            UpdatesPath = GetRelativePath(data["updates"]);
            InputPath = GetRelativePath(data["in"]);
            OutputPath = GetRelativePath(data["out"]);
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public ETLRegistrySettings(string path)
        {
            RegistryPath = new DirectoryInfo(path);

            string registry = Path.Combine(RegistryPath.FullName, "registry.json");
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
        }

        #endregion Основные функции
    }

    #endregion Настройки
}
