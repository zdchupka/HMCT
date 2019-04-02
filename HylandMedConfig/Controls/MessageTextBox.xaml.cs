using HylandMedConfig.Common;
using HylandMedConfig.Windows;
using System;
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
	public partial class MessageTextBox : UserControl
	{
		private const string ImageShare = @"\\one-021619\ClipboardImages$";
		public ICommand ToggleSmileyCommand
		{
			get;
			private set;
		}

		public ApplicationViewModel ViewModel
		{
			get
			{
				return DataContext as ApplicationViewModel;
			}
		}

		public MessageTextBox()
		{
			ToggleSmileyCommand = new DelegateCommand( ToggleSmileyCommand_Execute );

			InitializeComponent();
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

		private void txtMessage2_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag( ModifierKeys.Shift ) )
			{
				//ViewModel.Message = txtMessage2.GetRawText();
				if( ViewModel.SendMessage.CanExecute( null ) )
				{
					ViewModel.SendMessage.Execute( null );
				}
				e.Handled = true;
			}
			else if( e.Key == Key.Up )
			{
				if( ViewModel.CopyPreviousMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyPreviousMessageCommand.Execute( null );
					txtMessage.SelectionStart = txtMessage.Text.Length;
					//txtMessage2.CaretPosition = txtMessage2.CaretPosition.DocumentEnd;
					e.Handled = true;
				}
			}
			else if( e.Key == Key.Down )
			{
				if( txtMessage.SelectionStart == txtMessage.Text.Length && ViewModel.CopyNextMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyNextMessageCommand.Execute( null );
					txtMessage.SelectionStart = txtMessage.Text.Length;
					//txtMessage2.CaretPosition = txtMessage2.CaretPosition.DocumentEnd;
					e.Handled = true;
				}
			}
			else if( Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V )
			{
				if( ApplicationViewModel.Current.PasteImageCommand.CanExecute( null ) )
				{
					ApplicationViewModel.Current.PasteImageCommand.Execute( null );
					e.Handled = true;
				}
			}
		}

		private void txtMessage_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag( ModifierKeys.Shift ) )
			{
				txtMessage.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
				if( ViewModel.SendMessage.CanExecute( null ) )
				{
					ViewModel.SendMessage.Execute( null );
				}
				e.Handled = true;
			}
			else if( e.Key == Key.Up )
			{
				if( txtMessage.SelectionStart == txtMessage.Text.Length && ViewModel.CopyPreviousMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyPreviousMessageCommand.Execute( null );
					txtMessage.CaretIndex = int.MaxValue;
					e.Handled = true;
				}
			}
			else if( e.Key == Key.Down )
			{
				if( txtMessage.SelectionStart == txtMessage.Text.Length && ViewModel.CopyNextMessageCommand.CanExecute( null ) )
				{
					ViewModel.CopyNextMessageCommand.Execute( null );
					txtMessage.CaretIndex = int.MaxValue;
					e.Handled = true;
				}

			}
			else if( Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V )
			{
				if( ApplicationViewModel.Current.PasteImageCommand.CanExecute( null ) )
				{
					ApplicationViewModel.Current.PasteImageCommand.Execute( null );
					e.Handled = true;
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
			MemeChatMessage message = ApplicationViewModel.Current.ComputedMessage as MemeChatMessage;
			CreateMemeWindow window = new CreateMemeWindow() { Owner = Application.Current.MainWindow };
			if( message != null )
			{
				window.Message = message;
			}
			if( window.ShowDialog() ?? false )
			{
				if( ApplicationViewModel.Current.ComputedMessage.ToUsers.Count > 0 )
				{
					window.Message.ToUsers.Clear();
					window.Message.ToUsers.AddRange( ApplicationViewModel.Current.ComputedMessage.ToUsers );
				}
				window.Message.Tags = ApplicationViewModel.Current.ComputedMessage.Tags;
				ApplicationViewModel.Current.Message = window.Message.ToMessageString();
				MoveCursorToEnd();
			}
		}

		private void gifButton_Click( object sender, RoutedEventArgs e )
		{
			InsertImageWindow window = new InsertImageWindow() { Owner = Application.Current.MainWindow };
			window.Closed += Window_Closed;
			window.Show();
			//if( window.ShowDialog() ?? false )
			//{

			//}
		}

		private void Window_Closed( object sender, EventArgs e )
		{
			InsertImageWindow window = sender as InsertImageWindow;
			if( window.Accepted )
			{
				ImageUrlChatMessage imageMessage = new ImageUrlChatMessage();
				imageMessage.ImageUrl = window.SelectedResult.LargeImageUrl;
				if( ApplicationViewModel.Current.ComputedMessage.ToUsers.Count > 0 )
				{
					imageMessage.ToUsers.Clear();
					imageMessage.ToUsers.AddRange( ApplicationViewModel.Current.ComputedMessage.ToUsers );
				}
				imageMessage.Tags = ApplicationViewModel.Current.ComputedMessage.Tags;
				ApplicationViewModel.Current.Message = imageMessage.ToMessageString();
				MoveCursorToEnd();
			}
		}
	}
}
