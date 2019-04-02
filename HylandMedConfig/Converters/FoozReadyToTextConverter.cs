using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HylandMedConfig.Converters
{
    public class FoozReadyToTextConverter : ConverterMarkupExtension<FoozReadyToTextConverter>
	{
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DependencyProperty.UnsetValue)
            {
                if (System.Convert.ToBoolean(value))
                {
					return "-1";
                }
                else
                {
                    return "+1";
                }
            }
			return "";
        }

        public FoozReadyToTextConverter()
        {

        }
	}

	public class FoozReadyToForegroundConverter : ConverterMarkupExtension<FoozReadyToForegroundConverter>
	{
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DependencyProperty.UnsetValue)
            {
                if (System.Convert.ToBoolean(value))
                {
					return Brushes.Red;
                }
                else
                {
                    return Brushes.Green;
                }
            }
			return "";
        }

        public FoozReadyToForegroundConverter()
        {

        }
	}
}
