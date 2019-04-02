using HylandMedConfig.Common;
using Microsoft.Win32;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Windows.Input;

namespace HylandMedConfig.Windows
{
	public partial class TroubleshootingWindow : Window
	{
		private static TroubleshootingWindow _window = null;

		private BulkObservableCollection<string> _activeMessages = new BulkObservableCollection<string>();

		public ICollectionView ActiveChatMessagesView
		{
			get;
			private set;
		}


		public TroubleshootingWindow()
		{
			//_activeMessages.AddRange( ApplicationViewModel.GetActiveInstances() );
			ActiveChatMessagesView = CollectionViewSource.GetDefaultView( _activeMessages );

			DataContext = this;

			InitializeComponent();
		}

		private void Button_Click_1( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			_activeMessages.Clear();

			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				//_activeMessages.AddRange( ApplicationViewModel.GetActiveInstances() );
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		public static void ShowTroubleshooting()
		{
			if( _window != null && _window.IsLoaded )
			{
				_window.Focus();
				_window.Activate();
			}
			else
			{
				_window = new TroubleshootingWindow() { Owner = Application.Current.MainWindow };
				_window.Show();
			}
		}

		private void Button_Click1( object sender, RoutedEventArgs e )
		{
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}
	}
}
