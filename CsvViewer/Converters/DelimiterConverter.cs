using System;
using System.Globalization;
using System.Windows.Data;

namespace CsvViewer.Converters
{
    public class DelimiterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                ',' => "Запятая (,)",
                ';' => "Точка с запятой (;)",
                '\t' => "Табуляция (TAB)",
                '|' => "Вертикальная черта (|)",
                _ => value?.ToString() ?? "Неизвестно"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "Запятая (,)" => ',',
                "Точка с запятой (;)" => ';',
                "Табуляция (TAB)" => '\t',
                "Вертикальная черта (|)" => '|',
                _ => ','
            };
        }
    }
}