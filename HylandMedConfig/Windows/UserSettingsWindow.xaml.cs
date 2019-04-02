using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using HylandMedConfig.Properties;
using Microsoft.Win32;
using HylandMedConfig.Windows;

namespace HylandMedConfig.Dialogs
{
	/// <summary>
	/// Interaction logic for UserSettingsDialog.xaml
	/// </summary>
	public partial class UserSettingsDialog : Window
	{
		private List<UITheme> _themes = new List<UITheme>
		{
			UITheme.IG,
			UITheme.Metro,
			UITheme.MetroDark,
		};

		/// <summary>
		/// Gets a view that represents the available themes
		/// </summary>
		public ICollectionView AvailableThemesView
		{
			get;
			private set;
		}

		public UserSettingsDialog()
		{
			AvailableThemesView = new ListCollectionView( _themes );

			InitializeComponent();

			cmbBackgroundImageStretch.Items.Add( Stretch.Fill );
			cmbBackgroundImageStretch.Items.Add( Stretch.None );
			cmbBackgroundImageStretch.Items.Add( Stretch.Uniform );
			cmbBackgroundImageStretch.Items.Add( Stretch.UniformToFill );

			Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
			Top = Application.Current.MainWindow.Top;
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			cmbBackgroundImageStretch.SelectedItem = Settings.Default.BackgroundImageStretch;

			if( !IsFullyVisible() )
			{
				Left = Application.Current.MainWindow.Left - ActualWidth;
				if( !IsFullyVisible() )
				{
					Left = Application.Current.MainWindow.Left + ActualWidth;
				}
			}
		}

		bool IsFullyVisible()
		{
			return
				isPointVisibleOnAScreen( new Point( Left, Top + 1 ) ) &&
				isPointVisibleOnAScreen( new Point( Left + ActualWidth, Top + 1 ) );
		}


		bool isPointVisibleOnAScreen( Point p )
		{
			foreach( System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens )
			{
				if( p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom )
					return true;
			}
			return false;
		}

		private void Run_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if( dlg.ShowDialog( Window.GetWindow( this ) ) ?? false )
			{
				Settings.Default.IMReceivedPath = dlg.FileName;
				Settings.Default.UseRingtone = true;
			}
		}

		private void ChangeBackground_Click( object sender, RoutedEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog() { Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png, *.gif, *.tiff)|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff;" };
			if( dlg.ShowDialog() ?? false )
			{
				Settings.Default.BackgroundImageUrl = dlg.FileName;
			}
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			Settings.Default.Save();
		}

		private void ChangeFont_Click( object sender, RoutedEventArgs e )
		{
			System.Windows.Forms.FontDialog fd = new System.Windows.Forms.FontDialog();

			System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;

			if( Settings.Default.FontStyle == FontStyles.Italic )
			{
				style |= System.Drawing.FontStyle.Italic;
			}

			if( Settings.Default.FontWeight == FontWeights.Bold )
			{
				style |= System.Drawing.FontStyle.Bold;
			}

			System.Drawing.Font font = new System.Drawing.Font(
				Settings.Default.FontFamily.ToString(),
				(float)Settings.Default.FontSize * 72.0f / 96.0f,
				style );

			fd.Font = font;

			System.Windows.Forms.DialogResult dr = fd.ShowDialog();
			if( dr != System.Windows.Forms.DialogResult.Cancel )
			{
				Settings.Default.FontFamily = new System.Windows.Media.FontFamily( fd.Font.Name );
				Settings.Default.FontSize = fd.Font.Size * 96.0 / 72.0;
				Settings.Default.FontWeight = fd.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
				Settings.Default.FontStyle = fd.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
			}
		}

		private void ResetBackground_Click( object sender, RoutedEventArgs e )
		{
			if( MessageBox.Show(
				"Are you sure you want to reset background settings?",
				"Confirm",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question ) == MessageBoxResult.Yes )
			{
				ResetBackgroundSettings();
			}
		}

		private void ResetBackgroundSettings()
		{
			Settings.Default.Reset( "BackgroundImageOffsetX" );
			Settings.Default.Reset( "BackgroundImageOffsetY" );
			Settings.Default.Reset( "BackgroundImageOpacity" );
			Settings.Default.Reset( "BackgroundImageScale" );
			Settings.Default.Reset( "BackgroundImageStretch" );
			Settings.Default.Reset( "BackgroundImageUrl" );
		}

