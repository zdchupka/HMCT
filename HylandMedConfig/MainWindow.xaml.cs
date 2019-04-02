using HylandMedConfig.Properties;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HylandMedConfig.Common;
using System.Text;
using HylandMedConfig.Windows;

namespace HylandMedConfig
{
	public partial class MainWindow : Window
	{
		public ApplicationViewModel ViewModel
		{
			get
			{
				return DataContext as ApplicationViewModel;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			Width = Settings.Default.MainWindowWidth;
			Height = Settings.Default.MainWindowHeight;
			Top = Settings.Default.MainWindowTop;
		}

		private void MainWindow_StateChanged( object sender, EventArgs e )
		{
			if( WindowState == WindowState.Normal || WindowState == WindowState.Maximized )
			{
				ViewModel.NotifyActive();
			}
		}

		private void MainWindow_ReportActive( object sender, EventArgs e )
		{
			ViewModel.NotifyActive();
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			txtMessage.txtMessage.Focus();
			ScrollMessagesToBottom();

			if( Settings.Default.EnableAnimations )
			{
				Application.Current.Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.Slide;
				Application.Current.Resources[SystemParameters.ComboBoxPopupAnimationKey] = PopupAnimation.Slide;
			}
			else
			{
				Application.Current.Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.None;
				Application.Current.Resources[SystemParameters.ComboBoxPopupAnimationKey] = PopupAnimation.None;
			}
		}

		public double GetVerticalScrollPosition()
		{
			return chatList?.GetVerticalScrollPosition() ?? 0d;
		}

		public void SetVerticalScrollPosition( double offset )
		{
			chatList?.SetVerticalScrollPosition( offset );
		}

		public void ScrollMessagesToBottom()
		{
			chatList?.ScrollToBottom( true );
		}

		public void ScrollMessagesToTop()
		{
			chatList?.ScrollToTop();
		}

		private void MainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if( WindowState == WindowState.Normal )
			{
				Settings.Default.MainWindowHeight = Height;
				Settings.Default.MainWindowWidth = Width;
				Settings.Default.MainWindowLeft = Left;
				Settings.Default.MainWindowTop = Top;
			}
			ViewModel.ShutdownCommand.Execute( null );
		}

		private void MainWindow_Activated( object sender, EventArgs e )
		{
			ViewModel.NotifyActive();
		}

		private void userListExpander_ExpandChanged( object sender, EventArgs e )
		{
			if( chatList.RenderSize.Height > 0d && Settings.Default.EnableAnimations )
			{
				RenderTargetBitmap target = new RenderTargetBitmap( (int)chatList.RenderSize.Width, (int)chatList.RenderSize.Height, 96.0, 96.0, PixelFormats.Default );
				target.Render( chatList );
				messageListPreview.Source = target;
			}
			else
			{
				messageListPreview.Source = null;
			}
		}

		private void root_Deactivated( object sender, EventArgs e )
		{

			if( Settings.Default.ObscureWindow )
			{
				bool childActive = false;
				foreach( Window ownedWindow in this.OwnedWindows )
				{
					if( ownedWindow.IsVisible )
					{
						childActive = true;
						break;
					}
				}
				if( !childActive )
				{
					obscureScreen.Visibility = Visibility.Visible;
				}
			}
		}

		private void obscureScreen_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			obscureScreen.Visibility = Visibility.Collapsed;
		}

		private void ClearTagMessages_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = ViewModel.ClearTagMessagesCommand.CanExecute( e.Parameter );
		}

		private void ClearTagMessages_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ViewModel.ClearTagMessagesCommand.Execute( e.Parameter );
		}

		private void MenuItem_Click( object sender, RoutedEventArgs e )
		{
			TroubleshootingWindow.ShowTroubleshooting();
		}
	}
}
