using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace HylandMedConfig.Windows
{
	/// <summary>
	/// Interaction logic for MedConfigMessageBox.xaml
	/// </summary>
	public partial class MedConfigMessageBox : Window
	{
		private enum DialogType
		{
			Error,
			Information,
		}
		public string Text
		{
			get { return (string)GetValue( TextProperty ); }
			set { SetValue( TextProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register( "Text", typeof( string ), typeof( MedConfigMessageBox ), new PropertyMetadata( "" ) );


		private MedConfigMessageBox( DialogType type )
		{
			InitializeComponent();

			if( type == DialogType.Error )
			{
				imgInfo.Visibility = Visibility.Collapsed;
			}
			else
			{
				imgError.Visibility = Visibility.Collapsed;
			}
		}

		public static void ShowError( string messageBoxText )
		{
			new MedConfigMessageBox( DialogType.Error )
			{
				Text = messageBoxText,
				Owner = Application.Current.MainWindow,
				Title = "Error",
			}.ShowDialog();
		}

		public static void ShowInfo( string messageBoxText, string title = "" )
		{
			new MedConfigMessageBox( DialogType.Information )
			{
				Text = messageBoxText,
				Owner = Application.Current.MainWindow,
				Title = title,
			}.ShowDialog();
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}
	}
}
