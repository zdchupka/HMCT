using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using HylandMedConfig.Windows;

namespace HylandMedConfig.Controls
{
	public class EmoticonRichTextBox : TextBlock
	{
		#region Static
		private static string[] _imageExtensions = { ".tif", ".tiff", ".gif", ".jpeg", ".jpg", ".png", ".bmp" };

		public string RawText
		{
			get { return (string)GetValue( RawTextProperty ); }
			set { SetValue( RawTextProperty, value ); }
		}



		public static bool GetIsBigEmoji( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsBigEmojiProperty );
		}

		public static void SetIsBigEmoji( DependencyObject obj, bool value )
		{
			obj.SetValue( IsBigEmojiProperty, value );
		}

		// Using a DependencyProperty as the backing store for IsBigEmoji.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsBigEmojiProperty =
			DependencyProperty.RegisterAttached( "IsBigEmoji", typeof( bool ), typeof( EmoticonRichTextBox ), new PropertyMetadata( false ) );



		// Using a DependencyProperty as the backing store for RawText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RawTextProperty =
			DependencyProperty.Register( "RawText", typeof( string ), typeof( EmoticonRichTextBox ), new FrameworkPropertyMetadata( ( s, a ) =>
					  {
						  EmoticonRichTextBox thisControl = s as EmoticonRichTextBox;
						  string newText = thisControl.RawText;
						  thisControl.Text = newText;
						  int numFound;
						  ProcessInlines( thisControl.Inlines, thisControl.ImageSize, out numFound );

						  List<InlineUIContainer> containers = thisControl.Inlines.OfType<InlineUIContainer>().ToList();

						  int emptyInlines = thisControl.Inlines.OfType<Run>().Where( r => string.IsNullOrEmpty( r.Text.Trim( new char[] { '\r', '\n' } ) ) ).Count();

						  // When there is regular text involved, resize the emoticons to be inline
						  if( containers.Count < thisControl.Inlines.Count - emptyInlines )
						  {
							  foreach( InlineUIContainer container in containers )
							  {
								  Image image = ( container.Child as Grid ).Children[0] as Image;
								  if( image != null )
								  {
									  BitmapSource bitmapSource = WpfAnimatedGif.ImageBehavior.GetAnimatedSource( image ) as BitmapSource;
									  //Image image = new Image();
									  //WpfAnimatedGif.ImageBehavior.SetAnimatedSource( image, WpfAnimatedGif.ImageBehavior.GetAnimatedSource( image2 ) );

									  //Size size = new Size( 24, 24 );
									  //BitmapSource bitmapSource = image.Source as BitmapSource;
									  if( bitmapSource != null )
									  {
										  if( bitmapSource.PixelHeight < 24 )
										  {
											  image.Stretch = Stretch.Fill;
											  Binding heightBinding = new Binding( "Source.PixelHeight" ) { RelativeSource = RelativeSource.Self };
											  Binding widthBinding = new Binding( "Source.PixelWidth" ) { RelativeSource = RelativeSource.Self };

											  image.SetBinding( Image.HeightProperty, heightBinding );
											  image.SetBinding( Image.WidthProperty, widthBinding );
										  }
										  else
										  {
											  RenderOptions.SetBitmapScalingMode( image, BitmapScalingMode.HighQuality );
											  image.Height = 24;
											  image.Width = double.NaN;
											  image.Stretch = Stretch.Uniform;
										  }
									  }
								  }
								  //image.MaxHeight = size.Height;
								  ////emoticon.MaxWidth = size.Width;
								  //image.Stretch = Stretch.Uniform;

								  //container.Child = null;
								  //Grid grid = new Grid();
								  //grid.Children.Add( image );

								  //container.Child = grid;
							  }
						  }
					  } ) );

		public Size ImageSize
		{
			get { return (Size)GetValue( ImageSizeProperty ); }
			set { SetValue( ImageSizeProperty, value ); }
		}

		public static readonly DependencyProperty ImageSizeProperty =
			DependencyProperty.Register( "ImageSize", typeof( Size ), typeof( EmoticonRichTextBox ), new PropertyMetadata( Size.Empty ) );

