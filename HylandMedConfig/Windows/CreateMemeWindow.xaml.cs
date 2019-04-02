using HylandMedConfig.Common;
using Microsoft.Win32;
using System.Windows;

namespace HylandMedConfig.Windows
{
	public partial class CreateMemeWindow : Window
	{
		public MemeChatMessage Message
		{
			get { return (MemeChatMessage)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( MemeChatMessage ), typeof( CreateMemeWindow ), new PropertyMetadata( null ) );

		public CreateMemeWindow()
		{
			Message = new MemeChatMessage { FromUser = ApplicationViewModel.Current.ChatProxy.CurrentUser };

			InitializeComponent();
		}

		private void root_Loaded( object sender, RoutedEventArgs e )
		{
			if( string.IsNullOrEmpty( Message.ImageUrl ) )
			{
				Button_Click( sender, e );
			}
			else
			{
				txtFirstLine.SelectAll();
				txtFirstLine.Focus();
			}
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			OpenFileDialog dlg = new OpenFileDialog()
			{
				InitialDirectory = @"\\one-021619\Share\HylandMedConfig\Memes\Images",
				Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;",
			};
			if( dlg.ShowDialog() ?? false )
			{
				Message.ImageUrl = dlg.FileName;
				txtFirstLine.SelectAll();
				txtFirstLine.Focus();
			}
		}

		private void Button_Click_1( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}
	}
}
