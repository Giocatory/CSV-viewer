using CsvViewer.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CsvViewer.Converters
{
    /// <summary>
    /// Конвертер для визуальной индикации изменённых ячеек
    /// </summary>
    public class CellStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EditableCell cell && cell.IsModified)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 255, 204)); // Светло-жёлтый фон
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}