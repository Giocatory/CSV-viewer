using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace CsvViewer
{
    // Отображение разделителей в человекочитаемом виде
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
            => throw new NotImplementedException();
    }

    // Нумерация строк в DataGrid
    public class RowIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DataGrid dataGrid && values[1] != null)
            {
                int index = dataGrid.Items.IndexOf(values[1]);
                return (index + 1).ToString();
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}