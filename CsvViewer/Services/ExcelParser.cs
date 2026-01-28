using ClosedXML.Excel;
using CsvViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvViewer.Services
{
    public static class ExcelParser
    {
        /// <summary>
        /// Парсит XLSX и возвращает заголовки + редактируемые строки
        /// </summary>
        public static (List<string> headers, List<EditableRow> rows) Parse(Stream stream)
        {
            var parsed = ParseRaw(stream);
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
        public static List<List<string>> ParseRaw(Stream stream)
        {
            var result = new List<List<string>>();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var range = worksheet.RangeUsed();
            if (range == null)
                return result;

            var firstRow = range.FirstRow().RowNumber();
            var lastRow = range.LastRow().RowNumber();
            var firstCol = range.FirstColumn().ColumnNumber();
            var lastCol = range.LastColumn().ColumnNumber();

            for (int row = firstRow; row <= lastRow; row++)
            {
                var rowData = new List<string>();
                for (int col = firstCol; col <= lastCol; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    rowData.Add(cell.Value.ToString() ?? string.Empty);
                }
                result.Add(rowData);
            }

            return result;
        }

        /// <summary>
        /// Определяет, является ли поток XLSX файлом по сигнатуре
        /// </summary>
        public static bool IsXlsxFile(Stream stream)
        {
            long originalPosition = stream.Position;
            try
            {
                if (stream.Length < 4) return false;
                var buffer = new byte[4];
                int read = stream.Read(buffer, 0, 4);
                stream.Position = originalPosition;
                return read == 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
            }
            catch
            {
                stream.Position = originalPosition;
                return false;
            }
        }
    }
}