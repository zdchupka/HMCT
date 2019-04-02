using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace HylandMedConfig.Attached
{
	public static class BrowserBehavior
	{
		public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
		   "Html",
		   typeof( string ),
		   typeof( BrowserBehavior ),
		   new FrameworkPropertyMetadata( OnHtmlChanged ) );

		[AttachedPropertyBrowsableForType( typeof( WebBrowser ) )]
		public static string GetHtml( WebBrowser d )
		{
			return (string)d.GetValue( HtmlProperty );
		}

		public static void SetHtml( WebBrowser d, string value )
		{
			d.SetValue( HtmlProperty, value );
		}

		static void OnHtmlChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			WebBrowser wb = d as WebBrowser;
			if( !string.IsNullOrWhiteSpace( System.Convert.ToString( e.NewValue ) ) )
			{
				if( wb != null )
				{
					try
					{
						string html = @"<script>function window.alert(){ return false; } function window.confirm(){ return false; } function window.prompt(){ return false; }</script>" + e.NewValue.ToString();
						wb.NavigateToString( html );

						dynamic activeX = wb.GetType().InvokeMember(
							"ActiveXInstance",
							BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
							null,
							wb,
							new object[] { } );

						activeX.Silent = true;
					}
					catch { }
				}
			}
		}

		public static string GetYouTubeLink( DependencyObject obj )
		{
			return (string)obj.GetValue( YouTubeLinkProperty );
		}

		public static void SetYouTubeLink( DependencyObject obj, string value )
		{
			obj.SetValue( YouTubeLinkProperty, value );
		}

		// Using a DependencyProperty as the backing store for YouTubeLink.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty YouTubeLinkProperty =
			DependencyProperty.RegisterAttached( "YouTubeLink", typeof( string ), typeof( BrowserBehavior ), new FrameworkPropertyMetadata( OnYouTubeLinkChanged ) );

		static void OnYouTubeLinkChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			WebBrowser wb = d as WebBrowser;
			if( !string.IsNullOrWhiteSpace( System.Convert.ToString( e.NewValue ) ) )
			{
				if( wb != null )
				{
					try
					{
						string html = YouTubeHelper.Display( e.NewValue.ToString() );
						wb.NavigateToString( html );
					}
					catch { }
				}
			}
		}
	}

	public class YouTubeHelper
	{
		private static Regex YouTubeURLIDRegex = new Regex( @"[\?&]v=(?<v>[^&]+)" );
		private static string Html = @"<html>
<head>
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" /> 
</head>
<body style=""margin: 0px;"">
<div style=""position:relative;height:0;padding-bottom:75.0%"">
<iframe src=""https://www.youtube.com/embed/{0}?ecver=2"" width=""480"" height=""360"" frameborder=""0"" allow=""encrypted-media"" style=""position:absolute;width:100%;height:100%;left:0"" allowfullscreen></iframe></div></body></html>";

		public static string Display( string url )
		{
			// Sample url: http://www.youtube.com/watch?v=p7aLl2ymkaE&playnext_from=TL&videos=afsfl56bccg&feature=sub
			Match m = YouTubeURLIDRegex.Match( url );
			String id = m.Groups["v"].Value;

			return string.Format( Html, id );
		}
	}
}
