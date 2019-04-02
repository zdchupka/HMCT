using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HylandMedConfig.Common
{
	public class ChatBot : IDisposable
	{
		#region Declarations
		private readonly IChatProxy _chatProxy;
		private bool _isDisposed = false;
		private List<string> _sentMessages = new List<string>();
		private string _loginError = string.Empty;
		#endregion

		/// <summary>
		/// Gets the username for this bot
		/// </summary>
		public string Username
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the display name for this bot
		/// </summary>
		public string DisplayName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the nickname for this bot
		/// </summary>
		public string Nickname
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ImageUrl for this bot
		/// </summary>
		public string ImageUrl
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a message that is whispered to a user when they whisper to the bot with a '/help' command
		/// </summary>
		protected virtual string HelpMessage
		{
			get
			{
				return "/ascii throw new NotImplementedException();";
			}
		}

		public ChatBot( string username, string displayName, string imageUrl )
			: this( username, displayName, displayName, imageUrl )
		{

		}

		/// <summary>
		/// Create new bot and login.   Will throw ChatUserLoginException if this username is already logged in.
		/// </summary>
		/// <exception cref="HylandMedConfig.Common.ChatUserLoginException">
		/// Thrown when username is already logged in
		/// </exception>
		/// <param name="username"></param>
		/// <param name="displayName"></param>
		/// <param name="nickname"></param>
		/// <param name="imageUrl"></param>
		public ChatBot( string username, string displayName, string nickname, string imageUrl )
		{
			// Validate the username and display names
			ChatUser tempUser = new ChatUser( username, displayName, imageUrl, true );

			Username = tempUser.Username;
			DisplayName = tempUser.DisplayName;
			Nickname = tempUser.Nickname;
			ImageUrl = imageUrl;

			_chatProxy = ChatProxyFactory.CreateBotChatProxy( this );
			_chatProxy.Reconnected += _chatProxy_Reconnected;
			_chatProxy.MessageReceived += _chatProxy_MessageReceived;
			_chatProxy.UserLoggedOn += _chatProxy_UserLoggedOn;
			_chatProxy.UserLoggedOut += _chatProxy_UserLoggedOut;
			_chatProxy.UserEnteredText += _chatProxy_UserEnteredText;
			_chatProxy.UserClearedText += _chatProxy_UserClearedText;
			_chatProxy.UserIdle += _chatProxy_UserIdle;
			_chatProxy.UserActive += _chatProxy_UserActive;
			_chatProxy.UserFoozReady += _chatProxy_UserFoozReady;
			_chatProxy.UserNotFoozReady += _chatProxy_UserNotFoozReady;
			_chatProxy.FoozGameReady += _chatProxy_FoozGameReady;
			_chatProxy.MoodChanged += _chatProxy_MoodChanged;
			_chatProxy.Disconnected += _chatProxy_ServerShutdown;
			_chatProxy.UserVoted += _chatProxy_UserVoted;
			_chatProxy.Error += _chatProxy_ErrorReceived;
			_chatProxy.UserRatedMessage += _chatProxy_UserRatedMessage;
			_chatProxy.WhiteboardDataReceived += _chatProxy_WhiteboardDataReceived;
			_chatProxy.PollClosed += _chatProxy_PollClosed;
		}

		private void _chatProxy_PollClosed( object sender, PollClosedEventArgs e )
		{
			OnPollClosed( e.Message );
		}

		private void _chatProxy_WhiteboardDataReceived( object sender, WhiteboardDataEventArgs e )
		{
			OnWhiteboardDataReceived( e );
		}

		private void _chatProxy_UserRatedMessage( object sender, UserRatedMessagedEventArgs e )
		{
			OnUserRatedMessage( e.User, e.Message, e.Rating, e.PreviousRating );
		}

		private void _chatProxy_UserVoted( object sender, UserVotedEventArgs e )
		{
			OnUserVoted( e.User, e.Poll, e.Choice );
		}

		private void _chatProxy_ErrorReceived( object sender, ChatErrorEventArgs e )
		{
			OnError( e.Message );
		}

		private void _chatProxy_ServerShutdown( object sender, EventArgs e )
		{
			OnDisconnected();
		}

		private void _chatProxy_UserNotFoozReady( object sender, ChatUserEventArgs e )
		{
			OnUserNotFoozReady( e.User );
		}

		private void _chatProxy_UserFoozReady( object sender, ChatUserEventArgs e )
		{
			OnUserFoozReady( e.User );
		}

		private void _chatProxy_FoozGameReady( object sender, FoosballGameReadyEventArgs e )
		{
			OnFoozGameReady( e );
		}

		private void _chatProxy_UserActive( object sender, ChatUserEventArgs e )
		{
			OnUserActive( e.User );
		}

		private void _chatProxy_UserIdle( object sender, ChatUserEventArgs e )
		{
			OnUserIdle( e.User );
		}

		private void _chatProxy_MoodChanged( object sender, ChatUserEventArgs e )
		{
			OnMoodChanged( e.User );
		}

		private void _chatProxy_UserClearedText( object sender, ChatUserEventArgs e )
		{
			OnUserEnteredText( e.User );
		}

		private void _chatProxy_UserEnteredText( object sender, ChatUserEventArgs e )
		{
			OnUserEnteredText( e.User );
		}

		private void _chatProxy_UserLoggedOut( object sender, ChatUserEventArgs e )
		{
			OnUserLoggedOut( e.User );
		}

		private void _chatProxy_UserLoggedOn( object sender, ChatUserEventArgs e )
		{
			OnUserLoggedIn( e.User );
		}

		private void _chatProxy_MessageReceived( object sender, ChatReceivedEventArgs e )
		{
			if( e.Message.ToUsers.Contains( _chatProxy.CurrentUser ) )
			{
				if( Regex.IsMatch( e.Message.Text, @"^/help", RegexOptions.IgnoreCase ) )
				{
					OnHelpRequested( e.Message );
				}
				else
				{
					OnMessageReceived( e.Message );
				}
			}
			else
			{
				OnMessageReceived( e.Message );
			}

			if( e.Message is PollMessage )
			{
				OnPollReceived( e.Message as PollMessage );
			}
		}

		/// <summary>
		/// Notify users that you are typing (when you send a message, users will automatically 
		/// be notified that you are no longer typing, there is no need to call 'NotifyTextCleared')
		/// </summary>
		public void NotifyTextEntered()
		{
			_chatProxy.NotifyTextEntered();
		}

		/// <summary>
		/// Vote for a specified poll choice
		/// </summary>
		/// <param name="choice"></param>
		public void Vote( PollChoice choice )
		{
			_chatProxy.Vote( choice );
		}

		/// <summary>
		/// Notify users that you are not typing
		/// </summary>
		public void NotifyTextCleared()
		{
			_chatProxy.NotifyTextCleared();
		}

		/// <summary>
		/// Send a public message
		/// </summary>
		public void SendMessage( string text )
		{
			ChatUserMessage message = ChatUserMessage.Parse( _chatProxy.CurrentUser, text, _chatProxy.Users );
			SendMessage( message );
		}

		/// <summary>
		/// Send a public message
		/// </summary>
		public void SendMessage( ChatUserMessage message )
		{
			message.FromUser = _chatProxy.CurrentUser;

			_sentMessages.Add( message.ToMessageString() );

			_chatProxy.SendMessage( message );
		}

		/// <summary>
		/// Send drawing response to a message.  Points will be drawn on the 'messageToDrawOn'
		/// </summary>
		public void SendWhiteboardData( WhiteboardChatMessage messageToDrawOn, WhiteboardEntry entry )
		{
			_chatProxy.SendWhiteboardData( messageToDrawOn, entry );
		}

		/// <summary>
		/// Send a public formatted message
		/// </summary>
		public void SendMessage( string text, params object[] args )
		{
			SendMessage( string.Format( text, args ) );
		}

		/// <summary>
		/// Returns true if the specified user is this bot
		/// </summary>
		public bool IsThisUser( ChatUser user )
		{
			return user != null && user == _chatProxy.CurrentUser;
		}

		/// <summary>
		/// Returns true if the specified list of users contains this bot
		/// </summary>
		public bool ContainsThisUser( IEnumerable<ChatUser> users )
		{
			return users != null && users.Contains( _chatProxy.CurrentUser );
		}

		/// <summary>
		/// Send a whisper to the specified user
		/// </summary>
		public void SendWhisper( ChatUser user, string text )
		{
			SendMessage( string.Format( "@{0} {1}", user.Username, text ) );
		}

		/// <summary>
		/// Send formatted whisper to the specified user
		/// </summary>
		public void SendWhisper( ChatUser user, string text, params object[] args )
		{
			string unformattedMessage = string.Format( "@{0} {1}", user.Username, text );
			SendMessage( string.Format( unformattedMessage, args ) );
		}

		/// <summary>
		/// Send whisper message to a group of users
		/// </summary>
		public void SendWhisper( IEnumerable<ChatUser> users, string text )
		{
			StringBuilder toUsers = new StringBuilder();
			foreach( ChatUser user in users )
			{
				toUsers.AppendFormat( "@{0} ", user.Username );
			}
			SendMessage( string.Format( "{0} {1}", toUsers, text ) );
		}

		/// <summary>
		/// Send a reply to a message
		/// </summary>
		public void SendReply( ChatUserMessage message, string text )
		{
			if( message.Tags.Count > 0 )
			{
				StringBuilder reply = new StringBuilder();
				foreach( string tag in message.Tags )
				{
					reply.AppendFormat( "#{0} ", tag );
				}
				reply.Append( text );
				SendMessage( reply.ToString() );
			}
			else if( message.ToUsers != null && message.ToUsers.Count > 0 )
			{
				List<ChatUser> toUsers = new List<ChatUser> { message.FromUser };
				foreach( ChatUser user in message.ToUsers )
				{
					if( !IsThisUser( user ) )
					{
						toUsers.Add( user );
					}
				}
				SendWhisper( toUsers, text );
			}
			else
			{
				SendMessage( text );
			}
		}

		/// <summary>
		/// Send a reply to a message
		/// </summary>
		public void SendReply( ChatUserMessage message, ChatUserMessage replyMessage )
		{
			if( message.ToUsers != null && message.ToUsers.Count > 0 )
			{
				replyMessage.ToUsers = message.ToUsers;
				replyMessage.ToUsers.Add( message.FromUser );
				List<ChatUser> toUsers = new List<ChatUser> { message.FromUser };
				foreach( ChatUser user in message.ToUsers )
				{
					if( !IsThisUser( user ) )
					{
						toUsers.Add( user );
					}
				}
			}

			SendMessage( replyMessage );
		}

		/// <summary>
		/// Set your bot's mood
		/// </summary>
		public void SetMood( string mood )
		{
			_chatProxy.SetMood( mood );
		}

		/// <summary>
		/// Rate a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="rating"></param>
		public void RateMessage( ChatUserMessage message, MessageRating rating )
		{
			_chatProxy.RateMessage( message, rating );
		}

		/// <summary>
		/// Occurs when a message is received from another user
		/// </summary>
		/// <param name="message"></param>
		protected virtual void OnMessageReceived( ChatUserMessage message )
		{
			//Debug.WriteLine( "Message received: {0}", message );
		}

		/// <summary>
		/// Occurs when a poll message is received from another user (this occurs after the OnMessageReceived)
		/// </summary>
		/// <param name="message"></param>
		protected virtual void OnPollReceived( PollMessage message )
		{
			//Debug.WriteLine( "Poll received: {0}", message );
		}

		/// <summary>
		/// Occurs when help is requested from a bot
		/// </summary>
		protected virtual void OnHelpRequested( ChatUserMessage message )
		{
			SendWhisper( message.FromUser, HelpMessage );
		}

		/// <summary>
		/// Occurs when a user votes on a poll
		/// </summary>
		/// <param name="user">The user that voted</param>
		/// <param name="poll">The poll that the user voted on</param>
		/// <param name="choice">The choice that the user voted for</param>
		protected virtual void OnUserVoted( ChatUser user, PollMessage poll, PollChoice choice )
		{
			//Debug.WriteLine( "User: {0}, Voted on: '{1}', Answered: {2}", user, poll.Text, choice.Text );
		}

		/// <summary>
		/// Occurs when a user rates a message
		/// </summary>
		/// <param name="user">The user that is rating the message</param>
		/// <param name="message">The message that is being rated</param>
		/// <param name="rating">The rating that is being given</param>
		protected virtual void OnUserRatedMessage( ChatUser user, ChatUserMessage message, MessageRating rating, MessageRating previousRating )
		{
			//Debug.WriteLine( "User: {0}, rated message {1}", user, rating.ToString() );
		}

		/// <summary>
		/// Occurs when a user draws on a 
		/// </summary>
		/// <param name="user"></param>
		/// <param name="e"></param>
		protected virtual void OnWhiteboardDataReceived( WhiteboardDataEventArgs e )
		{

		}

		/// <summary>
		/// Occurs when a poll is closed
		/// </summary>
		/// <param name="message"></param>
		protected virtual void OnPollClosed( PollMessage message )
		{

		}

		/// <summary>
		/// Occurs when an error happens
		/// </summary>
		/// <param name="error"></param>
		protected virtual void OnError( string error )
		{
			Debug.WriteLine( "Error: " + error );
		}

		/// <summary>
		/// Occurs when a user logs in
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserLoggedIn( ChatUser user )
		{
			//Debug.WriteLine( "User Logged In: {0}", user );
		}

		/// <summary>
		/// Occurs when a user logs out
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserLoggedOut( ChatUser user )
		{
			//Debug.WriteLine( "User Logged Out: {0}", user );
		}

		/// <summary>
		/// Occurs when a user has entered text in the message textbox
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserEnteredText( ChatUser user )
		{
			//Debug.WriteLine( "User Entered Text: {0}", user );
		}

		/// <summary>
		/// Occurs when a user clears their entered message in the textbox
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserClearedText( ChatUser user )
		{
			//Debug.WriteLine( "User Cleared Text: {0}", user );
		}

		/// <summary>
		/// Occurs when a user goes idle
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserIdle( ChatUser user )
		{
			//Debug.WriteLine( "User Idle: {0}", user );
		}

		/// <summary>
		/// Occurs when a user changes their mood
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnMoodChanged( ChatUser user )
		{
			//Debug.WriteLine( "{0} changed their mood", user );
		}

		/// <summary>
		/// Occurs when when a user becomes active after being idle
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserActive( ChatUser user )
		{
			//Debug.WriteLine( "User Active: {0}", user );
		}

		/// <summary>
		/// Occurs when a user +1's
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserFoozReady( ChatUser user )
		{
			//Debug.WriteLine( "User Ready for Fooz: {0}", user );
		}

		/// <summary>
		/// Occurs when a user unchecks the +1 checkbox
		/// </summary>
		/// <param name="user"></param>
		protected virtual void OnUserNotFoozReady( ChatUser user )
		{
			//Debug.WriteLine( "User Not Ready for Fooz: {0}", user );
		}

		/// <summary>
		/// Occurs when a foosball game is ready
		/// </summary>
		protected virtual void OnFoozGameReady( FoosballGameReadyEventArgs e )
		{
			//Debug.WriteLine( "Fooz Game ready" );
		}

		/// <summary>
		/// Gets a readonly list of all users on the server (bots are only available when they are logged on)
		/// </summary>
		public IReadOnlyCollection<ChatUser> Users
		{
			get
			{
				return _chatProxy.Users;
			}
		}

		/// <summary>
		/// Gets a list of messages that have been received / sent
		/// </summary>
		protected IReadOnlyCollection<ChatUserMessage> GetAllPreviousMessages()
		{
			return ( _chatProxy as TcpChatProxy )?.Messages?.OfType<ChatUserMessage>().ToList().AsReadOnly();
		}

		/// <summary>
		/// Gets a list of messages that have been sent by this bot
		/// </summary>
		/// <returns></returns>
		protected IReadOnlyCollection<string> GetMyPreviousMessages()
		{
			return _sentMessages.AsReadOnly();
		}

		/// <summary>
		/// Occurs when you are re-connected to the server
		/// </summary>
		protected virtual void OnReconnected()
		{
			Debug.WriteLine( "Reconnected" );
		}

		/// <summary>
		/// Occurs when you are disconnected from the server
		/// </summary>
		protected virtual void OnDisconnected()
		{
			Debug.WriteLine( "OnDisconnected" );
		}

		private void _chatProxy_Reconnected( object sender, EventArgs e )
		{
			OnReconnected();
		}

		public void Dispose()
		{
			if( !_isDisposed )
			{
				if( _chatProxy != null )
				{
					_chatProxy.Dispose();
				}
				_isDisposed = true;
			}
		}
	}
}
