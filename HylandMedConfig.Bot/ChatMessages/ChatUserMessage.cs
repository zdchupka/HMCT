using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Data;

namespace HylandMedConfig.Common
{
	[Serializable]
	public abstract class ChatUserMessage : ViewModelBase
	{
		private string _text = string.Empty;
		private List<string> _tags = new List<string>();
		private List<ChatUser> _toUsers = new List<ChatUser>();
		private ChatUser _fromUser = null;
		private bool _isHidden = false;

		public ObservableCollection<ChatUser> VotesFor
		{
			get;
			private set;
		}

		public ObservableCollection<ChatUser> VotesAgainst
		{
			get;
			private set;
		}

		public bool IsHidden
		{
			get { return _isHidden; }
			set
			{
				_isHidden = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets or sets the user that sent the message
		/// </summary>
		public ChatUser FromUser
		{
			get
			{
				return _fromUser;
			}
			set
			{
				_fromUser = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the message identifier
		/// </summary>
		public Guid ID
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the datetime the message was sent
		/// </summary>
		public DateTime Date
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the command related to this message
		/// </summary>
		public virtual string Command
		{
			get { return Commands.NONE; }
		}

		/// <summary>
		/// Gets or sets the user the message was sent to (null if to everyone)
		/// </summary>
		public List<ChatUser> ToUsers
		{
			get
			{
				return _toUsers;
			}
			set
			{
				_toUsers = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets a bool that indicates if this message was a whisper
		/// </summary>
		public bool IsWhisper
		{
			get
			{
				return ToUsers != null && ToUsers.Count > 0;
			}
		}

		/// <summary>
		/// Gets the text of the message
		/// </summary>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}

		public virtual string GetSpeechText()
		{
			return string.Format( "{0} says: {1}", FromUser.DisplayName, Text );
		}

		/// <summary>
		/// Gets or sets the Tag of the message
		/// </summary>
		public List<string> Tags
		{
			get
			{
				return _tags;
			}
			set
			{
				_tags = value;
				OnPropertyChanged();
			}
		}

		public ChatUserMessage()
		{
			Date = DateTime.Now;
			ID = Guid.NewGuid();
			VotesFor = new ObservableCollection<ChatUser>();
			VotesAgainst = new ObservableCollection<ChatUser>();
		}

		internal void ChangeRatingForUser( ChatUser user, MessageRating rating, out MessageRating previousRating )
		{
			switch( rating )
			{
				case MessageRating.Neutral:
					if( VotesAgainst.Contains( user ) )
					{
						previousRating = MessageRating.ThumbsDown;
						VotesAgainst.Remove( user );
					}
					else if( VotesFor.Contains( user ) )
					{
						previousRating = MessageRating.ThumbsUp;
						VotesFor.Remove( user );
					}
					else
					{
						previousRating = MessageRating.Neutral;
					}
					break;
				case MessageRating.ThumbsUp:
					if( VotesAgainst.Contains( user ) )
					{
						previousRating = MessageRating.ThumbsDown;
						VotesAgainst.Remove( user );
						VotesFor.Add( user );
					}
					else if( !VotesFor.Contains( user ) )
					{
						previousRating = MessageRating.Neutral;
						VotesFor.Add( user );
					}
					else
					{
						previousRating = MessageRating.ThumbsUp;
					}
					break;
				case MessageRating.ThumbsDown:
					if( VotesFor.Contains( user ) )
					{
						previousRating = MessageRating.ThumbsUp;
						VotesFor.Remove( user );
						VotesAgainst.Add( user );
					}
					else if( !VotesAgainst.Contains( user ) )
					{
						previousRating = MessageRating.Neutral;
						VotesAgainst.Add( user );
					}
					else
					{
						previousRating = MessageRating.ThumbsDown;
					}
					break;
				default:
					previousRating = MessageRating.Neutral;
					break;
			}
		}

		public virtual string ToMessageString( bool includeTag = true )
		{
			StringBuilder message = new StringBuilder();

			if( includeTag && Tags != null && Tags.Count > 0 )
			{
				foreach( string tag in Tags )
				{
					message.AppendFormat( "#{0} ", tag );
				}
			}
			else if( ToUsers != null )
			{
				foreach( ChatUser user in ToUsers )
				{
					message.AppendFormat( "@{0} ", user.Username );
				}
			}

			if( !string.IsNullOrEmpty( Command ) )
			{
				message.AppendFormat( "/{0} ", Command );
			}

			message.Append( Text );

			return message.ToString();
		}

		public override string ToString()
		{
			return string.Format( "{0}: {1}", FromUser, Text );
		}

		internal static class Commands
		{
			public const string NONE = "";
			public const string HTML = "html";
			public const string WEB = "web";
			public const string IMAGEURL = "imageurl";
			public const string VIDEOURL = "videourl";
			public const string HELP = "help";
			public const string STATUS_UPDATE = "me";
			public const string ASCII = "ascii";
			public const string XAML = "xaml";
			public const string MEME = "meme";
			public const string POLL = "poll";
			public const string LINK = "link";
			public const string WHITEBOARD = "whiteboard";
		}

		public static ChatUserMessage Parse( ChatUser fromUser, string text, IList<ChatUser> allUsers, string fixedTag, List<ChatUser> fixedUsers )
		{
			ChatUserMessage message = Parse( fromUser, text, allUsers, true );
            if (!string.IsNullOrEmpty(fixedTag))
            {
                message.Tags.Clear();
                message.Tags.Add(fixedTag);
            }
            if(fixedUsers != null && fixedUsers.Count > 0)
            {
                message.ToUsers.AddRange(fixedUsers);
            }
			return message;
		}


		private static ChatUserMessage Parse( ChatUser fromUser, string text, IList<ChatUser> allUsers, bool ignoreTagAndUsers )
		{
			List<string> tags = new List<string>();
			List<ChatUser> toUsers = new List<ChatUser>();

			if( !ignoreTagAndUsers )
			{
				// If the message is tagged, send to all users, don't even parse usernames
				if( TryGetTags( text, out tags, out text ) )
				{

				}
				else
				{
					toUsers = GetWhisperUsers( text, allUsers, out text );
				}
			}

			ChatUserMessage message = null;

			if( MatchesCommand( text, Commands.IMAGEURL ) )
			{
				string imageUrl = RemoveCommand( text, Commands.IMAGEURL );
				message = new ImageUrlChatMessage { ImageUrl = imageUrl };
			}
			else if( MatchesCommand( text, Commands.WEB ) )
			{
				string url = RemoveCommand( text, Commands.WEB );
				message = new WebUrlChatMessage { Url = url };
			}
			else if( MatchesCommand( text, Commands.STATUS_UPDATE ) )
			{
				string status = RemoveCommand( text, Commands.STATUS_UPDATE );
				message = new UserStatusMessage { Text = status };
			}
			else if( MatchesCommand( text, Commands.ASCII ) )
			{
				string asciiText = RemoveCommand( text, Commands.ASCII );
				message = new ASCIIChatMessage { Text = asciiText };
			}
			else if( MatchesCommand( text, Commands.XAML ) )
			{
				string xaml = RemoveCommand( text, Commands.XAML );
				message = new XamlChatMessage { Xaml = xaml };
			}
			else if( MatchesCommand( text, Commands.MEME ) )
			{
				string memeText = RemoveCommand( text, Commands.MEME );
				message = MemeChatMessage.FromString( memeText );
			}
			else if( MatchesCommand( text, Commands.POLL ) )
			{
				string pollText = RemoveCommand( text, Commands.POLL );
				message = PollMessage.FromString( pollText );
			}
			else if( MatchesCommand( text, Commands.LINK ) || HyperlinkUtility.IsHyperlink( text ) )
			{
				string url = RemoveCommand( text, Commands.LINK );
				message = new HyperlinkChatMessage( url );
			}
			else if( MatchesCommand( text, Commands.VIDEOURL ) )
			{
				string url = RemoveCommand( text, Commands.VIDEOURL );

				if( url.Contains( "https://" ) )
				{
					url = url.Replace( "https://", "http://" );
				}
				message = new VideoUrlChatMessage { VideoUrl = url };
			}
			else if( MatchesCommand( text, Commands.WHITEBOARD ) || text == "/" + Commands.WHITEBOARD )
			{
				if( text == "/" + Commands.WHITEBOARD )
				{
					text += " ";
				}
				string url = RemoveCommand( text, Commands.WHITEBOARD );
				message = new WhiteboardChatMessage { ImageUrl = url };
			}
			else
			{
				message = new NormalChatUserMessage { Text = text };
			}

			message.ToUsers = toUsers;
			message.FromUser = fromUser;
			message.Tags = tags;

			return message;
		}

		public static ChatUserMessage Parse( ChatUser fromUser, string text, IList<ChatUser> allUsers )
		{
			return Parse( fromUser, text, allUsers, false );
		}

		private static bool MatchesCommand( string text, string command )
		{
			return Regex.IsMatch( text, string.Format( "^/{0} ", command ) );
		}

		private static string RemoveCommand( string text, string command )
		{
			return Regex.Replace( text, string.Format( "^/{0} ", command ), string.Empty, RegexOptions.IgnoreCase );
		}

		internal static string GetCommand( string text )
		{
			if( string.IsNullOrWhiteSpace( text ) )
			{
				return string.Empty;
			}
			string command = text.Split( new char[] { ' ' } )[0];
			return command.Substring( 1 );
		}

		internal static bool TryGetTags( string text, out List<string> tags, out string newText )
		{
			tags = new List<string>();
			newText = text;
			while( true )
			{
				Match m = Regex.Match( newText, @"^#(?<tag>\S+)\s+" );
				if( m.Success )
				{
					string tag = m.Groups["tag"].Value;
					newText = newText.Substring( tag.Length + 2 );
					tags.Add( tag );
				}
				else
				{
					break;
				}
			}
			return tags.Count > 0;
		}

		internal static List<ChatUser> GetWhisperUsers( string text, IList<ChatUser> allUsers, out string newText )
		{
			newText = text;
			List<ChatUser> users = new List<ChatUser>();
			string[] values = text.Split( new char[] { ' ' } );

			while( true )
			{
				Match m = Regex.Match( newText, @"^@(?<username>\S+)\s+" );
				if( m.Success )
				{
					string username = m.Groups["username"].Value;
					ChatUser user = allUsers.FirstOrDefault( u => u.Username == username );
					if( user != null )
					{
						newText = newText.Substring( m.Value.Length );
						if( !users.Contains( user ) )
						{
							users.Add( user );
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			//Match m2 = Regex.Match( newText, @"^#(?<tag>\S+)\s+" );
			//if( m2.Success )
			//{
			//	string tag = m2.Groups["tag"].Value;
			//	newText = newText.Substring( m2.Value.Length );
			//}

			return users;
		}

		public static bool operator ==( ChatUserMessage x, ChatUserMessage y )
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

		public static bool operator !=( ChatUserMessage x, ChatUserMessage y )
		{
			return !( x == y );
		}

		public override bool Equals( object obj )
		{
			if( obj is ChatUserMessage )
			{
				return ID.Equals( ( obj as ChatUserMessage ).ID );
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}
	}
}