		private static int GetNextEmoticon( string text, int startIndex, out string emoticonKey )
		{
			emoticonKey = string.Empty;
			int nextIndex = -1;
			foreach( string[] keys in EmoticonLibrary.Current.Select( m => m.Keys ) )
			{
				foreach( string key in keys )
				{
					int index = text.IndexOf( key, startIndex );
					if( index >= 0 )
					{
						if( nextIndex < 0 || index < nextIndex )
						{
							nextIndex = index;
							emoticonKey = key;
						}
					}
				}
			}
			return nextIndex;
		}

		private static int GetNextUser( string text, int startIndex, out string emoticonKey, out ChatUser user )
		{
			emoticonKey = string.Empty;
			user = null;
			int nextIndex = -1;
			foreach( ChatUser chatuser in ApplicationViewModel.Current.ChatProxy.Users )
			{
				string key = $"({chatuser.Username})";
				int index = text.IndexOf( key, startIndex );
				if( index >= 0 )
				{
					if( nextIndex < 0 || index < nextIndex )
					{
						nextIndex = index;
						emoticonKey = key;
						user = chatuser;
					}
				}
			}
			return nextIndex;
		}

		public static bool GetIsBigEmoji( string text )
		{
			string emoticonFound = string.Empty;
			int index = GetNextEmoticon( text, 0, out emoticonFound );
			while( index >= 0 )
			{
				text = text.Replace( emoticonFound, string.Empty );
				index = GetNextEmoticon( text, 0, out emoticonFound );
			}

			return string.IsNullOrEmpty( text.Trim() );
		}

