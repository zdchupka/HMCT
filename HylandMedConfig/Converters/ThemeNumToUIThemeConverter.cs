using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Converters
{
    public class ThemeNumToUIThemeConverter : ConverterMarkupExtension<ThemeNumToUIThemeConverter>
    {
        public ThemeNumToUIThemeConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int num = System.Convert.ToInt32(value);
            return (UITheme)num;
        }
    }
}
