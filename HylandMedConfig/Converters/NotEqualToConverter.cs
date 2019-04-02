using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Converters
{
    class NotEqualToConverter : ConverterMarkupExtension<NotEqualToConverter>
    {
        public NotEqualToConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !value.ToString().Equals(parameter);
        }
    }
}
