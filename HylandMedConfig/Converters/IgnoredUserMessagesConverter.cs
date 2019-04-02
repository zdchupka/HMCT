using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;
using System.Collections.Specialized;

namespace HylandMedConfig.Converters
{
	class IgnoredUserMessagesConverter : ConverterMarkupExtension<IgnoredUserMessagesConverter>
	{
		public IgnoredUserMessagesConverter()
		{

		}

		public override object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			foreach( object value in values )
			{
				if( value == DependencyProperty.UnsetValue )
				{
					return string.Empty;
				}
			}

			ObservableCollection<ChatUserMessage> messages = ApplicationViewModel.Current.AllMessages;
			StringCollection ignoredUsers = Settings.Default.IgnoredUsers;
			ChatUser user = values[0] as ChatUser;

			if( ignoredUsers.Contains( user.Username ) )
			{
				long count = 0;

				foreach( ChatUserMessage message in messages.OfType<ChatUserMessage>().Where( m => m.FromUser == user ) )
				{
					count++;
				}
				if( count == 0 )
				{
					return "Ignoring...";
				}
				return string.Format( "Ignoring...  ( {0} )", count );
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
