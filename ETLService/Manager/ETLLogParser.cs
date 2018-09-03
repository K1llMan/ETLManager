using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ETLCommon;

namespace ETLService.Manager
{
    public class ETLLogRecord
    {
        public string Kind;
        public string Message;
        public string Module;
        public DateTime Date;

        public List<ETLLogRecord> Children;

        public ETLLogRecord(string message)
        {
            Children = new List<ETLLogRecord>();

            Module = message.GetMatches("^.+?(?=:)").FirstOrDefault().Trim();
            string date = message.GetMatches(@"([\d\.]+ [\d\:]+)").FirstOrDefault();
            Date = DateTime.ParseExact(date, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            Kind = message.GetMatches(@"(?<=[\d] )[\w]+?(?=: )").FirstOrDefault();

            foreach (string s in new string[] { Module + ":", date, Kind + ":" })
                message = message.Replace(s, string.Empty).Trim();

            Message = message;
        }
    }

    /// <summary>
    /// Класс преобразования текстового лога в структуру для отображения на клиенте
    /// </summary>
    public static class ETLLogParser
    {
        public static List<ETLLogRecord> Parse(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            FileStream fs = null;
            StreamReader sr = null;

            try {
                fs = new FileStream(fileName, FileMode.Open);
                sr = new StreamReader(fs, Encoding.UTF8);
                var data = sr.ReadToEnd()
                    .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    // Отрезается стек вызова в сообщениях об ошибках
                    .Where(s => !s.TrimStart().StartsWith("at"));

                return data.Select(s => new ETLLogRecord(s)).ToList();
            }
            catch (Exception ex) {
                throw new Exception($"Ошибка при разборе файла лога \"{fileName}\"", ex);
            }
            finally {
                sr?.Close();
                fs?.Close();
            }
        }
    }
}
