using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CsvViewer.Converters
{
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
        {
            throw new NotImplementedException();
        }
    }
}