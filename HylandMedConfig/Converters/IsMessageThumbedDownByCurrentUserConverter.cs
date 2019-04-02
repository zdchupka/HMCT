using System;
using HylandMedConfig.Common;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Globalization;

namespace HylandMedConfig.Converters
{
    public class IsMessageThumbedDownByCurrentUserConverter : ConverterMarkupExtension<IsMessageThumbedDownByCurrentUserConverter>
    {
        public IsMessageThumbedDownByCurrentUserConverter()
        {

        }

		public override object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			if( values[0] == DependencyProperty.UnsetValue )
			{
				return false;
			}
			IList<ChatUser> users = values[0] as IList<ChatUser>;

			return users.Contains( ApplicationViewModel.Current.ChatProxy.CurrentUser );
		}
	}
}
