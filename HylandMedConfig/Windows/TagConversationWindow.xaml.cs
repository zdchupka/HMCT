using HylandMedConfig.Common;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace HylandMedConfig.Windows
{
	public partial class TagConversationWindow : Window
	{




        public List<ChatUser> FixedToUsers
        {
            get { return (List<ChatUser>)GetValue(FixedToUsersProperty); }
            set { SetValue(FixedToUsersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FixedToUsers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FixedToUsersProperty =
            DependencyProperty.Register("FixedToUsers", typeof(List<ChatUser>), typeof(TagConversationWindow), new PropertyMetadata(null));



        public string FilterTag
		{
			get { return (string)GetValue( FilterTagProperty ); }
			set { SetValue( FilterTagProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for FilterTag.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FilterTagProperty =
			DependencyProperty.Register( "FilterTag", typeof( string ), typeof( TagConversationWindow ), new PropertyMetadata( "" ) );


		public ICollectionView FilteredMessagesView
		{
			get;
			private set;
		}


		public TagConversationWindow()
		{
			InitializeComponent();
		}

        public TagConversationWindow(ApplicationViewModel viewModel, string tag)
            : this(viewModel)
        {
            FilterTag = tag;
            
			FilteredMessagesView.Filter = messagesView_Filter;

        }

        public TagConversationWindow( ApplicationViewModel viewModel, List<ChatUser> users )
            : this(viewModel)
		{
            FixedToUsers = users;
            
			
			FilteredMessagesView.Filter = messagesViewUser_Filter;

		}

        private TagConversationWindow(ApplicationViewModel viewModel)
        {
            DataContext = viewModel;
            FilteredMessagesView = new ListCollectionView( viewModel.AllMessages );

			InitializeComponent();
        }

		private bool messagesView_Filter( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			return message != null && message.Tags.Contains( FilterTag );
		}

        private bool messagesViewUser_Filter( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

            List<ChatUser> users = message.ToUsers.Union(new List<ChatUser> { message.FromUser }).Where(u => u != ApplicationViewModel.Current.ChatProxy.CurrentUser).ToList();
            List<ChatUser> users2 = FixedToUsers.Union(new List<ChatUser> { message.FromUser }).Where(u => u != ApplicationViewModel.Current.ChatProxy.CurrentUser).ToList();

            return message != null && users.SequenceEqualAnyOrder(users2);
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
