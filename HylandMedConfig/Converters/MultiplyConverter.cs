using System;
using System.Globalization;

namespace HylandMedConfig.Converters
{
    public class MultiplyConverter : ConverterMarkupExtension<MultiplyConverter>
    {
        public MultiplyConverter()
        {

        }

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double)
                {
                    result *= (double)values[i];
                }
            }

            return result;
        }
    }
}
