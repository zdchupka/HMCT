using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HylandMedConfig.ChatMessages;
using HylandMedConfig.Common;
using HylandMedConfig.Controls;
using HylandMedConfig.Properties;
using HylandMedConfig.Services;
using HylandMedConfig.Windows;

namespace HylandMedConfig
{
	public class ApplicationViewModel : ViewModelBase, IDataErrorInfo, IDisposable
	{
		#region Declarations
		public const string ClipboardImageShare = @"\\one-021619\ClipboardImages$";
		private SpeechSynthesizer _speechSynth = new SpeechSynthesizer();
		private static readonly object _lock = new object();
		private bool _hideImages = false;
		private bool _hideMemes = false;
		private bool _hidePolls = false;
		private bool _hideWhiteboards = false;
		private bool _hideXaml = false;
		private MediaPlayer _mediaPlayer = new MediaPlayer();
		bool _shuttingDown = false;
		bool _disposed = false;
		private BulkObservableCollection<ChatUserMessage> _messages = new BulkObservableCollection<ChatUserMessage>
		{
			new SystemChatMessage(Resources.STR_WELCOME_MESSAGE),
		};
		private List<string> _sentMessages = new List<string>();
		private string _message = string.Empty;
		private Version _availableVersion = new Version();
		internal IChatProxy _chatProxy;
		private long _unreadMessageCount = 0;
		private bool _hideBotMessages = false;
		private ObservableCollection<string> _ignoredUsers = new ObservableCollection<string>();
		private bool _showPublicMessages = true;
		private bool _showPrivateMessages = true;
		private bool _isHidden = false;
		private bool? _isConnected = null;
		private string _smileyFilterText;
		private string _tagFilterText;
		private string _botFilterText;
		private ChatUserMessage _computedMessage = new NormalChatUserMessage();
		private ObservableCollection<TagFilter> _tags = new ObservableCollection<TagFilter>();
		private ObservableCollection<Brush> _brushes = new ObservableCollection<Brush>
		{
			new SolidColorBrush(Color.FromRgb(0, 0, 0)),
			new SolidColorBrush(Color.FromRgb(127, 127, 127)),
			new SolidColorBrush(Color.FromRgb(136, 0, 21)),
			new SolidColorBrush(Color.FromRgb(237, 28, 36)),
			new SolidColorBrush(Color.FromRgb(255, 127, 39)),
			new SolidColorBrush(Color.FromRgb(255, 242, 0)),
			new SolidColorBrush(Color.FromRgb(34, 177, 76)),
			new SolidColorBrush(Color.FromRgb(0, 162, 232)),
			new SolidColorBrush(Color.FromRgb(63, 72, 204)),
			new SolidColorBrush(Color.FromRgb(163, 73, 164)),

			new SolidColorBrush(Color.FromRgb(255, 255, 255)),
			new SolidColorBrush(Color.FromRgb(195, 195, 195)),
			new SolidColorBrush(Color.FromRgb(185, 122, 87)),
			new SolidColorBrush(Color.FromRgb(255, 174, 201)),
			new SolidColorBrush(Color.FromRgb(255, 201, 14)),
			new SolidColorBrush(Color.FromRgb(239, 228, 176)),
			new SolidColorBrush(Color.FromRgb(181, 230, 29)),
			new SolidColorBrush(Color.FromRgb(153, 217, 234)),
			new SolidColorBrush(Color.FromRgb(112, 146, 190)),
			new SolidColorBrush(Color.FromRgb(200, 191, 231)),
		};
		private ObservableCollection<double> _thicknesses = new ObservableCollection<double>
		{
			0.25d,
			0.5d,
			1d,
			2d,
			4d,
			8d,
			12d,
			16d,
		};

		private ObservableCollection<double> _opacities = new ObservableCollection<double>
		{
			0.25d,
			0.5d,
			0.75d,
			1d,
		};

		#endregion

		#region Events

		public event EventHandler MessagesChanged;
		private void OnMessagesChanged( EventArgs e )
		{
			MessagesChanged?.Invoke( this, e );
		}

		#endregion

		#region Properties

		public static ApplicationViewModel Current
		{
			get;
			set;
		}

		public ObservableCollection<ChatUserMessage> AllMessages
		{
			get
			{
				return _messages;
			}
		}

		public long FilterCount
		{
			get
			{
				var filteredMessages = _messages.Except( MessagesView.OfType<ChatUserMessage>() ).OfType<ChatUserMessage>();
				StringCollection ignoredUsers = Settings.Default.IgnoredUsers;

				long count = 0;
				foreach( ChatUserMessage message in filteredMessages )
				{
					// Do not count ignored users in the count
					if( !ignoredUsers.Contains( message.FromUser.Username ) )
					{
						count++;
					}
				}
				return count;
			}
		}

		/// <summary>
		/// Proxy used to communicate with the chat server
		/// </summary>
		public IChatProxy ChatProxy
		{
			get
			{
				return _chatProxy;
			}
		}

		public string SmileyFilterText
		{
			get
			{
				return _smileyFilterText;
			}
			set
			{
				_smileyFilterText = value;
				OnPropertyChanged();
				SmiliesView.Refresh();
			}
		}

		public string TagFilterText
		{
			get
			{
				return _tagFilterText;
			}
			set
			{
				_tagFilterText = value;
				OnPropertyChanged();
				TagsView.Refresh();
			}
		}

		public string BotFilterText
		{
			get
			{
				return _botFilterText;
			}
			set
			{
				_botFilterText = value;
				OnPropertyChanged();
				BotsView.Refresh();
			}
		}

		public ICollectionView SmiliesView
		{
			get;
			private set;
		}

		public ICollectionView TagsView
		{
			get;
			private set;
		}

		public ICollectionView BrushesView
		{
			get;
			private set;
		}

		public ICollectionView ThicknessesView
		{
			get;
			private set;
		}

		public ICollectionView OpacitiesView
		{
			get;
			private set;
		}

		public Brush CurrentBrush
		{
			get
			{
				Brush brush = BrushesView.CurrentItem as Brush;
				if( brush == null )
				{
					brush = Brushes.Black;
				}
				return brush;
			}
		}

		public double CurrentThickness
		{
			get
			{
				double thickness = 4d;
				if( ThicknessesView.CurrentItem != null )
				{
					thickness = (double)ThicknessesView.CurrentItem;
				}
				return thickness;
			}
		}

		public double CurrentOpacity
		{
			get
			{
				double opacity = 1d;
				if( OpacitiesView.CurrentItem != null )
				{
					opacity = (double)OpacitiesView.CurrentItem;
				}
				return opacity;
			}
		}

