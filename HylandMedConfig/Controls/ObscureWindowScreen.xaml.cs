using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace HylandMedConfig.Controls
{
	/// <summary>
	/// Interaction logic for ObscureWindowScreen.xaml
	/// </summary>
	public partial class ObscureWindowScreen : UserControl
	{
		private static ImageSource CmdIcon;
		private static string _windowTitle = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.System ), "cmd.exe" );
		//private static ImageSource OriginalIcon;

		static ObscureWindowScreen()
		{
			string path = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.System ), "cmd.exe" );
			CmdIcon = Icon.ExtractAssociatedIcon( path ).ToImageSource();
		}

		public ObscureWindowScreen()
		{
			InitializeComponent();

			txtPrompt.Text = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) + ">";
			txtVersion.Text = string.Format( "Microsoft Windows [Version {0}]", Environment.OSVersion.Version.ToString() );
			txtCopyright.Text = string.Format( "(c) {0} Microsoft Corporation. All rights reserved.", DateTime.Now.Year );
			IsVisibleChanged += ObscureWindowScreen_IsVisibleChanged;
		}

		private void ObscureWindowScreen_IsVisibleChanged( object sender, System.Windows.DependencyPropertyChangedEventArgs e )
		{
			if( IsVisible )
			{
				Application.Current.MainWindow.Title = _windowTitle;
				Application.Current.MainWindow.Icon = CmdIcon;
			}
			else
			{
				Binding b = new Binding( "Version" );
				b.StringFormat = Properties.Resources.STR_MAIN_TITLE;

				Application.Current.MainWindow.SetBinding( Window.TitleProperty, b );
				Application.Current.MainWindow.Icon = null;
			}
		}
	}
}