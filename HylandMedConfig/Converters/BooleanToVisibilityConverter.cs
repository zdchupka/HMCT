using System;
using System.Windows;
using System.Windows.Data;

namespace HylandMedConfig.Converters
{
    public class BooleanToVisibilityConverter : ConverterMarkupExtension<BooleanToVisibilityConverter>
	{
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DependencyProperty.UnsetValue)
            {
                if (System.Convert.ToBoolean(value))
                {
                    if (parameter != null && parameter.Equals("NOT"))
                    {
                        return Visibility.Collapsed;
                    }
                    return Visibility.Visible;
                }
                else
                {
                    if (parameter != null && parameter.Equals("NOT"))
                    {
                        return Visibility.Visible;
                    }
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public BooleanToVisibilityConverter()
        {

        }
	}
}