		public bool ShowPublicMessages
		{
			get
			{
				return _showPublicMessages;
			}
			set
			{
				_showPublicMessages = value;
				OnPropertyChanged();

				List<ChatUserMessage> messagesToRemove = _messages.OfType<ChatUserMessage>().Where( m => !m.IsWhisper ).ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		public bool ShowPrivateMessages
		{
			get
			{
				return _showPrivateMessages;
			}
			set
			{
				_showPrivateMessages = value;
				OnPropertyChanged();

				List<ChatUserMessage> messagesToRemove = _messages.OfType<ChatUserMessage>().Where( m => m.IsWhisper ).ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		public bool HideBotMessages
		{
			get
			{
				return _hideBotMessages;
			}
			set
			{
				_hideBotMessages = value;
				OnPropertyChanged();

				List<ChatUserMessage> messagesToRemove = _messages.OfType<ChatUserMessage>().Where( m => m.FromUser.IsBot ).ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		public bool HideXamlMessages
		{
			get
			{
				return _hideXaml;
			}
			set
			{
				_hideXaml = value;
				OnPropertyChanged();

				List<XamlChatMessage> messagesToRemove = _messages.OfType<XamlChatMessage>().ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		/// <summary>
		/// Gets a value that represents the installed version.  If not installed, will return null
		/// </summary>
		public Version Version
		{
			get
			{
				Version version = new Version();
				if( ApplicationDeployment.IsNetworkDeployed )
				{
					version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
				}
				return version;
			}
		}

		public bool? IsConnected
		{
			get
			{
				return _isConnected;
			}
			set
			{
				_isConnected = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the number of unread messages
		/// </summary>
		public long UnreadMessageCount
		{
			get
			{
				return _unreadMessageCount;
			}
			private set
			{
				_unreadMessageCount = value;
				OnPropertyChanged();
				Application.Current.MainWindow.UpdateTaskbarItemInfo( value );
			}
		}

		public ICollectionView SentMessagesView
		{
			get;
			private set;
		}

		public ICollectionView FoozUsersView
		{
			get;
			private set;
		}

		public ICollectionView IgnoredUsersView
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value that the current user will be sending
		/// </summary>
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				HandleTextChangedUserNotifications( _message, value );
				_message = value;
				OnPropertyChanged();
				_computedMessage = ChatUserMessage.Parse( _chatProxy.CurrentUser, Message, _chatProxy.Users );
				OnPropertyChanged( nameof( ApplicationViewModel.ComputedMessage ) );
			}
		}

		public ChatUserMessage ComputedMessage
		{
			get
			{
				return _computedMessage;
			}
		}

		/// <summary>
		/// Gets a list of users (online combined with offline)
		/// </summary>
		public ListCollectionView UsersView
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a list of online bots
		/// </summary>
		public ListCollectionView BotsView
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a list of users that are currently typing
		/// </summary>
		public ListCollectionView TypingUsersView
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a list of messages that have been sent and received (ordered by date)
		/// </summary>
		public ListCollectionView MessagesView
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a boolean indicating if images are shown
		/// </summary>
		public bool HideImages
		{
			get
			{
				return _hideImages;
			}
			set
			{
				_hideImages = value;
				OnPropertyChanged();

				List<ImageUrlChatMessage> messagesToRemove = _messages.OfType<ImageUrlChatMessage>().ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating if memes are shown
		/// </summary>
		public bool HideMemes
		{
			get
			{
				return _hideMemes;
			}
			set
			{
				_hideMemes = value;
				OnPropertyChanged();

				List<MemeChatMessage> messagesToRemove = _messages.OfType<MemeChatMessage>().ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating if polls are shown
		/// </summary>
		public bool HidePolls
		{
			get
			{
				return _hidePolls;
			}
			set
			{
				_hidePolls = value;
				OnPropertyChanged();

				List<PollMessage> messagesToRemove = _messages.OfType<PollMessage>().ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating if Whiteboards are shown
		/// </summary>
		public bool HideWhiteboards
		{
			get
			{
				return _hideWhiteboards;
			}
			set
			{
				_hideWhiteboards = value;
				OnPropertyChanged();

				List<WhiteboardChatMessage> messagesToRemove = _messages.OfType<WhiteboardChatMessage>().ToList();

				_messages.RemoveRange( messagesToRemove );
				_messages.AddRange( messagesToRemove );
			}
		}

		/// <summary>
		/// Gets a boolean indicating if the application is currently hidden
		/// </summary>
		public bool IsHidden
		{
			get
			{
				return _isHidden;
			}
			set
			{
				_isHidden = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Commands

		public ICommand PlayMessage
		{
			get;
			private set;
		}

		public ICommand SendMessage
		{
			get;
			private set;
		}

		public ICommand UpdateCommand
		{
			get;
			private set;
		}

		public ICommand ShutdownCommand
		{
			get;
			private set;
		}

		public ICommand CopyPreviousMessageCommand
		{
			get;
			private set;
		}

		public ICommand SetMoodCommand
		{
			get;
			private set;
		}

		public ICommand CopyMessageTextCommand
		{
			get;
			private set;
		}

		public ICommand CopyNextMessageCommand
		{
			get;
			private set;
		}

		public ICommand ClearMessagesCommand
		{
			get;
			private set;
		}

		public ICommand RemoveMessageCommand
		{
			get;
			private set;
		}

		public ICommand RemoveMessagesBeforeCommand
		{
			get;
			private set;
		}

		public ICommand StartWhisperToUsersCommand
		{
			get;
			private set;
		}

		public ICommand StartTaggedMessageCommand
		{
			get;
			private set;
		}

		public ICommand AddWhisperUserCommand
		{
			get;
			private set;
		}

		public ICommand ToggleFoozReadyCommand
		{
			get;
			private set;
		}

		public ICommand ToggleIgnoreUserCommand
		{
			get;
			private set;
		}

		public ICommand PasteImageCommand
		{
			get;
			private set;
		}

		public ICommand CheckForUpdateCommand
		{
			get;
			private set;
		}

		public ICommand ToggleHiddenCommand
		{
			get;
			private set;
		}

		public ICommand ShowTaggedMessageCommand
		{
			get;
			private set;
		}

		public ICommand VoteCommand
		{
			get;
			private set;
		}

		public ICommand ClosePollCommand
		{
			get;
			private set;
		}

		public ICommand ToggleLockWhiteboardCommand
		{
			get;
			private set;
		}

		public ICommand ClearWhiteboardCommand
		{
			get;
			private set;
		}

		public ICommand ToggleThumbsUpCommand
		{
			get;
			private set;
		}

		public ICommand ToggleThumbsDownCommand
		{
			get;
			private set;
		}

		public ICommand ReplyAllTagsCommand
		{
			get;
			private set;
		}

		public ICommand IgnoreTagCommand
		{
			get;
			private set;
		}

        public ICommand MuteTagCommand
		{
			get;
			private set;
		}

		public ICommand ClearTagMessagesCommand
		{
			get;
			private set;
		}

		public string Error
		{
			get
			{
				return null;
			}
		}

		public ICommand OpenTagInWindowCommand
		{
			get;
			private set;
		}

        public ICommand OpenWhisperInNewWindowCommand
        {
            get;
            private set;
        }

		public ICommand ShowChangeNicknameWindowCommand
		{
			get;
			private set;
		}

		public string this[string columnName]
		{
			get
			{
				switch( columnName )
				{
					case nameof( ApplicationViewModel.Message ):
						if( _computedMessage is IValidateMessage )
						{
							string error;
							if( !( _computedMessage as IValidateMessage ).Validate( out error ) )
							{
								return error;
							}
						}
						return null;
				}
				return null;
			}
		}

		#endregion

		#region Constructor(s)
		public ApplicationViewModel( IChatProxy chatProxy )
		{
			_chatProxy = chatProxy;

			IsConnected = _chatProxy.IsConnected;

			ChatProxy.MessageReceived += proxy_MessageReceived;
			ChatProxy.Reconnected += ChatProxy_Reconnected;
			ChatProxy.UserLoggedOn += proxy_UserLoggedOn;
			ChatProxy.UserLoggedOut += proxy_UserLoggedOut;
			ChatProxy.Disconnected += ChatProxy_Disconnected;
			ChatProxy.UserFoozReady += proxy_UserFoozReady;
			ChatProxy.UserNotFoozReady += proxy_UserNotFoozReady;
			ChatProxy.FoozGameReady += ChatProxy_FoozGameReady;
			ChatProxy.ChatUserStatsReceived += ChatProxy_ChatUserStatsReceived;
			ChatProxy.Error += ChatProxy_ErrorReceived;
			ChatProxy.UserVoted += ChatProxy_UserVoted;
			ChatProxy.PollClosed += ChatProxy_PollClosed;
			ChatProxy.MessageSentSuccessfully += ChatProxy_MessageSent;
			ChatProxy.UserRatedMessage += ChatProxy_UserRatedMessage;

			SmiliesView = CollectionViewSource.GetDefaultView( EmoticonLibrary.Current );
			SmiliesView.Filter = smiliesView_Filter;
			MessagesView = new ListCollectionView( _messages );
			MessagesView.Filter = messagesView_Filter;
			MessagesView.SortDescriptions.Add( new SortDescription( "Date", ListSortDirection.Ascending ) );
			_messages.CollectionChanged += _messages_CollectionChanged;

			TagsView = CollectionViewSource.GetDefaultView( _tags );
			TagsView.SortDescriptions.Add( new SortDescription( nameof( TagFilter.Tag ), ListSortDirection.Ascending ) );
			TagsView.Filter = tagsView_Filter;

			BrushesView = CollectionViewSource.GetDefaultView( _brushes );
			BrushesView.MoveCurrentToFirst();

			ThicknessesView = CollectionViewSource.GetDefaultView( _thicknesses );
			ThicknessesView.MoveCurrentTo( 4d );

			OpacitiesView = CollectionViewSource.GetDefaultView( _opacities );
			OpacitiesView.MoveCurrentTo( 1d );

			TypingUsersView = new ListCollectionView( _chatProxy.Users );
			TypingUsersView.Filter = typingUsersView_Filter;
			UsersView = new ListCollectionView( _chatProxy.Users );
			UsersView.CustomSort = new ChatUserComparer( _chatProxy.CurrentUser );
			UsersView.Filter = UsersView_Filter;
			SentMessagesView = new ListCollectionView( _sentMessages );
			FoozUsersView = new ListCollectionView( _chatProxy.Users );
			FoozUsersView.Filter = foozUsersView_Filter;

			BotsView = new ListCollectionView( _chatProxy.Users );
			BotsView.Filter = BotUsersView_Filter;
			BotsView.CustomSort = new ChatUserComparer( _chatProxy.CurrentUser );

			SendMessage = new DelegateCommand( SendMessage_CanExecute, SendMessage_Execute );
			UpdateCommand = new DelegateCommand( UpdateCommand_Execute );
			ShutdownCommand = new DelegateCommand( Shutdown_Execute );
			CopyPreviousMessageCommand = new DelegateCommand( CopyLastMessageCommand_CanExecute, CopyLastMessageCommand_Execute );
			CopyMessageTextCommand = new DelegateCommand( CopyMessageTextCommand_CanExecute, CopyMessageTextCommand_Execute );
			CopyNextMessageCommand = new DelegateCommand( CopyNextMessageCommand_CanExecute, CopyNextMessageCommand_Execute );
			RemoveMessageCommand = new DelegateCommand( RemoveMessageCommand_CanExecute, RemoveMessageCommand_Execute );
			RemoveMessagesBeforeCommand = new DelegateCommand( RemoveMessagesBeforeCommand_CanExecute, RemoveMessagesBeforeCommand_Execute );
			StartWhisperToUsersCommand = new DelegateCommand( StartWhisperToUserCommand_CanExecute, StartWhisperToUserCommand_Execute );
			AddWhisperUserCommand = new DelegateCommand( AddWhisperUserCommand_CanExecute, AddWhisperUserCommand_Execute );
			ToggleFoozReadyCommand = new DelegateCommand( ToggleFoozReadyCommand_Execute );
			ClearMessagesCommand = new DelegateCommand( ClearMessagesCommand_CanExecute, ClearMessagesCommand_Execute );
			PlayMessage = new DelegateCommand( PlayMessage_CanExecute, PlayMessage_Execute );
			ToggleIgnoreUserCommand = new DelegateCommand( ToggleIgnoreUserCommand_CanExecute, ToggleIgnoreUserCommand_Execute );
			PasteImageCommand = new DelegateCommand( PasteImageCommand_CanExecute, PasteImageCommand_Execute );
			CheckForUpdateCommand = new DelegateCommand( CheckForUpdate_Execute );
			SetMoodCommand = new DelegateCommand( SetMoodCommand_Execute );
			ToggleHiddenCommand = new DelegateCommand( ToggleHiddenCommand_Execute );
			VoteCommand = new DelegateCommand( VoteCommand_CanExecute, VoteCommand_Execute );
			ClosePollCommand = new DelegateCommand( ClosePollCommand_CanExecute, ClosePollCommand_Execute );
			ToggleThumbsUpCommand = new DelegateCommand( ToggleThumbsUpCommand_CanExecute, ToggleThumbsUpCommand_Execute );
			ToggleThumbsDownCommand = new DelegateCommand( ToggleThumbsDownCommand_CanExecute, ToggleThumbsDownCommand_Execute );
			StartTaggedMessageCommand = new DelegateCommand( StartTaggedMessageCommand_CanExecute, StartTaggedMessageCommand_Execute );
			IgnoreTagCommand = new DelegateCommand( IgnoreTagCommand_Execute );
            MuteTagCommand = new DelegateCommand(MuteTagCommand_Execute);
			ClearTagMessagesCommand = new DelegateCommand( ClearTagMessages_CanExecute, ClearTagMessages_Execute );
			ToggleLockWhiteboardCommand = new DelegateCommand( ToggleLockWhiteboardCommand_CanExecute, ToggleLockWhiteboardCommand_Execute );
			ClearWhiteboardCommand = new DelegateCommand( ClearWhiteboardCommand_CanExecute, ClearWhiteboardCommand_Execute );
			OpenTagInWindowCommand = new DelegateCommand( OpenTagInWindowCommand_CanExecute, OpenTagInWindowCommand_Execute );
			ShowChangeNicknameWindowCommand = new DelegateCommand( ShowChangeNicknameWindowCommand_CanExecute, ShowChangeNicknameWindowCommand_Execute );
			ShowTaggedMessageCommand = new DelegateCommand( ShowTaggedMessageCommand_CanExecute, ShowTaggedMessageCommand_Execute );
			ReplyAllTagsCommand = new DelegateCommand( ReplyAllTagsCommand_CanExecute, ReplyAllTagsCommand_Execute );
            OpenWhisperInNewWindowCommand = new DelegateCommand(OpenWhisperInNewWindowCommand_CanExecute, OpenWhisperInNewWindowCommand_Execute);

			IgnoredUsersView = CollectionViewSource.GetDefaultView( _ignoredUsers );

			if( Settings.Default.IgnoredUsers != null )
			{
				foreach( string user in Settings.Default.IgnoredUsers )
				{
					_ignoredUsers.Add( user );
				}
			}

			_speechSynth.SetOutputToDefaultAudioDevice();

#if DEBUG
			//List<Tuple<string, string>> debugMessages = new List<Tuple<string, string>>
			//{
			//	new Tuple<string, string>("jferrin","this is a test"),
			//	new Tuple<string, string>("tstocker","this is a test"),
			//	new Tuple<string, string>("jchagnon","this is a test"),
			//	new Tuple<string, string>("tkielbasa","this is a test"),
			//	new Tuple<string, string>("ggiannone","this is a test"),
			//	new Tuple<string, string>("zchupka","this is a test"),
			//	new Tuple<string, string>("jferrin","this is a test"),
			//	new Tuple<string, string>("tstocker","this is a test"),
			//	new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//	//new Tuple<string, string>("jferrin","this is a test"),
			//	//new Tuple<string, string>("tstocker","this is a test"),
			//	//new Tuple<string, string>("jchagnon","this is a test"),
			//	//new Tuple<string, string>("tkielbasa","this is a test"),
			//	//new Tuple<string, string>("ggiannone","this is a test"),
			//	//new Tuple<string, string>("zchupka","this is a test"),
			//};
			//CreateTestMessages( debugMessages );
#endif
		}

		private void CreateTestMessages( List<Tuple<string, string>> messages )
		{
			foreach( Tuple<string, string> message in messages )
			{
				NormalChatUserMessage testMessage = new NormalChatUserMessage { FromUser = _chatProxy.Users.FirstOrDefault( u => u.Username == message.Item1 ), Text = message.Item2 };
				AddMessage( testMessage );
				( (TcpChatProxy)_chatProxy ).Messages.Add( testMessage );
			}
		}

		private void ChatProxy_UserRatedMessage( object sender, UserRatedMessagedEventArgs e )
		{
			if( Settings.Default.EnableNotifications && Settings.Default.UseThumbsUpRingtone && e.Rating == MessageRating.ThumbsUp && e.Message.FromUser == ChatProxy.CurrentUser )
			{
				try
				{
					_mediaPlayer.Open( new Uri( Settings.Default.ThumbsUpUrl, UriKind.Absolute ) );
					_mediaPlayer.Play();
				}
				catch
				{ }
			}

			if( Settings.Default.EnableNotifications && Settings.Default.UseThumbsDownRingtone && e.Rating == MessageRating.ThumbsDown && e.Message.FromUser == ChatProxy.CurrentUser )
			{
				try
				{
					_mediaPlayer.Open( new Uri( Settings.Default.ThumbsDownUrl, UriKind.Absolute ) );
					_mediaPlayer.Play();
				}
				catch
				{ }
			}
		}

		private void IgnoreTagCommand_Execute( object obj )
		{
			TagFilter tag = _tags.FirstOrDefault( t => t.Tag.Equals( System.Convert.ToString( obj ) ) );
			if( tag != null )
			{
				tag.Show = false;
				RefreshTagFilters( tag );
			}
		}

        public TagFilter GetTag(string tagname)
        {
            return _tags.FirstOrDefault(t => t.Tag.Equals(tagname));
        }

        private void MuteTagCommand_Execute(object obj)
        {
            TagFilter tag = _tags.FirstOrDefault( t => t.Tag.Equals( System.Convert.ToString( obj ) ) );
            if (tag != null)
            {
                tag.IsMuted = !tag.IsMuted;
            }
        }

		private bool ClearTagMessages_CanExecute( object obj )
		{
			return obj is TagFilter;
		}

		private void ClearTagMessages_Execute( object obj )
		{
			TagFilter tag = obj as TagFilter;
			List<ChatUserMessage> messagesToRemove = tag.FilteredMessagesView.OfType<ChatUserMessage>().ToList();
			_messages.RemoveRange( messagesToRemove );
		}



		private void OpenTagInWindowCommand_Execute( object obj )
		{
			string tag = System.Convert.ToString( obj );
			foreach( TagConversationWindow window in Application.Current.Windows.OfType<TagConversationWindow>() )
			{
				if( tag == window.FilterTag )
				{
					window.Activate();
					return;
				}
			}

			string title = string.Format( "#{1} - {0}", Application.Current.MainWindow.Title, tag );
			TagConversationWindow window2 = new TagConversationWindow( this, tag ) { Title = title };
			window2.Show();
		}

		private bool ShowChangeNicknameWindowCommand_CanExecute( object obj )
		{
			return obj is ChatUser;
		}

		private void ShowChangeNicknameWindowCommand_Execute( object obj )
		{
			ChatUser user = obj as ChatUser;
			SetNicknameWindow window = new SetNicknameWindow( user )
			{
				Owner = Application.Current.MainWindow,
			};
			if( window.ShowDialog() ?? false )
			{
				if( user.IsBot )
				{
					BotsView.Refresh();
				}
				else
				{
					UsersView.Refresh();
				}
			}
		}

		private void ShowTaggedMessageCommand_Execute( object obj )
		{
			ChatUserMessage msg = obj as ChatUserMessage;
			msg.IsHidden = false;
		}

		private bool ShowTaggedMessageCommand_CanExecute( object obj )
		{
			ChatUserMessage msg = obj as ChatUserMessage;
			return msg != null && msg.IsHidden;
		}

        private bool OpenWhisperInNewWindowCommand_CanExecute(object obj)
        {
            return obj is ChatUser || obj is ChatUserMessage;
        }

        private void OpenWhisperInNewWindowCommand_Execute(object obj)
        {
            if (obj is ChatUser)
            {
                ChatUser user = obj as ChatUser;
                List<ChatUser> users = new List<ChatUser> { user };

                // TODO find existing window
                //         string tag = System.Convert.ToString( obj );
                foreach (TagConversationWindow window2 in Application.Current.Windows.OfType<TagConversationWindow>())
                {
                    if (users.SequenceEqual(window2.FixedToUsers))
                    {
                        window2.Activate();
                        return;
                    }
                }

                TagConversationWindow window = new TagConversationWindow(this, new List<ChatUser> { user }) { Title = user.DisplayName };
                window.Show();
            }
            else if(obj is ChatUserMessage)
            {
                ChatUserMessage message = obj as ChatUserMessage;

                List<ChatUser> users = message.ToUsers.Union(new List<ChatUser> { message.FromUser }).Where(u => u != ApplicationViewModel.Current.ChatProxy.CurrentUser).ToList();

                foreach (TagConversationWindow window2 in Application.Current.Windows.OfType<TagConversationWindow>())
                {
                    if (users.SequenceEqualAnyOrder(window2.FixedToUsers))
                    {
                        window2.Activate();
                        return;
                    }
                }

                
                string title = string.Join( ", ", users.Select( u => u.DisplayNameResolved ).OrderBy( u => u ) );

                TagConversationWindow window = new TagConversationWindow(this, users) { Title = title };
                window.Show();
            }
        }

		private bool OpenTagInWindowCommand_CanExecute( object obj )
		{
			return obj != null && !string.IsNullOrEmpty( obj.ToString() );
		}

		private void ChatProxy_MessageSent( object sender, ChatReceivedEventArgs e )
		{
			AddMessage( e.Message );

			if( Settings.Default.AutomaticallyClean )
			{
				ClearMessagesCommand.Execute( Settings.Default.KeepLastMessages );
			}

			_sentMessages.Add( Message );
			SentMessagesView.MoveCurrentToLast();
			SentMessagesView.MoveCurrentToNext();

			foreach( string tag in e.Message.Tags )
			{
				TagFilter existingFilter = _tags.FirstOrDefault( t => t.Tag.Equals( tag ) );
				if( existingFilter == null )
				{
					_tags.Add( new TagFilter( _messages, tag ) );
				}
				else
				{
					if( !existingFilter.Show )
					{
						existingFilter.Show = true;
						RefreshTagFilters( existingFilter );
					}
				}
			}

			if( Application.Current.MainWindow.IsActive )
			{
				// Automatically check / uncheck the box when doing /me +1
				if( e.Message is UserStatusMessage && Regex.IsMatch( e.Message.Text, @"^\+[1-9999999]", RegexOptions.IgnoreCase ) )
				{
					ChatProxy.NotifyFoozReady();
				}
				else if( e.Message is UserStatusMessage && Regex.IsMatch( e.Message.Text, @"^\-[1-9999999]", RegexOptions.IgnoreCase ) )
				{
					ChatProxy.NotifyNotFoozReady();
				}

				ScrollToBottom();

				Message = string.Empty;

                if(!string.IsNullOrWhiteSpace(_previousMessageToRestore))
                {
                    Message = _previousMessageToRestore;
                    _previousMessageToRestore = string.Empty;
                }
				if( e.Message.Tags.Count > 0 )
				{
					StartTaggedMessageCommand.Execute( e.Message.Tags );
				}
				else if( e.Message.ToUsers != null && e.Message.ToUsers.Count > 0 )
				{
					StartWhisperToUsersCommand.Execute( e.Message.ToUsers );
				}
			}
		}

        private string _previousMessageToRestore = string.Empty;

		private void ChatProxy_UserVoted( object sender, UserVotedEventArgs e )
		{
			e.Poll.Vote( e.User, e.Choice.ID );
		}

		private void ChatProxy_PollClosed( object sender, PollClosedEventArgs e )
		{
			e.Message.Close();

			AddMessage( new PollClosedMessage( e.Message ) );
		}

		private void ChatProxy_Reconnected( object sender, EventArgs e )
		{
			if( !IsConnected ?? false )
			{
				AddMessage( new SystemChatMessage( string.Format( Resources.STR_SERVER_UP, DateTime.Now ) ) );
				ShowNotificationInternal( SystemChatMessage.SystemUser, "Connected" );
			}
			UsersView?.Refresh();
			BotsView?.Refresh();
			IsConnected = true;
		}

		private void ChatProxy_ErrorReceived( object sender, ChatErrorEventArgs e )
		{
			MedConfigMessageBox.ShowError( e.Message );
		}

		private void ToggleFoozReadyCommand_Execute( object obj )
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				if( ChatProxy.CurrentUser.IsFoozReady )
				{
					ChatProxy.NotifyNotFoozReady();
				}
				else
				{
					ChatProxy.NotifyFoozReady();
				}
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private void ChatProxy_ChatUserStatsReceived( object sender, ChatUserStatsEventArgs e )
		{
			_sentMessages.Add( Message );
			SentMessagesView.MoveCurrentToLast();
			SentMessagesView.MoveCurrentToNext();

			List<string> tags;
			string junk;
			ChatUserMessage.TryGetTags( Message, out tags, out junk );

			Message = string.Empty;

			if( tags.Count > 0 )
			{
				StartTaggedMessageCommand.Execute( tags );
			}

			EmployeeSearchResult employee = EmployeeSearchHelper.GetAndoverEmployees().FirstOrDefault( emp => emp.NetworkLogon == e.User.Username );
			AddMessage( new ChatUserStatsMessage( e.User, e.Stats, employee ) );
			ScrollToBottom();
		}

		private bool PlayMessage_CanExecute( object args )
		{
			return args is ChatUserMessage;
		}

		private void PlayMessage_Execute( object args )
		{
			ChatUserMessage message = args as ChatUserMessage;

			_speechSynth.SpeakAsync( message.GetSpeechText() );
		}

		private bool ToggleIgnoreUserCommand_CanExecute( object obj )
		{
			return obj is ChatUser;
		}

		private void ToggleIgnoreUserCommand_Execute( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( Settings.Default.IgnoredUsers.Contains( user.Username ) )
			{
				Settings.Default.IgnoredUsers.Remove( user.Username );
				_ignoredUsers.Remove( user.Username );
			}
			else
			{
				Settings.Default.IgnoredUsers.Add( user.Username );
				_ignoredUsers.Add( user.Username );
			}

			Settings.Default.Save();
			TypingUsersView.Refresh();

			List<ChatUserMessage> messagesToRemove = _messages.Where( m => m.FromUser == user ).ToList();
			_messages.RemoveRange( messagesToRemove );
			_messages.AddRange( messagesToRemove );

		}

		private bool PasteImageCommand_CanExecute( object obj )
		{
			return ClipboardHelper.GetImageData() != null;
		}

		private void PasteImageCommand_Execute( object obj )
		{
			string fileName = Guid.NewGuid().ToString() + ".png";
			string fullPath = System.IO.Path.Combine( ClipboardImageShare, fileName );
			if( ClipboardHelper.SaveImageToFile( fullPath ) )
			{
                _previousMessageToRestore = Message;
				MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
				TextBox textBoxMesage = txtMessage.FindName( "txtMessage" ) as TextBox;
				textBoxMesage.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
				if( ComputedMessage is WhiteboardChatMessage )
				{
					ComputedMessage.Text = fullPath;
					Message = ComputedMessage.ToMessageString();
				}
				else if( ComputedMessage is ImageUrlChatMessage )
				{
					ComputedMessage.Text = fullPath;
					Message = ComputedMessage.ToMessageString();
				}
				else
				{
					ImageUrlChatMessage message = new ImageUrlChatMessage { FromUser = ChatProxy.CurrentUser, ImageUrl = fullPath, ToUsers = ComputedMessage.ToUsers, Tags = ComputedMessage.Tags };
					Message = message.ToMessageString();
				}
				SendMessage.Execute( null );
				txtMessage.MoveCursorToEnd();
			}
		}

		private bool ClearMessagesCommand_CanExecute( object obj )
		{
			int messagesToKeep;
			bool isNumber = int.TryParse( System.Convert.ToString( obj ), out messagesToKeep );
			return isNumber && MessagesView.Count > messagesToKeep;
		}

		private void ClearMessagesCommand_Execute( object obj )
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				int messageToKeep;
				int.TryParse( System.Convert.ToString( obj ), out messageToKeep );

				if( messageToKeep == 0 )
				{
					_messages.Clear();
					_sentMessages.Clear();
				}
				else
				{
					int indexOfMarker = MessagesView.Count - messageToKeep;

					if( indexOfMarker >= 0 )
					{
						ChatUserMessage markerMessage = MessagesView.GetItemAt( indexOfMarker ) as ChatUserMessage;
						if( markerMessage != null )
						{
							var messagesToDelete = _messages.Where( m => m.Date < markerMessage.Date ).ToList();

							_messages.RemoveRange( messagesToDelete );

							if( messageToKeep <= 0 )
							{
								_sentMessages.Clear();
							}
						}
					}
				}

				IntPtr pHandle = NativeMethods.GetCurrentProcess();
				NativeMethods.SetProcessWorkingSetSize( pHandle, -1, -1 );
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private void ToggleLockWhiteboardCommand_Execute( object obj )
		{
			WhiteboardChatMessage message = obj as WhiteboardChatMessage;
			if( message.IsLocked )
			{
				ChatProxy.UnlockWhiteboard( message );
			}
			else
			{
				ChatProxy.LockWhiteboard( message );
			}
		}

		private bool ToggleLockWhiteboardCommand_CanExecute( object obj )
		{
			WhiteboardChatMessage message = obj as WhiteboardChatMessage;
			return message != null && message.FromUser == ChatProxy.CurrentUser;
		}

		private void ClearWhiteboardCommand_Execute( object obj )
		{
			WhiteboardChatMessage message = obj as WhiteboardChatMessage;
			ChatProxy.ClearWhiteboard( message );
		}

		private bool ClearWhiteboardCommand_CanExecute( object obj )
		{
			WhiteboardChatMessage message = obj as WhiteboardChatMessage;
			return message != null && message.FromUser == ChatProxy.CurrentUser && message.Entries.Count > 0;
		}

		private bool AddWhisperUserCommand_CanExecute( object obj )
		{
			return obj is ChatUser;
		}

		private void AddWhisperUserCommand_Execute( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( user == null ) return;

			ChatUserMessage message = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );
			message.Tags.Clear();
			if( !message.ToUsers.Contains( user ) )
			{
				message.ToUsers.Add( user );
			}
			else
			{
				message.ToUsers.Remove( user );
			}
			Message = message.ToMessageString();

			MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
			txtMessage.MoveCursorToEnd();
		}

		private bool StartTaggedMessageCommand_CanExecute( object obj )
		{
			return !string.IsNullOrEmpty( System.Convert.ToString( obj ) );
		}

		private void StartTaggedMessageCommand_Execute( object obj )
		{
			List<string> tags = obj as List<string>;
			if( tags != null )
			{
				ChatUserMessage message = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );
				message.Tags = tags;
				Message = message.ToMessageString();

				MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
				txtMessage.MoveCursorToEnd();
			}
			else
			{
				string tag = System.Convert.ToString( obj );
				if( !string.IsNullOrWhiteSpace( tag ) )
				{
					ChatUserMessage message = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );

					if( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
					{
						if( !message.Tags.Contains( tag ) )
						{
							message.Tags.Add( tag );
						}
						else
						{
							message.Tags.Remove( tag );
						}
					}
					else
					{
						message.Tags.Clear();
						message.Tags.Add( tag );
					}

					Message = message.ToMessageString();

					MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
					txtMessage.MoveCursorToEnd();
				}
			}
		}

		private bool ReplyAllTagsCommand_CanExecute( object obj )
		{
			return obj is ChatUserMessage;
		}

		private void ReplyAllTagsCommand_Execute( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			ChatUserMessage currentMessage = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );
			currentMessage.Tags.Clear();
			currentMessage.Tags.AddRange( message.Tags );

			Message = currentMessage.ToMessageString();

			MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
			txtMessage.MoveCursorToEnd();
		}

		private bool StartWhisperToUserCommand_CanExecute( object obj )
		{
			return obj is List<ChatUser> || obj is ChatUser;
		}

		private void StartWhisperToUserCommand_Execute( object obj )
		{
			List<ChatUser> users = obj as List<ChatUser>;
			ChatUser user = obj as ChatUser;

			if( user != null )
			{
				users = new List<ChatUser> { user };
			}

			if( users != null )
			{
				ChatUserMessage message = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );
				message.Tags.Clear();
				message.ToUsers = users;
				Message = message.ToMessageString();

				MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
				txtMessage.MoveCursorToEnd();
			}
		}

		private void RemoveMessageCommand_Execute( object obj )
		{
			_messages.Remove( obj as ChatUserMessage );

			IntPtr pHandle = NativeMethods.GetCurrentProcess();
			NativeMethods.SetProcessWorkingSetSize( pHandle, -1, -1 );
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private bool RemoveMessageCommand_CanExecute( object obj )
		{
			return obj is ChatUserMessage;
		}

		private bool RemoveMessagesBeforeCommand_CanExecute( object obj )
		{
			return obj is ChatUserMessage;
		}

		private void RemoveMessagesBeforeCommand_Execute( object obj )
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				ChatUserMessage message = obj as ChatUserMessage;

				var messagesToRemove = _messages.Where( m => m.Date < message.Date ).ToList();
				_messages.RemoveRange( messagesToRemove );

				IntPtr pHandle = NativeMethods.GetCurrentProcess();
				NativeMethods.SetProcessWorkingSetSize( pHandle, -1, -1 );
				GC.Collect();
				GC.WaitForPendingFinalizers();

				ScrollToTop();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private string GetWhisperUserName( string msg )
		{
			foreach( ChatUser user in ChatProxy.Users )
			{
				string pattern = string.Format( @"^@{0}\s", user.Username );
				if( Regex.IsMatch( msg, pattern, RegexOptions.IgnoreCase ) )
				{
					return user.Username;
				}
			}
			return null;
		}

		#endregion

		#region Methods

		public void RefreshTagFilters( TagFilter filter )
		{
			List<ChatUserMessage> messagesToRemove = _messages.OfType<ChatUserMessage>().Where( m => m.Tags.Contains( filter.Tag ) ).ToList();

			_messages.RemoveRange( messagesToRemove );
			_messages.AddRange( messagesToRemove );
		}

		private void ChatProxy_FoozGameReady( object sender, FoosballGameReadyEventArgs e )
		{
			AddMessage( new FoozReadyChatMessage( e.YellowOne, e.YellowTwo, e.BlackOne, e.BlackTwo ) );
			ShowNotificationInternal( null, Resources.STR_FOOZ_GAME_READY );
		}

		private bool typingUsersView_Filter( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( user != null && ChatProxy.CurrentUser != null )
			{
				return
					user.IsTyping &&
					!Settings.Default.IgnoredUsers.Contains( user.Username ) &&
					user.Status == UserStatus.Active &&
					user.Username != ChatProxy.CurrentUser.Username;
			}
			return false;
		}

		private bool tagsView_Filter( object obj )
		{
			TagFilter mapping = obj as TagFilter;
			if( mapping != null )
			{
				if( string.IsNullOrWhiteSpace( TagFilterText ) )
				{
					return true;
				}
				return mapping.Tag.Contains( TagFilterText );
			}
			return false;
		}

		private bool smiliesView_Filter( object obj )
		{
			EmoticonLibraryItem mapping = obj as EmoticonLibraryItem;
			if( mapping != null )
			{
				if( string.IsNullOrWhiteSpace( SmileyFilterText ) )
				{
					return true;
				}
				return mapping.Keys.FirstOrDefault( k => k.IndexOf( SmileyFilterText, StringComparison.CurrentCultureIgnoreCase ) >= 0 ) != null;
			}
			return false;
		}

		private bool foozUsersView_Filter( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( user != null )
			{
				return user.IsFoozReady;
			}
			return false;
		}

		private bool UsersView_Filter( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( user != null )
			{
				return !user.IsBot;
			}
			return false;
		}

		private bool BotUsersView_Filter( object obj )
		{
			ChatUser user = obj as ChatUser;
			if( user != null )
			{
				if( user.IsBot )
				{
					if( string.IsNullOrWhiteSpace( BotFilterText ) )
					{
						return true;
					}
					string botFilterTextLower = BotFilterText.ToLower();

					if( user.Nickname != null && user.Nickname.ToLower().Contains( botFilterTextLower ) )
					{
						return true;
					}
					if( user.DisplayName.ToLower().Contains( botFilterTextLower ) )
					{
						return true;
					}
					if( user.Username.ToLower().Contains( botFilterTextLower ) )
					{
						return true;
					}

					if( user.BotCreatorUserName.ToLower() == botFilterTextLower )
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool messagesView_Filter( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;
			if( message != null )
			{
				bool show = true;
				if( HideBotMessages && message.FromUser.IsBot )
				{
					show = false;
				}


				foreach( string tag in message.Tags )
				{
					if( _tags.Any( t => !t.Show ) )
					{
						TagFilter filter = _tags.FirstOrDefault( t => t.Tag.Equals( tag ) );
						if( filter != null && !filter.Show )
						{
							show = false;
							break;
						}
					}
				}

				if( message.FromUser != null && Settings.Default.IgnoredUsers.Contains( message.FromUser.Username ) )
				{
					show = false;
				}

				if( message.IsWhisper && !ShowPrivateMessages )
				{
					show = false;
				}

				if( !message.IsWhisper && !ShowPublicMessages )
				{
					show = false;
				}

				if( message is ImageUrlChatMessage && HideImages )
				{
					show = false;
				}

				if( message is MemeChatMessage && HideMemes )
				{
					show = false;
				}

				if( message is PollMessage && HidePolls )
				{
					show = false;
				}

				if( message is WhiteboardChatMessage && HideWhiteboards )
				{
					show = false;
				}

				if( message is XamlChatMessage && HideXamlMessages )
				{
					show = false;
				}
				return show;
			}
			return true;
		}

		private bool VoteCommand_CanExecute( object obj )
		{
			PollChoice choice = ( obj as PollChoice );
			if( choice != null )
			{
				PollMessage message = _messages.OfType<PollMessage>().FirstOrDefault( p => p.Choices.Select( c => c.ID ).Contains( choice.ID ) );
				if( message != null )
				{
					return message.IsOpen;
				}
			}
			return false;
		}

		private void VoteCommand_Execute( object obj )
		{
			PollChoice choice = obj as PollChoice;
			_chatProxy.Vote( choice );
		}

		private bool ClosePollCommand_CanExecute( object obj )
		{
			PollMessage message = obj as PollMessage;
			if( message != null )
			{
				return message.IsOpen && message.FromUser == _chatProxy.CurrentUser;
			}
			return false;
		}

		private void ClosePollCommand_Execute( object obj )
		{
			PollMessage message = obj as PollMessage;
			_chatProxy.ClosePoll( message );
		}

		private void ToggleThumbsDownCommand_Execute( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			if( message.VotesAgainst.Contains( _chatProxy.CurrentUser ) )
			{
				_chatProxy.RateMessage( message, MessageRating.Neutral );
			}
			else
			{
				_chatProxy.RateMessage( message, MessageRating.ThumbsDown );
			}
		}

		private bool ToggleThumbsDownCommand_CanExecute( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;
			return message != null;// && message.FromUser != _chatProxy.CurrentUser;
		}

		private void ToggleThumbsUpCommand_Execute( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			if( message.VotesFor.Contains( _chatProxy.CurrentUser ) )
			{
				_chatProxy.RateMessage( message, MessageRating.Neutral );
			}
			else
			{
				_chatProxy.RateMessage( message, MessageRating.ThumbsUp );
			}
		}

		private bool ToggleThumbsUpCommand_CanExecute( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;
			return message != null;// && message.FromUser != _chatProxy.CurrentUser;
		}

		private void ToggleHiddenCommand_Execute( object args )
		{
			if( IsHidden )
			{
				foreach( Window window in Application.Current.Windows )
				{
					if( window.IsLoaded )
					{
						if( window is SetMoodWindow )
						{
							window.ShowDialog();
						}
						else if( window is MainWindow )
						{
							window.Show();
							window.Activate();
							window.Focus();
							MessageTextBox txtMessage = Application.Current.MainWindow.FindName( "txtMessage" ) as MessageTextBox;
							txtMessage.MoveCursorToEnd();
						}
						else
						{
							window.Show();
						}
					}
				}
				IsHidden = false;
			}
			else
			{
				foreach( Window window in Application.Current.Windows )
				{
					if( window.IsLoaded )
					{
						window.Hide();
					}
				}
				IsHidden = true;
			}
		}

		private void SetMoodCommand_Execute( object args )
		{
			new SetMoodWindow( ChatProxy.CurrentUser )
			{
				Owner = Application.Current.MainWindow,
			}.ShowDialog();
		}

		private void CheckForUpdate_Execute( object args )
		{
			UpdateCheckInfo info = null;

			if( ApplicationDeployment.IsNetworkDeployed )
			{
				ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

				try
				{
					Mouse.OverrideCursor = Cursors.Wait;
					info = ad.CheckForDetailedUpdate();
				}
				catch( DeploymentDownloadException dde )
				{
					MedConfigMessageBox.ShowInfo( "The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message );
					return;
				}
				catch( InvalidDeploymentException ide )
				{
					MedConfigMessageBox.ShowInfo( "Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message );
					return;
				}
				catch( InvalidOperationException ioe )
				{
					MedConfigMessageBox.ShowInfo( "This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message );
					return;
				}
				finally
				{
					Mouse.OverrideCursor = null;
				}

				if( info.UpdateAvailable )
				{
					Boolean doUpdate = true;

					if( !info.IsUpdateRequired )
					{
						MessageBoxResult dr = MessageBox.Show( "An update is available. Would you like to update the application now?", "Update Available", MessageBoxButton.OKCancel );
						if( !( MessageBoxResult.OK == dr ) )
						{
							doUpdate = false;
						}
					}
					else
					{
						// Display a message that the app MUST reboot. Display the minimum required version.
						MedConfigMessageBox.ShowInfo( "This application has detected a mandatory update from your current " +
							"version to version " + info.MinimumRequiredVersion.ToString() +
							". The application will now install the update and restart.",
							"Update Available" );
					}

					if( doUpdate )
					{
						try
						{
							Mouse.OverrideCursor = Cursors.Wait;
							ad.Update();
							Application.Current.Restart();
						}
						catch( DeploymentDownloadException dde )
						{
							MedConfigMessageBox.ShowError( "Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde );
							return;
						}
						finally
						{
							Mouse.OverrideCursor = null;
						}
					}
				}
				else
				{
					MedConfigMessageBox.ShowInfo( "No update found!" );
				}
			}
			else
			{
				MedConfigMessageBox.ShowInfo( "This application is not deployed", "Info" );
			}
		}

		private void proxy_UserFoozReady( object sender, ChatUserEventArgs e )
		{
			ShowNotificationInternal( e.User, string.Format( Resources.STR_USER_READY_FOR_FOOZ, e.User.DisplayNameResolved ) );
		}

		private void proxy_UserNotFoozReady( object sender, ChatUserEventArgs e )
		{
			// Remove the last game ready message if it's less than 3 minutes ago
			FoozReadyChatMessage foozReadyMessage = _messages.OfType<FoozReadyChatMessage>().OrderBy( m => m.Date ).Where( m => m.Date > DateTime.Now.AddMinutes( -3 ) ).FirstOrDefault();
			if( foozReadyMessage != null )
			{
				_messages.Remove( foozReadyMessage );
			}
		}

		private void ChatProxy_Disconnected( object sender, EventArgs e )
		{
			if( IsConnected ?? false )
			{
				AddMessage( new SystemChatMessage( string.Format( Resources.STR_SERVER_DOWN, DateTime.Now ) ) );
				ShowNotificationInternal( SystemChatMessage.SystemUser, "Disconnected" );
			}
			IsConnected = false;
		}

		private void proxy_UserLoggedOut( object sender, ChatUserEventArgs e )
		{
			if( !e.User.IsBot )
			{
				ShowNotificationInternal( e.User, string.Format( Resources.STR_USER_LOGGED_OUT_NOTIFICATION, e.User.DisplayNameResolved ) );
				AddMessage( new SystemChatMessage( string.Format( Resources.STR_USER_LOGGED_OUT, e.User.DisplayNameResolved, DateTime.Now ) ), false );
			}
		}

		private void proxy_UserLoggedOn( object sender, ChatUserEventArgs e )
		{
			if( !e.User.IsBot )
			{
				ShowNotificationInternal( e.User, string.Format( Resources.STR_USER_LOGGED_ON_NOTIFICATION, e.User.DisplayNameResolved ) );
				AddMessage( new SystemChatMessage( string.Format( Resources.STR_USER_LOGGED_ON, e.User.DisplayNameResolved, DateTime.Now ) ), false );
			}
		}

		private void HandleTextChangedUserNotifications( string oldMessage, string newMessage )
		{
			string oldMessageText;
			string newMessageText;
			List<string> newTags;
			List<string> oldTags;

			// Remove tags
			ChatUserMessage.TryGetTags( newMessage, out newTags, out newMessageText );
			ChatUserMessage.TryGetTags( oldMessage, out oldTags, out oldMessageText );

			// When the user types in a new message, handle notifying users that the user has entered or cleared their text
			// This should also handle whisper messages
			List<ChatUser> newUsers = ChatUserMessage.GetWhisperUsers( newMessageText, ChatProxy.Users, out newMessageText );
			List<ChatUser> oldUsers = ChatUserMessage.GetWhisperUsers( oldMessageText, ChatProxy.Users, out oldMessageText );


			if( newUsers.Count == 0 )
			{
				newUsers = ChatProxy.Users.ToList();
			}
			if( oldUsers.Count == 0 )
			{
				oldUsers = ChatProxy.Users.ToList();
			}

			bool wasEmpty = string.IsNullOrWhiteSpace( oldMessageText );
			bool isEmpty = string.IsNullOrWhiteSpace( newMessageText );

			List<ChatUser> addedUsers = newUsers.Except( oldUsers ).ToList();
			List<ChatUser> removedUsers = oldUsers.Except( newUsers ).ToList();
			List<ChatUser> existingUsers = newUsers.Intersect( oldUsers ).ToList();

			bool clearedText = isEmpty && !wasEmpty;
			bool enteredText = !isEmpty && wasEmpty;

			if( addedUsers.Count > 0 )
			{
				if( isEmpty )
				{
					ChatProxy.NotifyTextCleared( addedUsers );
				}
				else if( !isEmpty )
				{
					ChatProxy.NotifyTextEntered( addedUsers );
				}
			}

			if( removedUsers.Count > 0 )
			{
				ChatProxy.NotifyTextCleared( removedUsers );
			}

			if( existingUsers.Count > 0 )
			{
				if( clearedText )
				{
					ChatProxy.NotifyTextCleared( existingUsers );
				}
				else if( enteredText )
				{
					ChatProxy.NotifyTextEntered( existingUsers );
				}
			}
		}

		private void ShowNotificationInternal( ChatUser user, string text )
		{
			if( ( user != null && Settings.Default.IgnoredUsers.Contains( user.Username ) ) ||
				IsHidden )
			{
				return;
			}

			if( Settings.Default.PlayAllMessages )
			{
				_speechSynth.SpeakAsync( text );
			}

			if( !Settings.Default.EnableNotifications )
			{
				return;
			}

			if( !IsAnyWindowActive() ||
				Application.Current.MainWindow.WindowState == WindowState.Minimized )
			{
				if( Settings.Default.ShowNotifications )
				{
					NotificationWindow.ShowUserNotification( user, text );
				}

				if( Settings.Default.TextToSpeechEnabled && !Settings.Default.PlayAllMessages )
				{
					_speechSynth.SpeakAsync( text );
				}

				if( Settings.Default.UseRingtone && File.Exists( Settings.Default.IMReceivedPath ) )
				{
					try
					{
						_mediaPlayer.Open( new Uri( Settings.Default.IMReceivedPath, UriKind.Absolute ) );
						_mediaPlayer.Play();
					}
					catch
					{ }
				}
			}
		}

		private static bool IsAnyWindowActive()
		{
			foreach( Window window in Application.Current.Windows )
			{
				if( !( window is NotificationWindow ) && window.IsActive )
				{
					return true;
				}
			}
			return false;
		}

		private string GetSpeechText( ChatUserMessage message )
		{
			StringBuilder sb = new StringBuilder( string.Format( "{0} ", message.FromUser.DisplayNameResolved ) );

			if( message is CombinedNormalChatUserMessages )
			{
				sb.Clear();
				StringBuilder text = new StringBuilder();
				sb.AppendLine( ( message as CombinedNormalChatUserMessages ).Messages.OrderBy( m => m.Date ).Last().Text );
			}
			else if( message is ASCIIChatMessage )
			{
				sb.Append( " says " );
				sb.Append( message.Text );
			}
			else if( message is HyperlinkChatMessage )
			{
				sb.Append( " sent a hyperlink " );
			}
			else if( message is ImageUrlChatMessage )
			{
				sb.Append( " sent an image " );
			}
			else if( message is MemeChatMessage )
			{
				sb.Append( " sent a meme " );
			}
			else if( message is VideoUrlChatMessage )
			{
				sb.Append( " sent a video " );
			}
			else if( message is XamlChatMessage )
			{
				sb.Append( " sent a xaml message " );
			}
			else if( message is WhiteboardChatMessage )
			{
				sb.Append( " sent a whiteboard " );
			}
			else if( message is PollMessage )
			{
				sb.Append( " sent a poll " );
			}
			else
			{
				if( message.ToUsers != null && message.ToUsers.Count > 0 )
				{
					sb.Append( " whispered " );
				}
				else
				{
					sb.Append( " says " );
				}
				sb.Append( message.Text );
			}
			return sb.ToString();
		}

		private void ShowNotificationInternal( ChatUserMessage message )
		{
			bool isFiltered = !MessagesView.Contains( message );

			if( Settings.Default.IgnoredUsers.Contains( message.FromUser.Username ) ||
				IsHidden ||
				isFiltered )
			{
				return;
			}

			if( Settings.Default.PlayAllMessages )
			{
				_speechSynth.SpeakAsync( GetSpeechText( message ) );
			}

			if( !Settings.Default.EnableNotifications )
			{
				return;
			}

			if( !IsAnyWindowActive() ||
			Application.Current.MainWindow.WindowState == WindowState.Minimized )
			{
				if( Settings.Default.ShowNotifications )
				{
					NotificationWindow.ShowUserNotification( message );
				}

				if( Settings.Default.TextToSpeechEnabled && !Settings.Default.PlayAllMessages )
				{
					_speechSynth.SpeakAsync( GetSpeechText( message ) );
				}

				if( Settings.Default.UseRingtone && File.Exists( Settings.Default.IMReceivedPath ) )
				{
					try
					{
						_mediaPlayer.Open( new Uri( Settings.Default.IMReceivedPath, UriKind.Absolute ) );
						_mediaPlayer.Play();
					}
					catch
					{ }
				}
			}
		}

		private void proxy_MessageReceived( object sender, ChatReceivedEventArgs e )
		{
			foreach( string tag in e.Message.Tags )
			{
				TagFilter existingFilter = _tags.FirstOrDefault( t => t.Tag.Equals( tag ) );
				if( existingFilter == null )
				{
					TagFilter filter = new TagFilter( _messages, tag );

					if( Settings.Default.IsTagIgnored( filter.Tag ) )
					{
						filter.Show = false;
					}
					else
					{
						filter.Show = true;
					}
					_tags.Add( filter );
				}
			}

			ChatUserMessage addedMessage = AddMessage( e.Message );
			ShowNotificationInternal( addedMessage );
			TypingUsersView.Refresh();

			if( Settings.Default.AutomaticallyClean )
			{
				ClearMessagesCommand.Execute( Settings.Default.KeepLastMessages );
			}
		}

		private void Invoke( Action action )
		{
			if( System.Windows.Application.Current != null && !_shuttingDown )
			{
				System.Windows.Application.Current.Dispatcher.Invoke(
					DispatcherPriority.Normal,
					(Action)delegate ()
					{
						action();
					} );
			}
		}

		private bool SendMessage_CanExecute( object args )
		{
			string newText;
			List<string> tags;
			ChatUserMessage.GetWhisperUsers( Message, ChatProxy.Users, out newText );
			ChatUserMessage.TryGetTags( newText, out tags, out newText );
			return !string.IsNullOrWhiteSpace( newText );
		}

		private void ScrollToBottom()
		{
			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			if( mainWindow != null )
			{
				mainWindow.ScrollMessagesToBottom();
			}
		}

		private void ScrollToTop()
		{
			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			if( mainWindow != null )
			{
				mainWindow.ScrollMessagesToTop();
			}
		}

		private void SendMessage_Execute( object args )
		{
			ChatUserMessage message = ChatUserMessage.Parse( ChatProxy.CurrentUser, Message, ChatProxy.Users );

			if( !message.IsWhisper && message.Text.Equals( "/help", StringComparison.InvariantCultureIgnoreCase ) )
			{
				_messages.Add( new HelpChatMessage() );
				_sentMessages.Add( Message );
				SentMessagesView.MoveCurrentToLast();
				SentMessagesView.MoveCurrentToNext();
				Message = string.Empty;
				if( message.Tags.Count > 0 )
				{
					StartTaggedMessageCommand.Execute( message.Tags );
				}
				ScrollToBottom();
				return;
			}

			if( !message.IsWhisper && message.Text.StartsWith( "/info", StringComparison.InvariantCultureIgnoreCase ) )
			{
				string[] values = message.Text.Split( new char[] { ' ' } );
                 if( values.Length >= 1 )
				{
                    string username = "";
                    if (values.Length > 1)
                    {
                        username = values[1];
                    }

                    if(string.IsNullOrWhiteSpace(username))
                    {
                        string[] employees = Settings.Default.CustomUserList.Cast<string>().ToArray();

                        if (employees.Length == 0)
                        {
                            HylandMedConfig.Windows.MedConfigMessageBox.ShowError("You have not configured your Out of Office list in the Settings");
                        }
                        else
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            try
                            {
                                var results = EmployeeSearchHelper.GetEmployees(employees);
                                _sentMessages.Add(Message);
                                SentMessagesView.MoveCurrentToLast();
                                SentMessagesView.MoveCurrentToNext();
                                Message = string.Empty;
                                if (message.Tags.Count > 0)
                                {
                                    StartTaggedMessageCommand.Execute(message.Tags);
                                }
                                if (results.Count() == 0)
                                {
                                    HylandMedConfig.Windows.MedConfigMessageBox.ShowError("No employees found, check your Out of Office list names");
                                }
                                else
                                {
                                    AddMessage(new ChatUserListStatsMessage(results));
                                    ScrollToBottom();
                                }
                            }
                            finally
                            {
                                Mouse.OverrideCursor = null;
                            }
                        }
                    }
					// If the username does not exist in our list of users, then just search the network, no need to request stats from server
					else if( ChatProxy.Users.FirstOrDefault( u => u.Username == username ) == null )
					{
						Mouse.OverrideCursor = Cursors.Wait;
						try
						{
							if( username.ToLower() == "andover" )
							{
								var employees = EmployeeSearchHelper.GetAndoverEmployees();

								_sentMessages.Add( Message );
								SentMessagesView.MoveCurrentToLast();
								SentMessagesView.MoveCurrentToNext();
								Message = string.Empty;
								if( message.Tags.Count > 0 )
								{
									StartTaggedMessageCommand.Execute( message.Tags );
								}
								AddMessage( new ChatUserListStatsMessage( employees ) );
								ScrollToBottom();
							}
                            else if( username.ToLower() == "hmct" )
							{
								List<EmployeeSearchResult> hmctEmployees = new List<EmployeeSearchResult>();
								var employees = EmployeeSearchHelper.GetAndoverEmployees();
								foreach( ChatUser user in _chatProxy.Users )
								{
									if( !user.IsBot )
									{
										EmployeeSearchResult employee = employees.FirstOrDefault( u => u.NetworkLogon == user.Username );
										if( employee != null )
										{
											hmctEmployees.Add( employee );
										}
									}
								}
								_sentMessages.Add( Message );
								SentMessagesView.MoveCurrentToLast();
								SentMessagesView.MoveCurrentToNext();
								Message = string.Empty;
								if( message.Tags.Count > 0 )
								{
									StartTaggedMessageCommand.Execute( message.Tags );
								}
								AddMessage( new ChatUserListStatsMessage( hmctEmployees ) );
								ScrollToBottom();
							}
							else
							{
								string firstname = null;
								string lastname = null;
								if( values.Length == 2 )
								{
									firstname = values[1].Substring( 0, 1 );
									lastname = values[1].Substring( 1, values[1].Length - 1 );
								}
								else if( values.Length == 3 )
								{
									firstname = values[1];
									lastname = values[2];
								}

								var employees = EmployeeSearchHelper.FindEmployee( firstname, lastname );

								if( employees.Count() > 4 )
								{
									MedConfigMessageBox.ShowError( "Found more than 4 users.  Please give a better name." );
								}
								else if( employees.Count() == 0 )
								{
									MedConfigMessageBox.ShowError( "Could not find any matching users" );
								}
								else
								{
									_sentMessages.Add( Message );
									SentMessagesView.MoveCurrentToLast();
									SentMessagesView.MoveCurrentToNext();
									Message = string.Empty;
									if( message.Tags.Count > 0 )
									{
										StartTaggedMessageCommand.Execute( message.Tags );
									}
									foreach( EmployeeSearchResult employee in employees )
									{
										AddMessage( new ChatUserStatsMessage( null, null, employee ) );
									}
									ScrollToBottom();
								}
							}
						}
						finally
						{
							Mouse.OverrideCursor = null;
						}
					}
					else
					{
						ChatProxy.RequestStats( username );
					}
					return;
				}
			}

			if( !message.IsWhisper && message.Text.StartsWith( "/mood " ) )
			{
				string mood = Message.Replace( "/mood ", string.Empty );

				ChatProxy.SetMood( mood );
				_sentMessages.Add( Message );
				SentMessagesView.MoveCurrentToLast();
				SentMessagesView.MoveCurrentToNext();

				Message = string.Empty;
				if( message.Tags.Count > 0 )
				{
					StartTaggedMessageCommand.Execute( message.Tags );
				}
				ScrollToBottom();

				return;
			}

			if( !message.IsWhisper && message.Text.Trim().Equals( "/clear", StringComparison.CurrentCultureIgnoreCase ) )
			{
				if( message.Tags.Count > 0 )
				{
					TagFilter tagFilter = _tags.FirstOrDefault( t => t.Tag == message.Tags[0] );
					if( ClearTagMessagesCommand.CanExecute( tagFilter ) )
					{
						ClearTagMessagesCommand.Execute( tagFilter );
						Message = string.Empty;
						StartTaggedMessageCommand.Execute( message.Tags );
					}
				}
				else
				{
					if( ClearMessagesCommand.CanExecute( 0 ) )
					{
						ClearMessagesCommand.Execute( 0 );
						Message = string.Empty;
					}
				}

				return;
			}

			if( !message.IsWhisper && message.Text.Trim().Equals( "/clean", StringComparison.CurrentCultureIgnoreCase ) )
			{
				if( ClearMessagesCommand.CanExecute( Settings.Default.KeepLastMessages ) )
				{
					ClearMessagesCommand.Execute( Settings.Default.KeepLastMessages );
				}
				Message = string.Empty;
				if( message.Tags.Count > 0 )
				{
					StartTaggedMessageCommand.Execute( message.Tags );
				}
				return;
			}

			// TODO: TEST!
			if( message is XamlChatMessage )
			{
				string error;
				if( !( message as XamlChatMessage ).ValidateXaml( out error ) )
				{
					MedConfigMessageBox.ShowError( error );
					return;
				}
			}

			ChatProxy.SendMessage( message );
		}

		private void UpdateCommand_Execute( object args )
		{
			if( ApplicationDeployment.CurrentDeployment.Update() )
			{
				Application.Current.Restart();
			}
		}

		private void Shutdown_Execute( object args )
		{
			Settings.Default.Save();
			Application.Current.Shutdown();

			Dispose();
		}

		// ******************************************************************
		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose( true );
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize( this );
		}

		// ******************************************************************
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be _disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be _disposed.
		protected virtual void Dispose( bool disposing )
		{
			_shuttingDown = true;
			IsConnected = false;

			// Check to see if Dispose has already been called.
			if( !this._disposed )
			{
				// If disposing equals true, dispose all managed
				// and unmanaged resources.
				if( disposing )
				{
					// Dispose managed resources.
					ChatProxy.Dispose();
					_speechSynth.Dispose();
				}

				// Note disposing has been done.
				_disposed = true;
			}
		}

		private bool CopyMessageTextCommand_CanExecute( object args )
		{
			return args is ChatUserMessage;
		}

		private void CopyMessageTextCommand_Execute( object args )
		{
			ChatUserMessage message = args as ChatUserMessage;

			if( message is CombinedNormalChatUserMessages )
			{
				StringBuilder sb = new StringBuilder();
				foreach( var m in ( message as CombinedNormalChatUserMessages ).Messages )
				{
					sb.AppendLine( m.ToMessageString( false ) );
				}
				Clipboard.SetDataObject( sb.ToString() );
			}
			else
			{
				Clipboard.SetDataObject( message.ToMessageString( false ) );
			}
		}

		private bool CopyLastMessageCommand_CanExecute( object args )
		{
			string newText;
			ChatUserMessage.GetWhisperUsers( Message, ChatProxy.Users, out newText );
			List<string> tags;
			ChatUserMessage.TryGetTags( newText, out tags, out newText );

			if( SentMessagesView.CurrentItem != null )
			{
				string text = SentMessagesView.CurrentItem.ToString();
				return string.IsNullOrWhiteSpace( newText ) || ( text == Message && SentMessagesView.CurrentPosition > 0 );
			}
			else
			{

				return !SentMessagesView.IsEmpty && string.IsNullOrWhiteSpace( newText );
			}
		}

		private void CopyLastMessageCommand_Execute( object args )
		{
			Invoke( () =>
			 {
				 string newText;
				 ChatUserMessage.GetWhisperUsers( Message, ChatProxy.Users, out newText );
				 bool isEmpty = string.IsNullOrWhiteSpace( newText );

				 if( isEmpty )
				 {
					 SentMessagesView.MoveCurrentToLast();
				 }
				 else
				 {
					 SentMessagesView.MoveCurrentToPrevious();
				 }
				 if( SentMessagesView.CurrentItem != null )
				 {
					 Message = SentMessagesView.CurrentItem.ToString();
				 }
			 } );
		}

		private bool CopyNextMessageCommand_CanExecute( object args )
		{
			if( SentMessagesView.CurrentItem != null )
			{
				string text = SentMessagesView.CurrentItem.ToString();
				return text == Message && SentMessagesView.CurrentPosition < _sentMessages.Count - 1;
			}
			return false;
		}

		private void CopyNextMessageCommand_Execute( object args )
		{
			Invoke( () =>
			 {
				 SentMessagesView.MoveCurrentToNext();
				 if( SentMessagesView.CurrentItem != null )
				 {
					 Message = SentMessagesView.CurrentItem.ToString();
				 }
			 } );
		}

		private ChatUserMessage AddMessage( ChatUserMessage message, bool increaseUnreadMessageCount = true )
		{
			bool appended = false;
			bool isIgnored = false;
			ChatUserMessage messageAdded = message;

			if( message is NormalChatUserMessage )
			{
				NormalChatUserMessage chatMessage = message as NormalChatUserMessage;
				NormalChatUserMessage lastMessage = _messages.LastOrDefault() as NormalChatUserMessage;
				CombinedNormalChatUserMessages lastMessages = _messages.LastOrDefault() as CombinedNormalChatUserMessages;

				if( lastMessage != null &&
					!chatMessage.IsHidden &&
					lastMessage.FromUser == chatMessage.FromUser &&
					lastMessage.IsWhisper == chatMessage.IsWhisper &&
					lastMessage.Tags.SequenceEqual( chatMessage.Tags ) &&
					!EmoticonRichTextBox.GetIsBigEmoji( lastMessage.Text ) &&
					!EmoticonRichTextBox.GetIsBigEmoji( chatMessage.Text ) &&
					lastMessage.ToUsers.SequenceEqual( chatMessage.ToUsers ) )
				{
					// If the last message was by the same user and was less than 15 seconds ago, just add
					// the message to the latest one
					if( ( DateTime.Now - lastMessage.Date ).TotalSeconds < 15 )
					{
						_messages.Remove( lastMessage );
						CombinedNormalChatUserMessages combinedMessage = new CombinedNormalChatUserMessages( lastMessage );
						combinedMessage.AddMessage( chatMessage );
						_messages.Add( combinedMessage );
						messageAdded = combinedMessage;
						appended = true;
					}
				}

				if( lastMessages != null &&
					!chatMessage.IsHidden &&
					lastMessages.FromUser == chatMessage.FromUser &&
					lastMessages.IsWhisper == chatMessage.IsWhisper &&
					lastMessages.Tags.SequenceEqual( chatMessage.Tags ) &&
					!EmoticonRichTextBox.GetIsBigEmoji( chatMessage.Text ) &&
					lastMessages.ToUsers.SequenceEqual( chatMessage.ToUsers ) )
				{
					// If the last message was by the same user and was less than 15 seconds ago, just add
					// the message to the latest one
					if( ( DateTime.Now - lastMessages.Messages[0].Date ).TotalSeconds < 15 )
					{
						lastMessages.AddMessage( chatMessage );
						_messages.Remove( lastMessages );
						_messages.Add( lastMessages );
						messageAdded = lastMessages;
						appended = true;
					}
				}
			}

			if( message.FromUser != null )
			{
				isIgnored = Settings.Default.IgnoredUsers.Contains( message.FromUser.Username );
			}

			if( !appended )
			{
				_messages.Add( message );
			}

			bool isFiltered = !MessagesView.Contains( messageAdded );

            bool isMuted = false;
            foreach(string tag in messageAdded.Tags)
            {
                if(GetTag(tag)?.IsMuted ?? false)
                {
                    isMuted = true;
                    break;
                }
            }

			// Do not increase message count for system messages
			if( increaseUnreadMessageCount && Application.Current.MainWindow != null && !isIgnored && !isFiltered && !isMuted )
			{
				// Increase the unread count if the window is not active and this is a regular message
				if( Application.Current.MainWindow.WindowState == WindowState.Minimized ||
					!IsAnyWindowActive() )
				{
					UnreadMessageCount++;
				}
			}
			return messageAdded;
		}

		private void _messages_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			OnMessagesChanged( EventArgs.Empty );
			OnPropertyChanged( nameof( ApplicationViewModel.FilterCount ) );
			CommandManager.InvalidateRequerySuggested();
		}

		#endregion

		#region Public Methods

		public void NotifyActive()
		{
			ChatProxy.NotifyActive();
			UnreadMessageCount = 0;
		}

		#endregion
	}

	public class TagFilter : ViewModelBase
	{
		private bool _show = true;
		private string _tag = string.Empty;
        private bool _isMuted = false;

		public string Tag
		{
			get
			{
				return _tag;
			}
			private set
			{
				_tag = value;
				OnPropertyChanged();
			}
		}

		public bool Show
		{
			get
			{
				return _show;
			}
			set
			{
				_show = value;
				OnPropertyChanged();
			}
		}

        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }
            set
            {
                _isMuted = value;
                OnPropertyChanged();
            }
        }

		public ICollectionView PreviewFilteredMessagesView
		{
			get;
			private set;
		}

		public ICollectionView FilteredMessagesView
		{
			get;
			private set;
		}

		public TagFilter( ObservableCollection<ChatUserMessage> messages, string tag )
		{
			Tag = tag;
			FilteredMessagesView = new ListCollectionView( messages );
			PreviewFilteredMessagesView = new ListCollectionView( messages );
			FilteredMessagesView.Filter = new Predicate<object>( Filter_Messages );
			PreviewFilteredMessagesView.Filter = new Predicate<object>( FilterPreview_Messages );
			messages.CollectionChanged += Messages_CollectionChanged;
		}

		private void Messages_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			FilteredMessagesView.Refresh();
			PreviewFilteredMessagesView.Refresh();
		}

		private bool Filter_Messages( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			if( message != null )
			{
				return message.Tags.Contains( Tag );
			}
			return false;
		}

		private bool FilterPreview_Messages( object obj )
		{
			ChatUserMessage message = obj as ChatUserMessage;

			if( message != null )
			{
				int indexOfFilteredMessage = FilteredMessagesView.OfType<ChatUserMessage>().ToList().IndexOf( message );
				if( indexOfFilteredMessage < 0 )
				{
					return false;
				}
				int index = FilteredMessagesView.OfType<ChatUserMessage>().Count() - indexOfFilteredMessage;
				return index < 11;
			}
			return false;
		}
	}
}

