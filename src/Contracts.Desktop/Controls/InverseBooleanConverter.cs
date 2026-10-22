namespace Contracts.Desktop.Controls;

using System.Globalization;
using System.Windows.Data;

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool flag && !flag;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool flag && !flag;
}
