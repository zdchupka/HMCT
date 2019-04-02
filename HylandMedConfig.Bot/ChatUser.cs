using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HylandMedConfig.Common.Properties;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class ChatUser : ViewModelBase
	{
		#region Declarations
		private UserStatus _status = UserStatus.Offline;
		private bool _isTyping = false;
		private bool _isFoozReady = false;
		private bool _isRemote = false;
		private readonly string _username;
		private readonly string _displayName;
		private readonly string _imageUrl;
		private readonly bool _isBot;
		private string _nickname;
		private string _mood;
		private string _botCreatorUserName;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the username
		/// </summary>
		public string Username
		{
			get
			{
				return _username;
			}
		}

		/// <summary>
		/// Gets the nickname for this user
		/// </summary>
		public string Nickname
		{
			get
			{
				return _nickname;
			}
			internal set
			{
				_nickname = value;
				OnPropertyChanged();
				OnPropertyChanged( nameof( DisplayNameResolved ) );
			}
		}

		/// <summary>
		/// Gets the display name for the user
		/// </summary>
		public string DisplayName
		{
			get
			{
				return _displayName;
			}
		}

		public string DisplayNameResolved
		{
			get
			{
				if( !string.IsNullOrWhiteSpace( _nickname ) )
				{
					return _nickname;
				}
				return _displayName;
			}
		}

		/// <summary>
		/// Gets the imageurl for the user
		/// </summary>
		public string ImageUrl
		{
			get
			{
				return _imageUrl;
			}
		}

		/// <summary>
		/// Gets whether or not this user is online (status is Active or Inactive)
		/// </summary>
		public bool IsOnline
		{
			get
			{
				return _status == UserStatus.Active || _status == UserStatus.Inactive;
			}
		}

		public bool IsActive
		{
			get
			{
				return _status == UserStatus.Active;
			}
		}

		/// <summary>
		/// Gets whether this user is ready for fooz
		/// </summary>
		public bool IsFoozReady
		{
			get
			{
				return _isFoozReady;
			}
			internal set
			{
				_isFoozReady = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets whether or not this user is remote
		/// </summary>
		public bool IsRemote
		{
			get
			{
				return _isRemote;
			}
			internal set
			{
				_isRemote = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the user's online status (active, inactive, offline)
		/// </summary>
		public UserStatus Status
		{
			get
			{
				return _status;
			}
			internal set
			{
				_status = value;
				OnPropertyChanged();
				OnPropertyChanged( "IsOnline" );
			}
		}

		/// <summary>
		/// Gets a value indicating if this user is typing
		/// </summary>
		public bool IsTyping
		{
			get
			{
				return _isTyping;
			}
			internal set
			{
				_isTyping = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets a boolean indicating if this user is a bot
		/// </summary>
		public bool IsBot
		{
			get
			{
				return _isBot;
			}
		}

		/// <summary>
		/// Gets your mood
		/// </summary>
		public string Mood
		{
			get
			{
				return _mood;
			}
			internal set
			{
				_mood = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the username that created this bot (if it is a bot)
		/// </summary>
		public string BotCreatorUserName
		{
			get
			{
				return _botCreatorUserName;
			}
		}

		/// <summary>
		/// Gets the version of the client
		/// </summary>
		public Version ClientVersion
		{
			get;
			private set;
		}
		#endregion

		#region Construction

		internal ChatUser( string username, string displayName, string imageUrl, bool isBot = false )
			: this( username, displayName, displayName, imageUrl, isBot )
		{

		}

		internal ChatUser( string username, string displayName, string nickname, string imageUrl, bool isBot = false, string botCreatorUserName = "" )
		{
			if( string.IsNullOrWhiteSpace( username ) || username.Length > 30 )
			{
				throw new ArgumentException( "username cannot be empty and must be less than 30 characters", username );
			}

			if( username.Contains( " " ) )
			{
				throw new ArgumentException( "username cannot contain spaceFINs" );
			}

			if( string.IsNullOrWhiteSpace( displayName ) || displayName.Length > 35 )
			{
				throw new ArgumentException( "displayName cannot be empty and must be less than 35 characters", displayName );
			}

			_botCreatorUserName = botCreatorUserName;
			_isBot = isBot;
			_displayName = displayName.Trim();
			_imageUrl = imageUrl;
			_username = username.Trim().ToLower();
		}

		#endregion

		internal void SetClientVersion( Version version )
		{
			ClientVersion = version;
		}

		public static bool operator ==( ChatUser x, ChatUser y )
		{
			if( (object)x == null && (object)y == null )
			{
				return true;
			}
			else if( (object)x == null || (object)y == null )
			{
				return false;
			}
			else
			{
				return x.Equals( y );
			}
		}
		public static bool operator !=( ChatUser x, ChatUser y )
		{
			return !( x == y );
		}

		public override string ToString()
		{
			return DisplayName;
		}

		public override bool Equals( object obj )
		{
			if( obj is ChatUser )
			{
				return Username.Equals( ( obj as ChatUser ).Username );
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Username.GetHashCode();
		}
	}

	[Serializable]
	public class ChatUserStats
	{
		public string Username { get; set; }
		public DateTime? LastLogin { get; set; }
		public int PublicMessagesSent { get; set; }
		public int PrivateMessagesSent { get; set; }
		public int PublicMessagesReceived { get; set; }
		public int PrivateMessagesReceived { get; set; }
		public int ThumbsUpReceived { get; set; }
		public int ThumbsDownReceived { get; set; }
		public int ThumbsUpGiven { get; set; }
		public int ThumbsDownGiven { get; set; }
		public int FoozGameAttempts { get; set; }
		public int FoozGamesRegistered { get; set; }
		public int ChangedMood { get; set; }

		public ChatUserStats()
		{
		}
	}
}
