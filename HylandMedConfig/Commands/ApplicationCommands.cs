using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using HylandMedConfig.Common;
using HylandMedConfig.Dialogs;
using System.Diagnostics;

namespace HylandMedConfig.Commands
{
	public static class ApplicationCommands
	{
		public static readonly ICommand ExportUserSettings = new RoutedCommand();
		public static readonly ICommand ImportUserSettings = new RoutedCommand();
		public static readonly ICommand CloseWindow = new CloseWindowCommand();
		public static readonly ICommand MinimizeWindow = new MinimizeWindowCommand();
		public static readonly ICommand MaximizeWindow = new MaximizeWindowCommand();
		public static readonly ICommand CopyTextToClipboard = new CopyTextToClipboardCommand();
		public static readonly ICommand OpenSettings = new OpenSettingsCommand();
		public static readonly ICommand ViewReleaseNotes = new ViewReleaseNotesCommand();
		public static readonly ICommand ViewBotAssemblies = new ViewBotAssembliesCommand();
		public static readonly ICommand ClearMessages = new RoutedCommand();
		public static readonly ICommand RestartCommand = new RestartCommand();
		public static readonly ICommand Feedback = new FeedbackCommand();
		public static readonly ICommand ImageSearchCommand = new RoutedCommand();
		public static readonly ICommand OKCommand = new RoutedCommand();
		public static readonly ICommand AddIgnoreTagCommand = new RoutedCommand();
		public static readonly ICommand RemoveIgnoreTagCommand = new RoutedCommand();
		public static readonly ICommand ClearTagMessagesCommand = new RoutedCommand();
		public static readonly ICommand ToggleLockWhiteboard = new RoutedCommand();


	}

	public class RestartCommand : ICommand
	{

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Application.Current.Restart();
		}
	}

	public class ViewBotAssembliesCommand : ICommand
	{

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Process.Start( @"\\one-021619\Share\HylandMedConfig" );
		}
	}

	public class FeedbackCommand : ICommand
	{
		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Process.Start( @"https://trello.com/b/a9gZCu4V" );
		}
	}

	public class ViewReleaseNotesCommand : ICommand
	{

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Process.Start( "ReleaseNotes.txt" );
		}
	}



	public class OpenSettingsCommand : ICommand
	{
		private static UserSettingsDialog _settingsDlg = new UserSettingsDialog();

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			if( _settingsDlg.IsLoaded )
			{
				_settingsDlg.Focus();
				_settingsDlg.Activate();
			}
			else
			{
				_settingsDlg = new UserSettingsDialog() { Owner = Application.Current.MainWindow };
				_settingsDlg.Show();
			}
		}
	}

	public class CopyTextToClipboardCommand : ICommand
	{
		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			if( parameter is ChatUserMessage )
			{
				Clipboard.SetDataObject( ( parameter as ChatUserMessage ).ToMessageString() );
			}
			else if( parameter != null )
			{
				Clipboard.SetDataObject( parameter.ToString() );
			}
		}
	}


	public class CloseWindowCommand : ICommand
	{
		public CloseWindowCommand()
		{

		}

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Window window = parameter as Window;
			if( window != null )
			{
				window.Close();
			}
		}
	}

	public class MinimizeWindowCommand : ICommand
	{
		public MinimizeWindowCommand()
		{

		}

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Window window = parameter as Window;
			if( window != null )
			{
				window.WindowState = WindowState.Minimized;
			}
		}
	}

	public class MaximizeWindowCommand : ICommand
	{
		public MaximizeWindowCommand()
		{
		}

		public bool CanExecute( object parameter )
		{
			Window window = parameter as Window;
			if( window != null )
			{
				return window.ResizeMode != ResizeMode.NoResize;
			}
			return false;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute( object parameter )
		{
			Window window = parameter as Window;
			if( window != null )
			{
				if( window.WindowState == WindowState.Maximized )
				{
					window.WindowState = WindowState.Normal;
				}
				else
				{
					window.WindowState = WindowState.Maximized;
				}
			}
		}
	}
}
