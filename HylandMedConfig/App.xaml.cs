using HylandMedConfig.Properties;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HylandMedConfig
{
	public partial class App : Application
	{
		private HotKey _hotKey;

		public App()
		{
			if( Settings.Default.UpgradeRequired )
			{
				Settings.Default.Upgrade();
				Settings.Default.UpgradeRequired = false;
				Settings.Default.Save();
			}

			if( Settings.Default.IgnoredUsers == null )
			{
				Settings.Default.IgnoredUsers = new System.Collections.Specialized.StringCollection();
				Settings.Default.Save();
			}

			if( Settings.Default.IgnoredTags == null )
			{
				Settings.Default.IgnoredTags = new System.Collections.Specialized.StringCollection();
				Settings.Default.Save();
			}

			ToolTipService.ShowDurationProperty.OverrideMetadata(
				typeof( DependencyObject ), new FrameworkPropertyMetadata( int.MaxValue ) );

			_hotKey = new HotKey( Key.F12, KeyModifier.Shift | KeyModifier.Ctrl, OnHotKeyHandler );

			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e )
		{
			if( e.Exception.GetType() == typeof( COMException ) )
			{
				e.Handled = true;
				HylandMedConfig.Windows.MedConfigMessageBox.ShowError( "COM Exception, idk what that is, continuing on..." );
			}
			else if( e.Exception.GetType() == typeof( OutOfMemoryException ) )
			{
				e.Handled = true;
				HylandMedConfig.Windows.MedConfigMessageBox.ShowError( "We like, ran out of memory..." );
			}
		}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			_hotKey.Dispose();
		}

		private void OnHotKeyHandler( HotKey hotKey )
		{
			ApplicationViewModel.Current.ToggleHiddenCommand.Execute( null );
		}
	}

	public class ThemeResourceDictionary : ResourceDictionary
	{
	}
}
