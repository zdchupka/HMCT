using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Converters
{
	public class PollChoiceTextConverter : ConverterMarkupExtension<PollChoiceTextConverter>
	{
		public PollChoiceTextConverter()
		{

		}

		public override object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			if( values[0] == DependencyProperty.UnsetValue ||
				values[1] == DependencyProperty.UnsetValue )
			{
				return 0d;
			}

			double votes = System.Convert.ToDouble( values[0] );
			double totalVotes = System.Convert.ToDouble( values[1] );

			return string.Format( "({0})", votes );
		}

	}

	public class PollPercentageConverter : ConverterMarkupExtension<PollPercentageConverter>
	{
		public PollPercentageConverter()
		{

		}

		public override object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			if( values[0] == DependencyProperty.UnsetValue ||
				values[1] == DependencyProperty.UnsetValue )
			{
				return 0d;
			}

			double votes = System.Convert.ToDouble( values[0] );
			double totalVotes = System.Convert.ToDouble( values[1] );

			if( totalVotes > 0 )
			{

				return votes * 100d / totalVotes;
			}
			else
			{
				return 0d;
			}
		}
	}
}
