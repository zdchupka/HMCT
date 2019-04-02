using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
    class FoozStatusMessageConverter : ConverterMarkupExtension<FoozStatusMessageConverter>
    {
        public FoozStatusMessageConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int userCount = System.Convert.ToInt32(value);

            switch (userCount)
            {
                case 1:
                    return "3 More...";
                case 2:
                    return "2 More...";
                case 3:
                    return "Just 1 More...";
                case 4:
                    return "Game Ready";
            }
            return string.Empty;
        }
    }
}
