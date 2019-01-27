using System;
using System.IO;
using System.Linq;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace ETLProgramCommon.DataAccess
{
    /// <summary>
    /// Класс для работы с архивами
    /// </summary>
    public static class Archivator
    {
        #region Основные функции

        /// <summary>
        /// Распаковка
        /// </summary>
        public static void Extract(FileInfo file, DirectoryInfo destDir)
        {
            if (!file.Exists)
                return;

            try
            {
                ExtractionOptions options = new ExtractionOptions {
                    ExtractFullPath = true,
                    Overwrite = true
                };

                // Данные извлекаются в директорию по названию файла
                string workDir = Path.Combine(destDir.FullName, Path.GetFileName(file.FullName));

                IArchive archive = ArchiveFactory.Open(file);
                if (!archive.IsSolid)
                    foreach (IArchiveEntry entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        entry.WriteToDirectory(workDir, options);
                else
                {
                    IReader reader = archive.ExtractAllEntries();
                    reader.WriteAllToDirectory(workDir, options);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при распаковке архива \"{file.FullName}\".", ex);
            }          
        }

        /// <summary>
        /// Распаковка данных из всех файлов в директории
        /// </summary>
        public static void ExtractAllFiles(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            if (!sourceDir.Exists)
                return;

            foreach (FileInfo file in sourceDir.GetFiles())
                Extract(file, destDir);
        }

        /// <summary>
        /// Распаковка всех данных, включая подкаталоги
        /// </summary>
        public static void ExtractAll(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            if (!sourceDir.Exists)
                return;

            ExtractAllFiles(sourceDir, destDir);

            foreach (DirectoryInfo dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
            {
                // Если директорию не существует, то создаём новую
                string path = dir.FullName.Replace(sourceDir.FullName, string.Empty).Trim(Path.DirectorySeparatorChar);
                DirectoryInfo workDir = new DirectoryInfo(Path.Combine(destDir.FullName, path));
                if (!workDir.Exists)
                    workDir.Create();

                ExtractAll(dir, workDir);
            }
        }

        #endregion Основные функции
    }
}
