using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Converters
{
    class ReverseBooleanConverter : ConverterMarkupExtension<ReverseBooleanConverter>
    {
        public ReverseBooleanConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !System.Convert.ToBoolean(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !System.Convert.ToBoolean(value);
        }
    }
}