		private static void ProcessInlines( InlineCollection inlines, Size size, out int numFound )
		{
			numFound = 0;

			try
			{
				for( int inlineIndex = 0; inlineIndex < inlines.Count; inlineIndex++ )
				{
					Inline i = inlines.ElementAt( inlineIndex );
					if( i is Run )
					{
						Run r = i as Run;
						string text = r.Text;

						foreach( Match m in Regex.Matches( text, HyperlinkUtility.UrlRegex2 ) )
						{
							TextPointer tp = i.ContentStart;
							while( !tp.GetTextInRun( LogicalDirection.Forward ).StartsWith( m.Value ) )
							{
								tp = tp.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextPointer end = tp;
							for( int j = 0; j < m.Value.Length; j++ )
							{
								end = end.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextRange tr = new TextRange( tp, end );
							tr.Text = string.Empty;
							UIElement element = CreateHyperlink( m.Value );
							InlineUIContainer iContainer = new InlineUIContainer( element, tp );
							iContainer.BaselineAlignment = BaselineAlignment.Bottom;
							continue;
						}

						foreach( Match m in Regex.Matches( text, HyperlinkUtility.UncRegex ) )
						{
							TextPointer tp = i.ContentStart;
							while( !tp.GetTextInRun( LogicalDirection.Forward ).StartsWith( m.Value ) )
							{
								tp = tp.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextPointer end = tp;
							for( int j = 0; j < m.Value.Length; j++ )
							{
								end = end.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextRange tr = new TextRange( tp, end );
							tr.Text = string.Empty;
							UIElement element = CreateHyperlink( m.Value );
							InlineUIContainer iContainer = new InlineUIContainer( element, tp );
							iContainer.BaselineAlignment = BaselineAlignment.Bottom;
							continue;
						}

						text = r.Text;
						foreach( Match m in Regex.Matches( text, HyperlinkUtility.UrlRegex ) )
						{
							TextPointer tp = i.ContentStart;
							while( !tp.GetTextInRun( LogicalDirection.Forward ).StartsWith( m.Value ) )
							{
								tp = tp.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextPointer end = tp;
							for( int j = 0; j < m.Value.Length; j++ )
							{
								end = end.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextRange tr = new TextRange( tp, end );
							tr.Text = string.Empty;
							UIElement element = CreateHyperlink( m.Value );
							InlineUIContainer iContainer = new InlineUIContainer( element, tp );
							iContainer.BaselineAlignment = BaselineAlignment.Bottom;
							continue;
						}



						string emoticonFound = string.Empty;
						int index = GetNextEmoticon( text, 0, out emoticonFound );
						if( index >= 0 )
						{
							TextPointer tp = i.ContentStart;
							while( !tp.GetTextInRun( LogicalDirection.Forward ).StartsWith( emoticonFound ) )
							{
								tp = tp.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextPointer end = tp;
							for( int j = 0; j < emoticonFound.Length; j++ )
							{
								end = end.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextRange tr = new TextRange( tp, end );
							tr.Text = string.Empty;


							//string uri = ApplicationViewModel.Current.SmiliesView.OfType<EmoticonMapping>().Where( d => d.KeyList.Contains( emoticonFound ) ).FirstOrDefault().Path;
							ImageSource imageSource = EmoticonLibrary.Current.Where( e => e.Keys.Contains( emoticonFound ) ).Select( e => e.ImageSource ).FirstOrDefault();

							System.Windows.Controls.Image emoticon = new System.Windows.Controls.Image();
							emoticon.Style = Application.Current.Resources["AnimatedImageStyle"] as Style;

							if( size == Size.Empty )
							{
								emoticon.Stretch = Stretch.Fill;
								Binding heightBinding = new Binding( "Source.PixelHeight" ) { RelativeSource = RelativeSource.Self };
								Binding widthBinding = new Binding( "Source.PixelWidth" ) { RelativeSource = RelativeSource.Self };

								emoticon.SetBinding( Image.HeightProperty, heightBinding );
								emoticon.SetBinding( Image.WidthProperty, widthBinding );
							}
							else
							{
								BitmapSource bitmapSource = imageSource as BitmapSource;
								if( bitmapSource != null )
								{
									if( bitmapSource.PixelHeight < size.Height )
									{
										emoticon.Stretch = Stretch.Fill;
										Binding heightBinding = new Binding( "Source.PixelHeight" ) { RelativeSource = RelativeSource.Self };
										Binding widthBinding = new Binding( "Source.PixelWidth" ) { RelativeSource = RelativeSource.Self };

										emoticon.SetBinding( Image.HeightProperty, heightBinding );
										emoticon.SetBinding( Image.WidthProperty, widthBinding );
									}
									else
									{
										RenderOptions.SetBitmapScalingMode( emoticon, BitmapScalingMode.HighQuality );
										emoticon.MaxHeight = size.Height;
										//emoticon.MaxWidth = size.Width;
										emoticon.Stretch = Stretch.Uniform;
									}
								}
								emoticon.MaxHeight = size.Height;
								//emoticon.MaxWidth = size.Width;
								emoticon.Stretch = Stretch.Uniform;
							}

							//ImageSource imageSource = new ImageSourceConverter().ConvertFromString( uri ) as ImageSource;

							WpfAnimatedGif.ImageBehavior.SetAnimatedSource( emoticon, imageSource );

							numFound++;
							Grid grid = new Grid();
							grid.ToolTip = emoticonFound;
							grid.Children.Add( emoticon );
							InlineUIContainer iui = new InlineUIContainer( grid, tp );
							iui.BaselineAlignment = BaselineAlignment.TextBottom;
						}

						emoticonFound = string.Empty;
						ChatUser user;
						index = GetNextUser( text, 0, out emoticonFound, out user );
						if( index >= 0 )
						{
							TextPointer tp = i.ContentStart;
							while( !tp.GetTextInRun( LogicalDirection.Forward ).StartsWith( emoticonFound ) )
							{
								tp = tp.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextPointer end = tp;
							for( int j = 0; j < emoticonFound.Length; j++ )
							{
								end = end.GetNextInsertionPosition( LogicalDirection.Forward );
							}
							TextRange tr = new TextRange( tp, end );
							tr.Text = string.Empty;

							ChatUserImage userImage = new ChatUserImage { User = user };
							

							if( size != Size.Empty )
							{
								userImage.Height = size.Height;
								userImage.Width = size.Width;
							}

							Grid grid = new Grid();
							grid.ToolTip = emoticonFound;
							grid.Children.Add( userImage );
							InlineUIContainer iui = new InlineUIContainer( grid, tp );
							iui.BaselineAlignment = BaselineAlignment.TextBottom;
						}
					}
				}
			}
			catch( Exception )
			{
				// Do nothing, error processing emoticons
			}
		}

		private static UIElement CreateHyperlink( string url )
		{
			TextBlock txt = new TextBlock
			{
				Text = url,
				ToolTip = url,
				Cursor = Cursors.Hand,
				TextDecorations = System.Windows.TextDecorations.Underline,
				TextWrapping = TextWrapping.Wrap,
			};
			txt.PreviewMouseLeftButtonDown += ( sender, args ) =>
			{
				try
				{
					Process.Start( ( sender as TextBlock ).Text );
				}
				catch( Exception ex )
				{
					MedConfigMessageBox.ShowError( ex.Message );
				}
			};
			txt.ContextMenu = new ContextMenu();
			MenuItem clickMenuItem = new MenuItem() { Header = "Copy Url" };
			clickMenuItem.Click += ( y, z ) =>
			{
				Clipboard.SetText( ( ( ( y as MenuItem ).Parent as ContextMenu ).PlacementTarget as TextBlock ).Text );
			};
			Grid grid = new Grid();
			grid.Children.Add( txt );
			grid.ToolTip = url;
			txt.ContextMenu.Items.Add( clickMenuItem );
			return grid;
		}
		#endregion

		private TextPointer _textSelectionStart = null;
		private TextRange _selectedRange = null;

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			Focus();
			_textSelectionStart = GetPositionFromPoint( e.GetPosition( this ), true );
			ClearSelection();

			if( e.ClickCount == 2 )
			{
				// TODO: Handle highlighting the current word
			}
			else if( e.ClickCount == 3 )
			{
				SelectAll();
			}

			e.Handled = true;
		}


		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if( _textSelectionStart != null && e.LeftButton == MouseButtonState.Pressed )
			{
				TextPointer endSelectPosition = GetPositionFromPoint( e.GetPosition( this ), true );
				SelectRange( _textSelectionStart, endSelectPosition );
			}
		}

		protected override void OnLostFocus( RoutedEventArgs e )
		{
			base.OnLostFocus( e );
			_textSelectionStart = null;
			ClearSelection();
		}

		private void ClearSelection()
		{
			TextRange range = new TextRange( ContentStart, ContentEnd );

			range.ApplyPropertyValue( TextElement.ForegroundProperty, Foreground );
			range.ApplyPropertyValue( TextElement.BackgroundProperty, null );

			_selectedRange = null;

			foreach( Inline inline in Inlines )
			{
				if( inline is InlineUIContainer )
				{
					InlineUIContainer container = inline as InlineUIContainer;
					container.BaselineAlignment = BaselineAlignment.TextBottom;
					if( container.Child is Grid )
					{
						( container.Child as Grid ).ClearValue( Grid.BackgroundProperty );
						( container.Child as Grid ).ClearValue( TextElement.ForegroundProperty );
					}

				}
			}

			ClearContextMenu();
		}

		private void SelectAll()
		{
			TextRange range = new TextRange( ContentStart, ContentEnd );

			Brush background = FindBackground();
			Brush foreground = FindForeground();

			range.ApplyPropertyValue( TextElement.ForegroundProperty, background );
			range.ApplyPropertyValue( TextElement.BackgroundProperty, foreground );

			foreach( Inline inline in Inlines )
			{
				if( inline is InlineUIContainer )
				{
					InlineUIContainer container = inline as InlineUIContainer;
					container.BaselineAlignment = BaselineAlignment.TextBottom;
					if( container.Child is Grid )
					{
						( container.Child as Grid ).Background = foreground;
						TextElement.SetForeground( container.Child, background );
					}
				}
			}

			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Items.Add( new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy } );
			ContextMenu = contextMenu;

			_selectedRange = range;
		}





		public Brush HighlightBackground
		{
			get { return (Brush)GetValue( HighlightBackgroundProperty ); }
			set { SetValue( HighlightBackgroundProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for HighlightBackground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HighlightBackgroundProperty =
			DependencyProperty.Register( "HighlightBackground", typeof( Brush ), typeof( EmoticonRichTextBox ), new PropertyMetadata( null ) );




		public Brush HighlightForeground
		{
			get { return (Brush)GetValue( HighlightForegroundProperty ); }
			set { SetValue( HighlightForegroundProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for HighlightForeground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HighlightForegroundProperty =
			DependencyProperty.Register( "HighlightForeground", typeof( Brush ), typeof( EmoticonRichTextBox ), new PropertyMetadata( null ) );




		private Brush FindBackground()
		{
			Brush background = HighlightBackground;

			if( background == null )
			{
				background = Background;
			}

			if( background == null )
			{
				ChatMessageCallout callout = this.FindParent<ChatMessageCallout>();
				if( callout != null )
				{
					background = callout.Background;
				}
			}

			if( background == null )
			{
				background = TryFindResource( "ChatMessageCalloutNormalBackgroundBrush" ) as SolidColorBrush;
			}
			return background;
		}

		private Brush FindForeground()
		{
			Brush foreground = HighlightForeground;

			if( foreground == null )
			{
				foreground = Foreground;
			}

			if( foreground == null )
			{
				ChatMessageCallout callout = this.FindParent<ChatMessageCallout>();
				if( callout != null )
				{
					foreground = callout.Foreground;
				}
			}

			if( foreground == null )
			{
				foreground = TryFindResource( "ForegroundBrush" ) as SolidColorBrush;
			}
			return foreground;
		}

		private void SelectRange( TextPointer start, TextPointer end )
		{
			ClearSelection();

			TextRange range = new TextRange( start, end );
			//ChatMessageCallout callout = this.FindParent<ChatMessageCallout>();
			//Brush background = Application.Current.TryFindResource( "ForegroundBrush" ) as SolidColorBrush;
			//Brush foreground = Application.Current.TryFindResource( "ChatMessageCalloutNormalBackgroundBrush" ) as SolidColorBrush;
			//if( callout != null )
			//{
			//	background = Foreground;
			//	foreground = callout.Background;
			//}

			Brush background = FindBackground();
			Brush foreground = FindForeground();

			range.ApplyPropertyValue( TextElement.ForegroundProperty, background );
			range.ApplyPropertyValue( TextElement.BackgroundProperty, foreground );

			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Items.Add( new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy } );
			ContextMenu = contextMenu;

			_selectedRange = range;

			foreach( Inline inline in Inlines )
			{
				if( inline is InlineUIContainer )
				{
					InlineUIContainer container = inline as InlineUIContainer;
					container.BaselineAlignment = BaselineAlignment.TextBottom;
					if( _selectedRange.Contains( container.ContentStart ) &&
						_selectedRange.Contains( container.ContentEnd ) )
					{
						if( container.Child is Grid )
						{
							( container.Child as Grid ).Background = foreground;
							TextElement.SetForeground( container.Child, background );
						}
					}
				}
			}

			if( string.IsNullOrEmpty( _selectedRange.Text ) )
			{
				ClearContextMenu();
			}
		}

		private void ClearContextMenu()
		{
			MessageBubble parent = this.FindParent<MessageBubble>();
			if( parent != null )
			{
				ContextMenu = parent.ContextMenu;
			}
			else if( ContextMenu != null )
			{
				ContextMenu.Items.Clear();
			}
		}

		protected override void OnMouseEnter( MouseEventArgs e )
		{
			base.OnMouseEnter( e );

			if( e.LeftButton == MouseButtonState.Released )
			{
				_textSelectionStart = null;
			}
		}

		protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnPreviewMouseLeftButtonUp( e );

			_textSelectionStart = null;

			e.Handled = true;
		}

		public EmoticonRichTextBox()
		{
			Focusable = true;

			CommandBindings.Add( new CommandBinding( ApplicationCommands.Copy, OnCopy, CanCopy ) );

			Cursor = Cursors.IBeam;
		}

		private void OnCopy( object sender, ExecutedRoutedEventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach( Inline inline in this.Inlines )
			{
				if( inline is Run )
				{
					Run run = inline as Run;
					if( run.Background != null )
					{
						//if( run.TextDecorations == System.Windows.TextDecorations.Strikethrough )
						//{
						//	sb.AppendFormat( "-{0}-", run.Text );
						//}
						//else if( run.TextDecorations == System.Windows.TextDecorations.Underline )
						//{
						//	sb.AppendFormat( "_{0}_", run.Text );
						//}
						//else if( run.FontWeight == FontWeights.Bold )
						//{
						//	sb.AppendFormat( "*{0}*", run.Text );
						//}
						//else if( run.FontStyle == FontStyles.Italic )
						//{
						//	sb.AppendFormat( "/{0}/", run.Text );
						//}
						//else
						{
							sb.Append( run.Text );
						}
					}
				}
				else if( inline is InlineUIContainer )
				{
					InlineUIContainer container = inline as InlineUIContainer;
					container.BaselineAlignment = BaselineAlignment.TextBottom;

					if( _selectedRange.Contains( container.ContentStart ) &&
						_selectedRange.Contains( container.ContentEnd ) )
					{
						if( container.Child is Grid )
						{
							Grid image = container.Child as Grid;
							sb.Append( image.ToolTip );
						}
					}
				}
			}
			Clipboard.SetText( sb.ToString() );
		}

		private void CanCopy( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = _selectedRange != null;
		}

		private void OnSelectAll( object sender, ExecutedRoutedEventArgs e )
		{
			SelectAll();
		}
	}
}
