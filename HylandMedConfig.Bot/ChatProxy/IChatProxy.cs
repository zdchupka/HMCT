using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HylandMedConfig.Common
{
	public interface IChatProxy : IDisposable
	{
		ReadOnlyObservableCollection<ChatUser> Users { get; }
		ChatUser CurrentUser { get; }
		bool IsConnected { get; }
		INicknameService NicknameService { get; }
		ITagService TagService { get; }

		event EventHandler Reconnected;
		event EventHandler<ChatErrorEventArgs> Error;
		event EventHandler Disconnected;
		event EventHandler<ChatUserEventArgs> UserLoggedOn;
		event EventHandler<ChatUserEventArgs> UserLoggedOut;
		event EventHandler<ChatReceivedEventArgs> MessageReceived;
		event EventHandler<ChatUserEventArgs> UserEnteredText;
		event EventHandler<ChatUserEventArgs> UserClearedText;
		event EventHandler<ChatUserEventArgs> UserIdle;
		event EventHandler<ChatUserEventArgs> UserActive;
		event EventHandler<ChatUserEventArgs> UserFoozReady;
		event EventHandler<ChatUserEventArgs> UserNotFoozReady;
		event EventHandler<FoosballGameReadyEventArgs> FoozGameReady;
		event EventHandler<ChatUserStatsEventArgs> ChatUserStatsReceived;
		event EventHandler<ChatUserEventArgs> MoodChanged;
		event EventHandler<UserVotedEventArgs> UserVoted;
		event EventHandler<ChatReceivedEventArgs> MessageSentSuccessfully;
		event EventHandler<UserRatedMessagedEventArgs> UserRatedMessage;
		event EventHandler<WhiteboardDataEventArgs> WhiteboardDataReceived;
		event EventHandler<PollClosedEventArgs> PollClosed;

		/// <summary>
		/// Send a message 
		/// </summary>
		/// <param name="message"></param>
		/// <returns>True if successful</returns>
		void SendMessage( ChatUserMessage message );
		void Vote( PollChoice choice );
		void ClosePoll( PollMessage message );
		bool SetMood( string mood );
		void NotifyTextEntered( IList<ChatUser> users = null );
		void NotifyTextCleared( IList<ChatUser> users = null );
		void NotifyActive();
		bool NotifyFoozReady();
		void NotifyNotFoozReady();
		void NotifyUserRemote();
		void NotifyUserNotRemote();
		void RequestStats( string username );
		void RateMessage( ChatUserMessage message, MessageRating rating );
		void SendWhiteboardData( ChatUserMessage message, WhiteboardEntry entry );
		void LockWhiteboard( WhiteboardChatMessage message );
		void UnlockWhiteboard( WhiteboardChatMessage message );
		void ClearWhiteboard( WhiteboardChatMessage message );
	}

	public class ChatProxyFactory
	{
		public static IChatProxy CreateChatProxy( string username, INicknameService nicknameService, ITagService tagService )
		{
			return new TcpChatProxy( new LoginArgs
			{
				Username = username,
				Version = typeof( ChatBot ).Assembly.GetName().Version,
			}, nicknameService, tagService );
		}

		public static IChatProxy CreateBotChatProxy( ChatBot chatBot )
		{
			return new TcpChatProxy( new LoginArgs
			{
				Username = chatBot.Username,
				DisplayName = chatBot.DisplayName,
				ImageUrl = chatBot.ImageUrl,
				Nickname = chatBot.Nickname,
				Version = typeof( ChatBot ).Assembly.GetName().Version,
				IsBot = true,
				BotCreatorUserName = Environment.UserName.ToLower(),
			} );
		}
	}
}
