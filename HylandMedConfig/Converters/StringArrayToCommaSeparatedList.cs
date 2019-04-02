using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Converters
{
	public class StringArrayToCommaSeparatedList : ConverterMarkupExtension<StringArrayToCommaSeparatedList>
	{
		public StringArrayToCommaSeparatedList()
		{

		}
		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if(value == DependencyProperty.UnsetValue)
			{
				return string.Empty;
			}

			string[] values = (string[])value;
			return string.Join( ",", values );
		}
	}
}
