using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Converters
{
	public class NumberToUiThemeConverter : ConverterMarkupExtension<NumberToUiThemeConverter>
	{
		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if(value == DependencyProperty.UnsetValue)
			{
				return UITheme.None;
			}

			long num = System.Convert.ToInt64( value );
			return (UITheme)num;
		}

		public override object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value == DependencyProperty.UnsetValue )
			{
				return (long)UITheme.None;
			}

			return (long)(UITheme)Enum.Parse( typeof( UITheme ), System.Convert.ToString( value ) );
		}
	}
}
