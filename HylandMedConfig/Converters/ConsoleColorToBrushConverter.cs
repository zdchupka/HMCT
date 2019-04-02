using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Converters
{
	public class ConsoleColorToBrushConverter : ConverterMarkupExtension<ConsoleColorToBrushConverter>
	{
		public ConsoleColorToBrushConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if(value == DependencyProperty.UnsetValue)
			{
				return Brushes.Purple;
			}

			switch((ConsoleColor)value)
			{
				case ConsoleColor.Black:
					return Brushes.Black;
				default:
					return Brushes.Purple;
			}
		}
	}
}
