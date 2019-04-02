using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Converters
{
	public class TextBoxFontConverter : ConverterMarkupExtension<TextBoxFontConverter>
	{
		public TextBoxFontConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string text = System.Convert.ToString( value );
			List<string> tags;
			ChatUserMessage.GetWhisperUsers( text, ApplicationViewModel.Current.UsersView.SourceCollection.OfType<ChatUser>().ToList(), out text );
			ChatUserMessage.TryGetTags( text, out tags, out text );
			if( text.StartsWith( string.Format( "/{0} ", ChatUserMessage.Commands.ASCII ) ) )
			{
				return new FontFamily( "Consolas" );
			}
			return Settings.Default.FontFamily;
		}
	}
}
