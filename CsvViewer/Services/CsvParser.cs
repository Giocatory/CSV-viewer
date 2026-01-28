using CsvViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CsvViewer.Services
{
    public static class CsvParser
    {
        /// <summary>
        /// Парсит CSV и возвращает заголовки + редактируемые строки
        /// </summary>
        public static (List<string> headers, List<EditableRow> rows) Parse(Stream stream, char delimiter = ',')
        {
            var parsed = ParseRaw(stream, delimiter);
            if (parsed.Count == 0)
                return (new List<string>(), new List<EditableRow>());

            var headers = parsed[0];
            var rows = new List<EditableRow>();

            for (int i = 1; i < parsed.Count; i++)
                rows.Add(new EditableRow(parsed[i]));

            return (headers, rows);
        }

        /// <summary>
        /// Низкоуровневый парсер (без создания моделей)
        /// </summary>
        public static List<List<string>> ParseRaw(Stream stream, char delimiter = ',')
        {
            var result = new List<List<string>>();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            char prevChar = '\0';

            while (reader.Peek() >= 0)
            {
                char c = (char)reader.Read();

                if (inQuotes)
                {
                    if (c == '"' && prevChar == '"') // Экранированная кавычка ""
                    {
                        currentField.Append('"');
                        prevChar = '\0';
                        continue;
                    }
                    else if (c == '"') // Закрывающая кавычка
                    {
                        inQuotes = false;
                        prevChar = c;
                        continue;
                    }
                    currentField.Append(c);
                }
                else
                {
                    if (c == '"') // Открывающая кавычка
                    {
                        inQuotes = true;
                    }
                    else if (c == delimiter) // Разделитель вне кавычек
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (c == '\r' || c == '\n') // Новая строка
                    {
                        // Обработка CRLF
                        if (c == '\r' && reader.Peek() == '\n')
                            reader.Read();

                        currentRow.Add(currentField.ToString());
                        currentField.Clear();

                        if (currentRow.Count > 0 || result.Count == 0)
                        {
                            result.Add(new List<string>(currentRow));
                        }
                        currentRow.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                prevChar = c;
            }

            // Добавляем последнее поле
            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
                if (currentRow.Count > 0)
                    result.Add(new List<string>(currentRow));
            }

            return result;
        }
    }
}