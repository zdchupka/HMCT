using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Converters
{
	class BackgroundImageMarginConverter : ConverterMarkupExtension<BackgroundImageMarginConverter>
	{
		public BackgroundImageMarginConverter()
		{

		}
		public override object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			double x = System.Convert.ToDouble( values[0] );
			double y = System.Convert.ToDouble( values[1] );

			return new Thickness( x, y, 0, 0 );
		}
	}

	public class ShouldHideTagConverter : ConverterMarkupExtension<ShouldHideTagConverter>
	{
		public ShouldHideTagConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			ChatUserMessage message = value as ChatUserMessage;

			if( message != null && message.FromUser != ApplicationViewModel.Current.ChatProxy.CurrentUser && message.Tags != null && message.Tags.Count > 0 )
			{
				foreach( string hiddenTag in Settings.Default.HiddenTags )
				{
					if( message.Tags.Contains( hiddenTag ) )
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public class TagsToUnsafeTagsConverter : ConverterMarkupExtension<TagsToUnsafeTagsConverter>
	{
		public TagsToUnsafeTagsConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			ChatUserMessage message = value as ChatUserMessage;
			if( message != null )
			{
				StringBuilder sb = new StringBuilder();
				foreach( string tag in ApplicationViewModel.Current.ChatProxy.TagService.GetHiddenTags( message.Tags ) )
				{
					sb.AppendFormat( "#{0} ", tag );
				}
				return sb.ToString().TrimEnd();
			}
			return "";
		}
	}
}
