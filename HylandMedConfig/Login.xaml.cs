using HylandMedConfig.Common;
using HylandMedConfig.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using HylandMedConfig.Services;

namespace HylandMedConfig
{
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();

			txtUsername.Text = Environment.UserName.ToLower();

			Loaded += Login_Loaded;

#if DEBUG
			txtUsername.IsReadOnly = false;		
#endif
		}

		private void Login_Loaded( object sender, RoutedEventArgs e )
		{
#if !DEBUG
			// Auto login when in release
			Button_Click( this, new RoutedEventArgs() );
#endif
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ApplicationViewModel.Current = new ApplicationViewModel( ChatProxyFactory.CreateChatProxy( txtUsername.Text, new UserSettingsNicknameService(), new UserSettingsTagService() ) );

				Application.Current.MainWindow = new MainWindow
				{
					DataContext = ApplicationViewModel.Current
				};

				Application.Current.MainWindow.Show();
				Close();
			}
			catch( ChatUserLoginException ex )
			{
				Mouse.OverrideCursor = null;
				MedConfigMessageBox.ShowError( ex.Message );
#if !DEBUG
				Application.Current.Shutdown();
#endif
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}
	}
}
