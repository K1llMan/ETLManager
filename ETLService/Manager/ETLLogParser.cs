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
        public DateTime Start;
        public DateTime End;

        public List<ETLLogRecord> Children;

        public ETLLogRecord(string message)
        {
            Children = new List<ETLLogRecord>();

            Module = message.GetMatches("^.+?(?=:)").FirstOrDefault().Trim();
            string date = message.GetMatches(@"([\d\.]+ [\d\:]+)").FirstOrDefault();
            Start = DateTime.ParseExact(date, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            Kind = message.GetMatches(@"(?<=[\d] )[\w]+?(?=: )").FirstOrDefault();

            List<string> replacements = new List<string> {
                Module + ":", date
            };

            if (!string.IsNullOrEmpty(Kind))
                replacements.Add(Kind + ":");

            foreach (string s in replacements)
                message = message.Replace(s, string.Empty).Trim();

            Message = message;
        }
    }

    /// <summary>
    /// Класс преобразования текстового лога в структуру для отображения на клиенте
    /// </summary>
    public static class ETLLogParser
    {
        private static void FormHierarchy(List<ETLLogRecord> log, object[][] rows, ref int curRow, int level)
        {
            ETLLogRecord parent = null;
            while (curRow < rows.Length) {
                int curLevel = (int)rows[curRow][0];
                ETLLogRecord record = (ETLLogRecord)rows[curRow][1];

                if (curLevel == level) {
                    log.Add(record);
                    curRow++;
                    parent = record;
                    continue;
                }

                if (curLevel > level) {
                    FormHierarchy(parent.Children, rows, ref curRow, curLevel);
                    // Время окончания родителя равно времени окончания последнего потомка
                    parent.End = parent.Children.Last().End;
                }

                if (curLevel < level)
                    return;
            }
        }

        public static List<ETLLogRecord> Parse(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            FileStream fs = null;
            StreamReader sr = null;

            try {
                fs = new FileStream(fileName, FileMode.Open);
                sr = new StreamReader(fs, Encoding.UTF8);
                object[][] data = sr.ReadToEnd()
                    .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    // Отрезается стек вызова в сообщениях об ошибках
                    .Where(s => !s.TrimStart().StartsWith("at"))
                    // По уровню отступов вычисляется уровень записи в иерархии
                    .Select(s => new object[]{ s.GetMatches(@"[\s]+").FirstOrDefault().Length / 4, new ETLLogRecord(s) })
                    .ToArray();

                // Заполнение поля окончания операции
                for (int i = 0; i < data.Length; i++) {
                    ETLLogRecord rec = (ETLLogRecord)data[i][1];
                    if (i == data.Length - 1) {
                        rec.End = rec.Start;
                        continue;
                    }

                    rec.End = ((ETLLogRecord)data[i + 1][1]).Start;
                }

                List<ETLLogRecord> records = new List<ETLLogRecord>();
                int curRow = 0;
                FormHierarchy(records, data, ref curRow, 0);

                return records;
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
