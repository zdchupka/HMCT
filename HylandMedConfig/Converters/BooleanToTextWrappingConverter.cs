using System;
using System.Windows;
using System.Windows.Data;

namespace HylandMedConfig.Converters
{
    public class BooleanToTextWrappingConverter : ConverterMarkupExtension<BooleanToTextWrappingConverter>
	{
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DependencyProperty.UnsetValue)
            {
                if (System.Convert.ToBoolean(value))
                {
                    return TextWrapping.Wrap;
                }
            }
            return TextWrapping.NoWrap;
        }

        public BooleanToTextWrappingConverter()
        {

        }
	}
}
