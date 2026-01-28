using CsvViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CsvViewer.Services
{
    public static class FileExporter
    {
        /// <summary>
        /// Экспорт в JSON
        /// </summary>
        public static void ExportToJson(CsvModel model, string filePath)
        {
            var jsonData = new List<Dictionary<string, string>>();

            foreach (var row in model.Rows)
            {
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < model.Headers.Count && i < row.Cells.Count; i++)
                {
                    dict[model.Headers[i]] = row[i];
                }
                jsonData.Add(dict);
            }

            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            File.WriteAllText(filePath, JsonSerializer.Serialize(jsonData, options), Encoding.UTF8);
        }

        /// <summary>
        /// Экспорт в CSV с сохранением изменений
        /// </summary>
        public static void ExportToCsv(CsvModel model, string filePath, char delimiter = ',')
        {
            var sb = new StringBuilder();

            // Заголовки
            sb.AppendLine(EscapeCsvLine(model.Headers, delimiter));

            // Данные
            foreach (var row in model.Rows)
            {
                var values = new List<string>();
                for (int i = 0; i < model.Headers.Count; i++)
                {
                    values.Add(i < row.Cells.Count ? row[i] : string.Empty);
                }
                sb.AppendLine(EscapeCsvLine(values, delimiter));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Экспорт в XLSX с сохранением изменений (требует ClosedXML)
        /// </summary>
        public static void ExportToXlsx(CsvModel model, string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Заголовки
            for (int i = 0; i < model.Headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = model.Headers[i];
            }

            // Данные
            for (int rowIndex = 0; rowIndex < model.Rows.Count; rowIndex++)
            {
                var row = model.Rows[rowIndex];
                for (int colIndex = 0; colIndex < model.Headers.Count; colIndex++)
                {
                    worksheet.Cell(rowIndex + 2, colIndex + 1).Value =
                        colIndex < row.Cells.Count ? row[colIndex] : string.Empty;
                }
            }

            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// Экранирование строки для CSV
        /// </summary>
        private static string EscapeCsvField(string value, char delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Экранируем если есть разделитель, кавычки или переносы строк
            if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }

        private static string EscapeCsvLine(IEnumerable<string> values, char delimiter)
        {
            return string.Join(delimiter.ToString(), values.Select(v => EscapeCsvField(v, delimiter)));
        }
    }
}