using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Converters
{
    public class IsGreaterThanConverter : ConverterMarkupExtension<IsGreaterThanConverter>
    {
        public IsGreaterThanConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double doubleValue = System.Convert.ToDouble(value);
            double compareValue = System.Convert.ToDouble(parameter);
            return doubleValue > compareValue;
        }
    }
}
