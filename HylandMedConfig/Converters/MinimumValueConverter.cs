using System;

namespace HylandMedConfig.Converters
{
	public class MinimumValueConverter : ConverterMarkupExtension<MinimumValueConverter>
	{
        public MinimumValueConverter()
        {

        }

		public override object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			double compareValue = System.Convert.ToDouble( parameter );
			double compareToValue = System.Convert.ToDouble( value );
			return Math.Min( compareValue, compareToValue );
		}
	}
}
