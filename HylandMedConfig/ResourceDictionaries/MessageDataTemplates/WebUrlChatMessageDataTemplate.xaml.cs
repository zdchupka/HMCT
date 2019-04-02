using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using HylandMedConfig.Common;
using HylandMedConfig.Controls;
using HylandMedConfig.Properties;
using HylandMedConfig.Attached;
using System.Windows.Documents;
using HylandMedConfig.Windows;
using HylandMedConfig.XamlControls;

namespace HylandMedConfig.ResourceDictionaries.MessageDataTemplates
{
	public partial class HTMLChatMessageDataTemplate : ResourceDictionary
	{
		private void webBrowser_LoadCompleted( object sender, System.Windows.Navigation.NavigationEventArgs e )
		{
			try
			{
				double height = (double)( ( sender as WebBrowser ).Document as dynamic ).body.scrollHeight;
				double width = (double)( ( sender as WebBrowser ).Document as dynamic ).body.scrollWidth;

				

				( sender as WebBrowser ).Height = height;
				( sender as WebBrowser ).Width = width;
				mshtml.IHTMLDocument2 dom = (mshtml.IHTMLDocument2)( sender as WebBrowser ).Document;
				dom.body.style.overflow = "hidden";
			}
			catch { }
		}
	}

	public partial class ASCIIChatMessageDataTemplate : ResourceDictionary
	{
	}

	public partial class PollMessageDataTemplate : ResourceDictionary
	{
		public PollMessageDataTemplate()
		{
		}

		private void PollMessageDataTemplate_Click( object sender, RoutedEventArgs e )
		{
			PollAttachedProperties.SetIsLocked( ( sender as Button ).TemplatedParent, true );
		}
	}

	public partial class PollClosedMessageDataTemplate : ResourceDictionary
	{
		public PollClosedMessageDataTemplate()
		{
		}
	}

	public partial class ImageUrlChatMessageDataTemplate : ResourceDictionary
	{
		void ImageUrlChatMessageDataTemplate_Click( object sender, RoutedEventArgs e )
		{
			//XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Image, new RepeatBehavior(1));
		}

		void ImageUrlChatMessageDataTemplate_SourceUpdated( object sender, System.Windows.Data.DataTransferEventArgs e )
		{
		}

		void ImageUrlChatMessageDataTemplate_Initialized( object sender, EventArgs e )
		{
			WpfAnimatedGif.ImageBehavior.AddAnimationLoadedHandler( sender as Image, ImageLoaded );
		}

		void ImageUrlChatMessageDataTemplate_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ImageUrlChatMessage urlMessage = ( sender as Image ).DataContext as ImageUrlChatMessage;
				if( urlMessage != null )
				{
					Process.Start( urlMessage.ImageUrl );
				}
			}
			catch( Exception ex )
			{
				MedConfigMessageBox.ShowError( ex.Message );
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private void ImageLoaded( object sender, RoutedEventArgs e )
		{
			ImageUrlChatMessage message = ( sender as Image ).DataContext as ImageUrlChatMessage;
			if( message != null )
			{
				message.Loaded = true;
			}
		}

		void Junk_Error( object sender, RoutedEventArgs e )
		{
			BitmapImage img = new BitmapImage( new Uri( "https://d8h50yxxu7ojg.cloudfront.net/images/icons/cross.png" ) );
			//(sender as Image).Source = img;
			DockPanel dc = ( sender as Image ).FindName( "loading" ) as DockPanel;
			dc.Visibility = Visibility.Collapsed;

			// Image errorImage = (sender as Image).FindName("errorImage") as Image;
			// errorImage.Source = new BitmapImage(new Uri("https://d8h50yxxu7ojg.cloudfront.net/images/icons/cross.png"));
			// errorImage.Visibility = Visibility.Visible;
		}

		void Junk_Loaded( object sender, RoutedEventArgs e )
		{
			DockPanel dc = ( sender as Image ).FindName( "loading" ) as DockPanel;
			dc.Visibility = Visibility.Collapsed;

			//Image errorImage = (sender as Image).FindName("errorImage") as Image;
			// errorImage.Visibility = Visibility.Collapsed;
		}
	}

	public partial class WebUrlChatMessageDataTemplate : ResourceDictionary
	{
		private void Button_Click( object sender, RoutedEventArgs e )
		{
			WebBrowser webBrowser = ( sender as Button ).FindName( "webBrowser" ) as WebBrowser;
			Border border = ( sender as Button ).FindName( "border" ) as Border;
			HyperlinkTextBlock hyperlink = ( sender as Button ).FindName( "hyperlink" ) as HyperlinkTextBlock;

			string url = ( webBrowser.DataContext as WebUrlChatMessage ).Url;
			url = Regex.Replace( url, @"^/web ", string.Empty );

			if( url.IndexOf( @"youtube.com/watch?v=" ) > 0 )
			{
				url = url.Replace( "/watch?v=", "/v/" );
				url += "&hl=en";
				webBrowser.Height = 240;
				webBrowser.Width = 320;
			}
			try
			{
				webBrowser.Navigate( url );
				webBrowser.Visibility = System.Windows.Visibility.Visible;
				hyperlink.Visibility = Visibility.Visible;
				border.Visibility = Visibility.Collapsed;
			}
			catch
			{
				webBrowser.Visibility = System.Windows.Visibility.Collapsed;
				border.Visibility = System.Windows.Visibility.Collapsed;
				hyperlink.Visibility = Visibility.Collapsed;
			}
		}

		private void webBrowser_LoadCompleted( object sender, System.Windows.Navigation.NavigationEventArgs e )
		{
			try
			{
				double height = (double)( ( sender as WebBrowser ).Document as dynamic ).body.scrollHeight;

				double maxHeight = 300d;

				if( height > maxHeight )
				{
					( sender as WebBrowser ).Height = maxHeight;
				}
				else
				{
					( sender as WebBrowser ).Height = height;
					mshtml.IHTMLDocument2 dom = (mshtml.IHTMLDocument2)( sender as WebBrowser ).Document;
					dom.body.style.overflow = "hidden";
				}
			}
			catch { }
		}

		private void webBrowser_Unloaded( object sender, RoutedEventArgs e )
		{
			( sender as WebBrowser ).Navigate( "about:blank" );
		}

	}

	public partial class LinkChatMessageDataTemplate : ResourceDictionary
	{
		void MouseButtonDown( object sender, MouseButtonEventArgs e )
		{
			if( e.LeftButton == MouseButtonState.Pressed )
			{
				string url = string.Empty;
				if( sender is FrameworkElement )
				{
					url = ( ( sender as FrameworkElement ).DataContext as HyperlinkChatMessage ).Url;
				}
				else if( sender is Run )
				{
					url = ( sender as Run ).Text;
				}
				try
				{
					Process.Start( url );
				}
				catch( Exception ex )
				{
					MedConfigMessageBox.ShowError( ex.Message );
				}
			}
		}

		private void CopyUrl_Click( object sender, RoutedEventArgs e )
		{
			Clipboard.SetText( ( (System.Windows.Documents.Run)( ( ( sender as MenuItem ).Parent as ContextMenu ).PlacementTarget as TextBlock ).Inlines.FirstInline ).Text );
		}
	}
}
