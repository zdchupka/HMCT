using HylandMedConfig.Common;
using HylandMedConfig.Properties;
using HylandMedConfig.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HylandMedConfig.Controls
{
	/// <summary>
	/// Interaction logic for Whiteboard.xaml
	/// </summary>
	public partial class Whiteboard : UserControl
	{
		#region Declarations

		private WhiteboardEntry _currentEntry;
		private Point _startPoint;
		private WhiteboardEntry[] _savedEntries;

		#endregion

		#region Properties



		public double CanvasHeight
		{
			get { return (double)GetValue( CanvasHeightProperty ); }
			set { SetValue( CanvasHeightProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for CanvasHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanvasHeightProperty =
			DependencyProperty.Register( "CanvasHeight", typeof( double ), typeof( Whiteboard ), new PropertyMetadata( 500d ) );



		public bool ClipContent
		{
			get { return (bool)GetValue( ClipContentProperty ); }
			set { SetValue( ClipContentProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for ClipContent.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ClipContentProperty =
			DependencyProperty.Register( "ClipContent", typeof( bool ), typeof( Whiteboard ), new PropertyMetadata( true ) );



		public double CanvasWidth
		{
			get { return (double)GetValue( CanvasWidthProperty ); }
			set { SetValue( CanvasWidthProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for CanvasWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanvasWidthProperty =
			DependencyProperty.Register( "CanvasWidth", typeof( double ), typeof( Whiteboard ), new PropertyMetadata( 500d ) );



		public WhiteboardChatMessage Message
		{
			get { return (WhiteboardChatMessage)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( WhiteboardChatMessage ), typeof( Whiteboard ), new FrameworkPropertyMetadata( ( s, a ) =>
			 {
				 Whiteboard wb = s as Whiteboard;
				 if( wb != null )
				 {
					 wb.DownloadNewImage();
				 }
			 } ) );

		#endregion

		private void DownloadNewImage()
		{
			if( !string.IsNullOrWhiteSpace( Message.ImageUrl ) )
			{
				try
				{
					Uri uri = new Uri( Message.ImageUrl );

					using( WebClient webClient = new WebClient() )
					{
						webClient.Proxy = null;  //avoids dynamic proxy discovery delay
						webClient.CachePolicy = new RequestCachePolicy( RequestCacheLevel.Default );

						byte[] imageBytes = null;

						imageBytes = webClient.DownloadData( uri );

						if( imageBytes == null )
						{
							return;
						}
						MemoryStream imageStream = new MemoryStream( imageBytes );
						BitmapImage image = new BitmapImage();

						image.BeginInit();
						image.StreamSource = imageStream;
						image.CacheOption = BitmapCacheOption.OnLoad;
						image.EndInit();

						image.Freeze();
						imageStream.Close();
						SetCanvasSize( image.PixelHeight, image.PixelWidth );
					}
				}
				catch( Exception )
				{
					//do something to report the exception

				}
			}
		}

		private void Whiteboard_DownloadFailed( object sender, ExceptionEventArgs e )
		{

		}

		private void Whiteboard_DownloadCompleted( object sender, EventArgs e )
		{
			BitmapFrame frame = sender as BitmapFrame;
			if( frame != null )
			{
				SetCanvasSize( frame.PixelHeight, frame.PixelWidth );
			}
		}

		private void SetCanvasSize( double pixelHeight, double pixelWidth )
		{
			CanvasHeight = Math.Min( pixelHeight, 800d );
			CanvasWidth = Math.Min( pixelWidth, 800d );

			double ratio = pixelWidth / pixelHeight;

			if( CanvasHeight == 800d && CanvasWidth < 800d )
			{
				CanvasWidth = 800d * ratio;
			}

			else if( CanvasWidth == 800d && CanvasHeight < 800d )
			{
				CanvasHeight = 800d / ratio;
			}

			// If we get here the width and height are the same, pick the larger one
			else if( CanvasWidth == 800d && CanvasHeight == 800d )
			{
				if( pixelWidth > pixelHeight )
				{
					//https://static.vecteezy.com/system/resources/previews/000/096/058/original/vector-bokeh-glitter-background-illustration.jpg
					CanvasHeight = 800d / ratio;
				}
				else
				{
					CanvasWidth = 800d * ratio;
				}
			}
		}

		#region Construction

		public Whiteboard()
		{
			InitializeComponent();
		}

		#endregion

		public bool CanDraw()
		{
			return !Message.IsLocked || Message.FromUser == ApplicationViewModel.Current.ChatProxy.CurrentUser;
		}

		private void Canvas_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			if( !Keyboard.IsKeyDown( Key.Space ) && !Keyboard.IsKeyDown( Key.RightCtrl ) && !Keyboard.IsKeyDown( Key.LeftCtrl ) )
			{
				BeginRecording( e.GetPosition( sender as Canvas ) );
				e.Handled = true;
			}
		}

		private void Canvas_MouseMove( object sender, MouseEventArgs e )
		{
			if( e.RightButton == MouseButtonState.Pressed )
			{
				return;
			}

			if( e.LeftButton == MouseButtonState.Pressed )
			{
				if( !CanDraw() )
				{
					if( _currentEntry != null && Message.Entries.Contains( _currentEntry ) )
					{
						Message.Entries.Remove( _currentEntry );
					}
					//ApplicationViewModel.Current.IsDrawing = false;
					_currentEntry = null;
				}

				Point currentPoint = e.GetPosition( sender as Canvas );
				if( _currentEntry != null )
				{
					_currentEntry.Points.Add( currentPoint );
				}

				//if( _currentEntry == null )
				//{
				//	BeginRecording( e.GetPosition( sender as Canvas ) );
				//}
			}
			else if( _currentEntry != null )
			{
				SendCurrentData();
			}
		}

		private void myCanvas_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			SendCurrentData();
		}

		private void theCanvas_MouseLeave( object sender, MouseEventArgs e )
		{
			//SendCurrentData();
		}

		private void BeginRecording( Point p )
		{
			if( CanDraw() )
			{
				_startPoint = p;

				Brush brush = ApplicationViewModel.Current.CurrentBrush.Clone();
				brush.Opacity = ApplicationViewModel.Current.CurrentOpacity;

				_currentEntry = new WhiteboardEntry
				{
					User = ApplicationViewModel.Current.ChatProxy.CurrentUser,
					Points = new PointCollection { p },
					Stroke = brush,
					StrokeThickness = ApplicationViewModel.Current.CurrentThickness,
				};

				Message.Entries.Add( _currentEntry );
			}
		}

		private void SendCurrentData()
		{
			if( _currentEntry != null && CanDraw() )
			{
				ApplicationViewModel.Current.ChatProxy.SendWhiteboardData( Message, _currentEntry );
				//ApplicationViewModel.Current.IsDrawing = false;
				_currentEntry = null;
			}
		}

		private void theCanvas_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
		{
			if( e.RightButton == MouseButtonState.Pressed && _currentEntry != null )
			{
				Message.Entries.Remove( _currentEntry );
				_currentEntry = null;
				//ApplicationViewModel.Current.IsDrawing = false;
				e.Handled = true;
			}
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			_savedEntries = new WhiteboardEntry[Message.Entries.Count];
			Message.Entries.CopyTo( _savedEntries, 0 );
		}

		private void Button_Click_1( object sender, RoutedEventArgs e )
		{
			if( _savedEntries != null )
			{
				ApplicationViewModel.Current.ChatProxy.ClearWhiteboard( Message );

				foreach( WhiteboardEntry entry in _savedEntries )
				{
					ApplicationViewModel.Current.ChatProxy.SendWhiteboardData( Message, entry );
					Message.Entries.Add( entry );
				}
			}
		}

		private void MenuItem_Click( object sender, RoutedEventArgs e )
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.AddExtension = true;
			dlg.Filter = "PNG|*.png";
			dlg.InitialDirectory = @"\\one-021619\Share\HylandMedConfig\Whiteboards";
			dlg.FileName = Message.ID.ToString();
			if( dlg.ShowDialog() ?? false )
			{
				try
				{
					var src = GetImage( itemsControl );
					using( FileStream fileStream = new FileStream( dlg.FileName, FileMode.OpenOrCreate ) )
					{
						SaveAsPng( src, fileStream );
					}
				}
				catch( Exception ex )
				{
					MedConfigMessageBox.ShowError( ex.Message );
				}
			}
		}

		public static RenderTargetBitmap GetImage( ItemsControl view )
		{
			Size size = new Size( view.ActualWidth, view.ActualHeight );
			if( size.IsEmpty )
				return null;

			RenderTargetBitmap result = new RenderTargetBitmap( (int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32 );

			DrawingVisual drawingvisual = new DrawingVisual();
			using( DrawingContext context = drawingvisual.RenderOpen() )
			{
				context.DrawRectangle( new VisualBrush( view ), null, new Rect( new Point(), size ) );
				context.Close();
			}

			result.Render( drawingvisual );
			return result;
		}

		public static void SaveAsPng( RenderTargetBitmap src, Stream outputStream )
		{
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add( BitmapFrame.Create( src ) );

			encoder.Save( outputStream );
		}

		private void MenuItem_Click_1( object sender, RoutedEventArgs e )
		{
			try
			{
				var src = GetImage( itemsControl );
				Clipboard.SetImage( src );
			}
			catch( Exception ex )
			{
				MedConfigMessageBox.ShowError( ex.Message );
			}

		}

		private void MenuItem_Click_2( object sender, RoutedEventArgs e )
		{
			EditWhiteboardWindow window = new EditWhiteboardWindow() { Message = Message };
			window.Show();
		}

		private void root_Loaded( object sender, RoutedEventArgs e )
		{
			//var test = VisualTreeHelper.GetChild( zoom, 0 );
		}
	}
}
