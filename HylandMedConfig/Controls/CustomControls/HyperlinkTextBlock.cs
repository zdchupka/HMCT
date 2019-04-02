using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HylandMedConfig.XamlControls
{
	/// <summary>
	/// Custom control to enable Hyperlinks to people using /xaml messages (useful for bots)
	/// </summary>
	public class HyperlinkTextBlock : TextBlock
	{
		public Uri UriSource
		{
			get { return (Uri)GetValue( UriSourceProperty ); }
			set { SetValue( UriSourceProperty, value ); }
		}

		public static readonly DependencyProperty UriSourceProperty =
			DependencyProperty.Register( "UriSource", typeof( Uri ), typeof( HyperlinkTextBlock ), new PropertyMetadata( null ) );

		public HyperlinkTextBlock()
		{
			MouseLeftButtonDown += HyperlinkTextBlock_MouseLeftButtonDown;

			ContextMenu = new ContextMenu();
			MenuItem clickMenuItem = new MenuItem() { Header = "Copy Url" };
			clickMenuItem.Command = HylandMedConfig.Commands.ApplicationCommands.CopyTextToClipboard;
			clickMenuItem.SetBinding( MenuItem.CommandParameterProperty, new Binding( "UriSource.AbsoluteUri" ) { Source = this } );
			ContextMenu.Items.Add( clickMenuItem );
		}

		private void HyperlinkTextBlock_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			try
			{
				if( UriSource != null && !string.IsNullOrWhiteSpace( UriSource.AbsoluteUri ) )
				{

					if( !string.IsNullOrWhiteSpace( UriSource.AbsoluteUri ) )
					{
						Process.Start( UriSource.AbsoluteUri );
					}

				}
			}
			catch( Exception ex )
			{
				HylandMedConfig.Windows.MedConfigMessageBox.ShowError( ex.Message );
			}
		}

		static HyperlinkTextBlock()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( HyperlinkTextBlock ), new FrameworkPropertyMetadata( typeof( HyperlinkTextBlock ) ) );
		}
	}

	public class HyperlinkRun : Run
	{
		public Uri UriSource
		{
			get { return (Uri)GetValue( UriSourceProperty ); }
			set { SetValue( UriSourceProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for UriSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty UriSourceProperty =
			DependencyProperty.Register( "UriSource", typeof( Uri ), typeof( HyperlinkRun ), new FrameworkPropertyMetadata( ( s, a ) =>
			{
				HyperlinkRun thisControl = s as HyperlinkRun;
				if( thisControl != null && thisControl.UriSource != null && thisControl.UriSource.IsAbsoluteUri )
				{
					thisControl.ToolTip = thisControl.UriSource.AbsoluteUri;
				}
			} ) );

		public HyperlinkRun()
			: base()
		{
			ContextMenu = new ContextMenu();
			MenuItem clickMenuItem = new MenuItem() { Header = "Copy Url" };
			clickMenuItem.Command = HylandMedConfig.Commands.ApplicationCommands.CopyTextToClipboard;
			clickMenuItem.SetBinding( MenuItem.CommandParameterProperty, new Binding( "UriSource.AbsoluteUri" ) { Source = this } );
			ContextMenu.Items.Add( clickMenuItem );
		}



		protected override void OnMouseDown( MouseButtonEventArgs e )
		{
			try
			{
				if( e.LeftButton == MouseButtonState.Pressed && UriSource != null && !string.IsNullOrWhiteSpace( UriSource.AbsoluteUri ) )
				{

					if( !string.IsNullOrWhiteSpace( UriSource.AbsoluteUri ) )
					{
						Process.Start( UriSource.AbsoluteUri );
					}

				}
			}
			catch( Exception ex )
			{
				HylandMedConfig.Windows.MedConfigMessageBox.ShowError( ex.Message );
			}
		}
	}
}
