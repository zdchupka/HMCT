using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using HylandMedConfig.Common;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace HylandMedConfig.Windows
{
	/// <summary>
	/// Interaction logic for SetMoodWindow.xaml
	/// </summary>
	public partial class SetMoodWindow : Window, IDataErrorInfo
	{
		public string Mood
		{
			get { return (string)GetValue( MoodProperty ); }
			set { SetValue( MoodProperty, value ); }
		}

		public string Error
		{
			get
			{
				return null;
			}
		}

		public string this[string columnName]
		{
			get
			{
				switch( columnName )
				{
					case nameof( Mood ):
						if( Mood.Length > TcpChatProxy.MaxMoodLength )
						{
							return $"Mood can only be {TcpChatProxy.MaxMoodLength} characters";
						}
						break;
				}
				return null;
			}
		}

		public static readonly DependencyProperty MoodProperty =
			DependencyProperty.Register( "Mood", typeof( string ), typeof( SetMoodWindow ), new PropertyMetadata( "" ) );


		public SetMoodWindow( ChatUser user )
		{
			InitializeComponent();

			Mood = user.Mood;
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			txtMood.Focus();
			txtMood.SelectAll();
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			txtMood.GetBindingExpression( TextBox.TextProperty ).UpdateSource();

			if( txtMood.Text.Length > TcpChatProxy.MaxMoodLength )
			{
				MedConfigMessageBox.ShowError( $"Mood is too long.  Mood can only be {TcpChatProxy.MaxMoodLength} characters" );
				return;
			}
			ApplicationViewModel.Current.ChatProxy.SetMood( Mood );
			Close();
		}

		private static void Invoke( Action action, [CallerMemberName]string name = null )
		{
			if( Application.Current != null && ApplicationViewModel.Current != null )
			{
				Application.Current.Dispatcher.Invoke(
					DispatcherPriority.Normal,
					(Action)delegate ()
					{
						action();
					} );
			}
		}
	}
}
