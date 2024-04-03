using System.Globalization;
using System.Windows.Data;

namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public class InvertedBooleanConverter : IValueConverter
{

    public InvertedBooleanConverter()
    {

    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolVal = (bool)value;
        return !boolVal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
