using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;

namespace ETLService.Manager
{
    #region Настройки реестра

    public class ETLRegistrySettings
    {
        #region Свойства

        public DirectoryInfo RegistryPath { get; set; }

        public DirectoryInfo LibsPath { get; set; }

        public DirectoryInfo ProgramsPath { get; set; }

        public DirectoryInfo UpdatesPath { get; set; }

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
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public ETLRegistrySettings(string path)
        {
            RegistryPath = new DirectoryInfo(path);

            FileStream fs = null;
            StreamReader sr = null;

            try
            {
                string registry = Path.Combine(RegistryPath.FullName, "registry.json");
                fs = new FileStream(registry, FileMode.Open);
                sr = new StreamReader(fs);

                UpdateSettings(JObject.Parse(sr.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                sr?.Close();
                fs?.Close();
            }
        }

        #endregion Основные функции        
    }

    #endregion Настройки реестра

    #region Настройки

    public class ETLManagerSettings
    {
        #region Свойства

        public ETLRegistrySettings Registry { get; set; }

        public DirectoryInfo RegistryPath { get; set; }

        #endregion Свойства

        #region Основные функции

        public ETLManagerSettings(FileInfo settings)
        {
            FileStream fs = null;
            StreamReader sr = null;

            try
            {
                fs = new FileStream(settings.FullName, FileMode.Open);
                sr = new StreamReader(fs);
                JObject data = JObject.Parse(sr.ReadToEnd());

                string path = data["RegistryPath"].ToString();
                path = string.IsNullOrEmpty(path) || !Directory.Exists(path) 
                        ? Path.Combine(AppContext.BaseDirectory, "Registry")
                        : path;

                Registry = new ETLRegistrySettings(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                sr?.Close();
                fs?.Close();
            }
        }

        #endregion Основные функции
    }

    #endregion Настройки
}
