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
using HylandMedConfig.Attached;

namespace HylandMedConfig.Windows
{
	/// <summary>
	/// Interaction logic for SetMoodWindow.xaml
	/// </summary>
	public partial class SetNicknameWindow : Window
	{
		public ICommand SetNicknameCommand
		{
			get;
			private set;
		}

		public string Nickname
		{
			get { return (string)GetValue( NicknameProperty ); }
			set { SetValue( NicknameProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Nickname.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty NicknameProperty =
			DependencyProperty.Register( "Nickname", typeof( string ), typeof( SetNicknameWindow ), new PropertyMetadata( "" ) );

		public ChatUser User
		{
			get;
			private set;
		}


		public SetNicknameWindow( ChatUser user )
		{
			SetNicknameCommand = new DelegateCommand( SetNicknameCommand_CanExecute, SetNicknameCommand_Execute );
			InitializeComponent();

			User = user;
		}

		private bool SetNicknameCommand_CanExecute( object obj )
		{
			return true;
		}

		private void SetNicknameCommand_Execute( object obj )
		{
			User.Nickname = Nickname;
			ApplicationViewModel.Current.ChatProxy.NicknameService.SaveNickname( User, Nickname );
			DialogResult = true;
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			txtNickname.Text = User.DisplayNameResolved;
			txtNickname.SelectAll();
			txtNickname.Focus();
		}
	}
}
