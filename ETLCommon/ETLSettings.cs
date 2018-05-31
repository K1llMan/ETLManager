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

        public DirectoryInfo ProgramsPath { get; set; }

        public DirectoryInfo UpdatesPath { get; set; }

        public DirectoryInfo InputPath { get; set; }

        public DirectoryInfo OutputPath { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private DirectoryInfo GetRelativePath(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(RegistryPath.FullName, path));
            if (!dir.Exists)
                dir.Create();

            return dir;
        }

        private void UpdateSettings(JObject data)
        {
            LibsPath = GetRelativePath(data["Libs"].ToString());
            ProgramsPath = GetRelativePath(data["Programs"].ToString());
            UpdatesPath = GetRelativePath(data["Updates"].ToString());
            InputPath = GetRelativePath(data["Input"].ToString());
            OutputPath = GetRelativePath(data["Output"].ToString());
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

        public DirectoryInfo RegistryPath { get; set; }

        #endregion Свойства

        #region Основные функции

        public ETLSettings(string settings)
        {
            Logger.WriteToTrace("Инициализация настроек.");

            JObject data = JsonCommon.Load(settings);
            string path = data["RegistryPath"].ToString();
            path = string.IsNullOrEmpty(path) || !Directory.Exists(path)
                ? Path.Combine(AppContext.BaseDirectory, "Registry")
                : path;

            RegistryPath = new DirectoryInfo(path);
            Registry = new ETLRegistrySettings(path);
        }

        #endregion Основные функции
    }

    #endregion Настройки
}
