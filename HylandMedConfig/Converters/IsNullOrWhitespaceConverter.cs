using System;
using System.Collections.Generic;
using System.Windows;

namespace HylandMedConfig.Converters
{
    class IsNullOrWhitespaceConverter : ConverterMarkupExtension<IsNullOrWhitespaceConverter>
    {
        public IsNullOrWhitespaceConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(System.Convert.ToString(value));
        }
    }

	class ListHasItemsConverter : ConverterMarkupExtension<ListHasItemsConverter>
    {
        public ListHasItemsConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			List<string> items = value as List<string>;
			return items != null && items.Count > 0;
        }
    }
}