		private void ResetFont_Click( object sender, RoutedEventArgs e )
		{
			if( MessageBox.Show(
			   "Are you sure you want to reset message styling settings?",
			   "Confirm",
			   MessageBoxButton.YesNo,
			   MessageBoxImage.Question ) == MessageBoxResult.Yes )
			{
				ResetFontSettings();
			}
		}

		private void ResetSystemFont_Click( object sender, RoutedEventArgs e )
		{
			if( MessageBox.Show(
			   "Are you sure you want to reset System Font styling settings?",
			   "Confirm",
			   MessageBoxButton.YesNo,
			   MessageBoxImage.Question ) == MessageBoxResult.Yes )
			{
				ResetSystemFontSettings();
			}
		}

		private void ResetSystemFontSettings()
		{
			Settings.Default.Reset( "SystemFontFamily" );
			Settings.Default.Reset( "SystemFontSize" );
			Settings.Default.Reset( "SystemFontWeight" );
			Settings.Default.Reset( "SystemFontStyle" );
			Settings.Default.Reset( "SystemForegroundColor" );
		}

		private void ResetFontSettings()
		{
			Settings.Default.Reset( "FontFamily" );
			Settings.Default.Reset( "FontSize" );
			Settings.Default.Reset( "FontWeight" );
			Settings.Default.Reset( "FontStyle" );
			Settings.Default.Reset( "MessageForegroundColor" );
			Settings.Default.Reset( "MessageBackgroundColor" );
			Settings.Default.Reset( "WhisperMessageForegroundColor" );
			Settings.Default.Reset( "WhisperMessageBackgroundColor" );
			Settings.Default.Reset( "GroupMessageForegroundColor" );
			Settings.Default.Reset( "GroupMessageBackgroundColor" );
			Settings.Default.Reset( "ShowMessageBorder" );
			Settings.Default.Reset( "ShowMessageDropShadow" );
			Settings.Default.Reset( "MessageBorderColor" );
			Settings.Default.Reset( "UserImageHeight" );
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			new IgnoreTagSettingsWindow() { Owner = GetWindow( this ) }.ShowDialog();
		}

		private void ExportUserSettings_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			SaveFileDialog dlg = new SaveFileDialog()
			{
				DefaultExt = "xml",
				Title = "Export Settings",
				CheckFileExists = false,
				AddExtension = true,
				Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
			};
			if( dlg.ShowDialog( this ) ?? false )
			{
				Settings.Default.Export( dlg.FileName );
			}
		}

		private void ImportUserSettings_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog()
			{
				DefaultExt = "xml",
				Title = "Import Settings",
				Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
			};
			if( dlg.ShowDialog( this ) ?? false )
			{
				Settings.Default.Import( dlg.FileName );
			}
		}

		private void Button_Click_1( object sender, RoutedEventArgs e )
		{
			System.Windows.Forms.FontDialog fd = new System.Windows.Forms.FontDialog();

			System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;

			if( Settings.Default.SystemFontStyle == FontStyles.Italic )
			{
				style |= System.Drawing.FontStyle.Italic;
			}

			if( Settings.Default.SystemFontWeight == FontWeights.Bold )
			{
				style |= System.Drawing.FontStyle.Bold;
			}

			System.Drawing.Font font = new System.Drawing.Font(
				Settings.Default.SystemFontFamily.ToString(),
				(float)Settings.Default.SystemFontSize * 72.0f / 96.0f,
				style );

			fd.Font = font;

			System.Windows.Forms.DialogResult dr = fd.ShowDialog();
			if( dr != System.Windows.Forms.DialogResult.Cancel )
			{
				Settings.Default.SystemFontFamily = new System.Windows.Media.FontFamily( fd.Font.Name );
				Settings.Default.SystemFontSize = fd.Font.Size * 96.0 / 72.0;
				Settings.Default.SystemFontWeight = fd.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
				Settings.Default.SystemFontStyle = fd.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
			}
		}

		private void Button_Click_2( object sender, RoutedEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if( dlg.ShowDialog( Window.GetWindow( this ) ) ?? false )
			{
				Settings.Default.ThumbsUpUrl = dlg.FileName;
			}
		}

		private void Button_Click_3( object sender, RoutedEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if( dlg.ShowDialog( Window.GetWindow( this ) ) ?? false )
			{
				Settings.Default.ThumbsDownUrl = dlg.FileName;
			}
		}
	}
}
