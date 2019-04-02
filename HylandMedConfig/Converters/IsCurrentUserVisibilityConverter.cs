using System;
using System.Windows;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
    public class IsCurrentUserVisibilityConverter : ConverterMarkupExtension<IsCurrentUserVisibilityConverter>
    {
        public IsCurrentUserVisibilityConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isCurrentUser = System.Convert.ToBoolean(new IsCurrentUserConverter().Convert(value, targetType, parameter, culture));
            return isCurrentUser ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
