using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace HylandMedConfig.Converters
{
	public class TagBackgroundBrushConverter : ConverterMarkupExtension<TagBackgroundBrushConverter>
	{
		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string tag = null;
			List<string> tags = value as List<string>;
			if( tags != null )
			{
				if( tags.Count > 0 )
				{
					tag = tags[0];
				}
			}
			else
			{
				string tagString = System.Convert.ToString( value );
				if( !string.IsNullOrWhiteSpace( tagString ) )
				{
					tag = tagString;
				}
			}

			if( !string.IsNullOrWhiteSpace( tag ) )
			{
				byte[] hash = BitConverter.GetBytes( tag.GetHashCode() );
				Color color = Color.FromRgb( hash[0], hash[1], hash[2] );
				return new SolidColorBrush( color ) { Opacity = 0.6 };
			}
			return null;
		}
	}

	public class TagForegroundBrushConverter : ConverterMarkupExtension<TagForegroundBrushConverter>
	{
		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string tag = null;
			List<string> tags = value as List<string>;
			if( tags != null )
			{
				if( tags.Count > 0 )
				{
					tag = tags[0];
				}
			}
			else
			{
				string tagString = System.Convert.ToString( value );
				if( !string.IsNullOrWhiteSpace( tagString ) )
				{
					tag = tagString;
				}
			}

			if( !string.IsNullOrWhiteSpace( tag ) )
			{
				byte[] hash = BitConverter.GetBytes( tag.GetHashCode() );
				Color color = Color.FromRgb( hash[0], hash[1], hash[2] );
				return new SolidColorBrush( ContrastColor( color ) );
			}
			return null;
		}

		Color ContrastColor( Color color )
		{
			byte d = 0;

			// Counting the perceptive luminance - human eye favors green color... 
			double a = 1 - ( 0.299 * color.R + 0.587 * color.G + 0.114 * color.B ) / 255;

			if( a < 0.5 )
				d = 0; // bright colors - black font
			else
				d = 255; // dark colors - white font

			return Color.FromRgb( d, d, d );
		}
	}
}
