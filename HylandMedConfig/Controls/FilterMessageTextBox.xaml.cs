using HylandMedConfig.Common;
using HylandMedConfig.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace HylandMedConfig.Controls
{
	/// <summary>
	/// Interaction logic for MessageTextBox.xaml
	/// </summary>
	public partial class FilteredMessageTextBox : UserControl
	{
		private const string ImageShare = @"\\one-021619\ClipboardImages$";
		public ICommand ToggleSmileyCommand
		{
			get;
			private set;
		}

        public List<ChatUser> FixedToUsers
        {
            get { return (List<ChatUser>)GetValue(FixedToUsersProperty); }
            set { SetValue(FixedToUsersProperty, value); }
        }

        public static readonly DependencyProperty FixedToUsersProperty =
            DependencyProperty.Register("FixedToUsers", typeof(List<ChatUser>), typeof(FilteredMessageTextBox), new PropertyMetadata(null));

        public string FixedFilterTag
		{
			get { return (string)GetValue( FixedFilterTagProperty ); }
			set { SetValue( FixedFilterTagProperty, value ); }
		}

		public static readonly DependencyProperty FixedFilterTagProperty =
			DependencyProperty.Register( "FixedFilterTag", typeof( string ), typeof( FilteredMessageTextBox ), new PropertyMetadata( "" ) );

		public string Message
		{
			get { return (string)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( string ), typeof( FilteredMessageTextBox ), new FrameworkPropertyMetadata( ( s, a ) =>
			{
				FilteredMessageTextBox thisControl = s as FilteredMessageTextBox;
				thisControl.ComputedMessage = ChatUserMessage.Parse( ApplicationViewModel.Current.ChatProxy.CurrentUser, thisControl.Message, ApplicationViewModel.Current.ChatProxy.Users, thisControl.FixedFilterTag, thisControl.FixedToUsers );
			} ) );




		public ChatUserMessage ComputedMessage
		{
			get { return (ChatUserMessage)GetValue( ComputedMessageProperty ); }
			set { SetValue( ComputedMessageProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for ComputedMessage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ComputedMessageProperty =
			DependencyProperty.Register( "ComputedMessage", typeof( ChatUserMessage ), typeof( FilteredMessageTextBox ), new PropertyMetadata( null ) );




		public ApplicationViewModel ViewModel
		{
			get
			{
				return DataContext as ApplicationViewModel;
			}
		}

		public FilteredMessageTextBox()
		{
			ToggleSmileyCommand = new DelegateCommand( ToggleSmileyCommand_Execute );

			InitializeComponent();

			ComputedMessage = new NormalChatUserMessage();

			ApplicationViewModel.Current.ChatProxy.MessageSentSuccessfully += ChatProxy_MessageSentSuccessfully;
		}

		private void ChatProxy_MessageSentSuccessfully( object sender, ChatReceivedEventArgs e )
		{
			if( e.Message.ID == _sentMessageID )
			{
				Message = string.Empty;
				_sentMessageID = Guid.Empty;
			}
		}

		private void ToggleSmileyCommand_Execute( object obj )
		{
			smileyButton.IsChecked = !smileyButton.IsChecked;

			if( !smileyButton.IsChecked.Value )
			{
				txtMessage.Focus();
			}
		}

		public void MoveCursorToEnd()
		{
			txtMessage.Focus();

			txtMessage.SelectionStart = txtMessage.Text.Length;
			//txtMessage2.CaretPosition = txtMessage.CaretPosition.DocumentEnd;
		}

		private Guid _sentMessageID = Guid.Empty;

		private void txtMessage_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag( ModifierKeys.Shift ) )
			{
				txtMessage.GetBindingExpression( TextBox.TextProperty ).UpdateSource();

				if( !string.IsNullOrWhiteSpace( txtMessage.Text ) )
				{
					ChatUserMessage message = ChatUserMessage.Parse( ApplicationViewModel.Current.ChatProxy.CurrentUser, txtMessage.Text, ApplicationViewModel.Current.ChatProxy.Users, FixedFilterTag, FixedToUsers );
					_sentMessageID = message.ID;
					ApplicationViewModel.Current.ChatProxy.SendMessage( message );
				}
				e.Handled = true;
			}
			else if( e.Key == Key.Up )
			{
				if( ViewModel.CopyPreviousMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyPreviousMessageCommand.Execute( null );
					txtMessage.CaretIndex = int.MaxValue;
					e.Handled = true;
				}
			}
			else if( e.Key == Key.Down )
			{
				if( ViewModel.CopyNextMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyNextMessageCommand.Execute( null );
				}
				txtMessage.CaretIndex = int.MaxValue;
				e.Handled = true;
			}
			else if( Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V )
			{
				if( PasteImageCommand_CanExecute( null ) )
				{
					PasteImageCommand_Execute( null );
					e.Handled = true;
				}
			}
		}

		private bool PasteImageCommand_CanExecute( object obj )
		{
			return ClipboardHelper.GetImageData() != null;
		}

		private void PasteImageCommand_Execute( object obj )
		{
			string fileName = Guid.NewGuid().ToString() + ".png";
			string fullPath = System.IO.Path.Combine( ApplicationViewModel.ClipboardImageShare, fileName );
			if( ClipboardHelper.SaveImageToFile( fullPath ) )
			{
				txtMessage.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
				if( ComputedMessage is WhiteboardChatMessage )
				{
					WhiteboardChatMessage message = new WhiteboardChatMessage
					{
						FromUser = ApplicationViewModel.Current.ChatProxy.CurrentUser,
						ImageUrl = fullPath,
					};
					Message = message.ToMessageString();
					_sentMessageID = ComputedMessage.ID;
					ApplicationViewModel.Current.ChatProxy.SendMessage( ComputedMessage );
				}
				else
				{
					ImageUrlChatMessage message = new ImageUrlChatMessage
					{
						FromUser = ApplicationViewModel.Current.ChatProxy.CurrentUser,
						ImageUrl = fullPath,
						ToUsers = ComputedMessage.ToUsers,
					};
					message.Tags.Add( FixedFilterTag );
					ApplicationViewModel.Current.ChatProxy.SendMessage( message );
				}
			}
		}

		private void EmoticonImage_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			EmoticonLibraryItem dataContext = ( sender as Border ).DataContext as EmoticonLibraryItem;

			txtMessage.InsertText( dataContext.Keys.First() );

			if( !Keyboard.IsKeyDown( Key.LeftCtrl ) && !Keyboard.IsKeyDown( Key.RightCtrl ) )
			{
				smileyButton.IsChecked = false;
				txtMessage.Focus();
			}
			e.Handled = true;
		}

		private void Popup_Opened( object sender, EventArgs e )
		{
			txtSmileyFilter.Focus();
		}

		private void Popup_Closed( object sender, EventArgs e )
		{
			txtSmileyFilter.Text = string.Empty;
		}

		private void ListBox_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.Key == Key.Enter || e.Key == Key.Return )
			{
				EmoticonLibraryItem current = ApplicationViewModel.Current.SmiliesView.CurrentItem as EmoticonLibraryItem;
				if( current != null )
				{
					txtMessage.InsertText( current.Keys.First() );
				}

				smileyButton.IsChecked = false;
				txtMessage.Focus();
				e.Handled = true;
			}
			else if( e.Key == Key.Escape )
			{
				smileyButton.IsChecked = false;
				txtMessage.Focus();
				e.Handled = true;
			}
		}

		private void memeButton_Click( object sender, RoutedEventArgs e )
		{
			MemeChatMessage message = ComputedMessage as MemeChatMessage;
			CreateMemeWindow window = new CreateMemeWindow() { Owner = Application.Current.MainWindow };
			if( message != null )
			{
				window.Message = message;
			}
			if( window.ShowDialog() ?? false )
			{
				Message = window.Message.ToMessageString();
				MoveCursorToEnd();
			}
		}

		private void gifButton_Click( object sender, RoutedEventArgs e )
		{
			InsertImageWindow window = new InsertImageWindow() { Owner = Application.Current.MainWindow };

			window.Closing += Window_Closing;
			window.Show();
			//if( window.ShowDialog() ?? false )
			//{
			//	ImageUrlChatMessage imageMessage = new ImageUrlChatMessage();
			//	imageMessage.ImageUrl = window.SelectedResult.LargeImageUrl;
			//	Message = imageMessage.ToMessageString();
			//	MoveCursorToEnd();
			//}
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			InsertImageWindow window = sender as InsertImageWindow;
			if( window.Accepted )
			{
				ImageUrlChatMessage imageMessage = new ImageUrlChatMessage();
				imageMessage.ImageUrl = window.SelectedResult.LargeImageUrl;
				Message = imageMessage.ToMessageString();
				MoveCursorToEnd();
			}
		}
	}
}
