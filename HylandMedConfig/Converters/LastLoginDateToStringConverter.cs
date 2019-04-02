using System;

namespace HylandMedConfig.Converters
{
	public class LastLoginDateToStringConverter : ConverterMarkupExtension<LastLoginDateToStringConverter>
	{
		public LastLoginDateToStringConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			DateTime? lastLogin = value as DateTime?;
			if( lastLogin.HasValue )
			{
				DateTime now = DateTime.Now;
				int days = (int)( now.Date - lastLogin.Value.Date ).TotalDays;

				if( days == 0 )
				{
					return string.Format( "Today at {0:t}", lastLogin.Value );
				}

				if( days == 1 )
				{
					return string.Format( "Yesterday at {0:t}", lastLogin.Value );
				}

				return string.Format( "{0} days ago", days );
			}
			else
			{
				return "Never";
			}
		}
	}
}
