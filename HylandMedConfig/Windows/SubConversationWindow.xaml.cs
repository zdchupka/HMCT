using HylandMedConfig.Common;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System;

namespace HylandMedConfig.Windows
{
	public partial class SubConversationWindow : Window
	{
		public ICollectionView FilteredMessagesView
		{
			get;
			private set;
		}

		public SubConversationWindow()
		{
			InitializeComponent();
		}

		public SubConversationWindow( ApplicationViewModel viewModel, Predicate<object> filter )
		{
			DataContext = viewModel;

			FilteredMessagesView = new ListCollectionView( viewModel.AllMessages );
			FilteredMessagesView.Filter = filter;

			InitializeComponent();
		}

		private void root_Loaded( object sender, RoutedEventArgs e )
		{
			txtMessage.txtMessage.Focus();
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			List<ChatUserMessage> messagesToRemove = FilteredMessagesView.OfType<ChatUserMessage>().ToList();
			messagesToRemove.ForEach( m => ApplicationViewModel.Current.AllMessages.Remove( m ) );
		}
	}
}
