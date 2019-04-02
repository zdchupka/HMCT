using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HylandMedConfig.Common
{
	public class ChatErrorEventArgs : EventArgs
	{
		public ChatErrorEventArgs( string message )
		{
			Message = message;
		}

		public string Message
		{
			get;
			private set;
		}
	}

	public class ChatUserStatsEventArgs : EventArgs
	{
		public ChatUserStatsEventArgs( ChatUser user, ChatUserStats stats )
		{
			User = user;
			Stats = stats;
		}

		public ChatUser User
		{
			get;
			private set;
		}

		public ChatUserStats Stats
		{
			get;
			private set;
		}
	}

	public class ChatUsernameEventArgs : EventArgs
	{
		public ChatUsernameEventArgs( string username )
		{
			Username = username;
		}

		public string Username
		{
			get;
			private set;
		}
	}

	public class WhiteboardDataEventArgs : EventArgs
	{
		public WhiteboardDataEventArgs()
		{

		}

		public WhiteboardChatMessage Message
		{
			get;
			set;
		}

		public WhiteboardEntry Entry
		{
			get;
			set;
		}
	}

	public class UserVotedEventArgs : EventArgs
	{
		public UserVotedEventArgs( ChatUser user, PollMessage poll, PollChoice choice )
		{
			User = user;
			Choice = choice;
			Poll = poll;
		}

		public ChatUser User
		{
			get;
			private set;
		}

		public PollChoice Choice
		{
			get;
			private set;
		}

		public PollMessage Poll
		{
			get;
			private set;
		}
	}

	public class PollClosedEventArgs : EventArgs
	{
		public PollClosedEventArgs( PollMessage message )
		{
			Message = message;
		}

		public PollMessage Message
		{
			get;
			private set;
		}
	}

	public class UserRatedMessagedEventArgs : EventArgs
	{
		public UserRatedMessagedEventArgs( ChatUser user, ChatUserMessage message, MessageRating rating, MessageRating previousRating )
		{
			User = user;
			Message = message;
			Rating = rating;
			PreviousRating = previousRating;
		}

		public ChatUser User
		{
			get;
			private set;
		}

		public ChatUserMessage Message
		{
			get;
			private set;
		}

		public MessageRating Rating
		{
			get;
			private set;
		}

		public MessageRating PreviousRating
		{
			get;
			private set;
		}
	}

	public class ChatUserEventArgs : EventArgs
	{
		public ChatUserEventArgs( ChatUser user )
		{
			User = user;
		}

		public ChatUser User
		{
			get;
			private set;
		}
	}

	public class FoosballGameReadyEventArgs : EventArgs
	{
		public FoosballGameReadyEventArgs( ChatUser blackOne, ChatUser blackTwo, ChatUser yellowOne, ChatUser yellowTwo )
		{
			BlackOne = blackOne;
			BlackTwo = blackTwo;
			YellowOne = yellowOne;
			YellowTwo = yellowTwo;
		}

		public ChatUser BlackOne
		{
			get;
			private set;
		}

		public ChatUser BlackTwo
		{
			get;
			private set;
		}

		public ChatUser YellowOne
		{
			get;
			private set;
		}

		public ChatUser YellowTwo
		{
			get;
			private set;
		}
	}

	public class ChatReceivedEventArgs : EventArgs
	{
		public ChatReceivedEventArgs( ChatUserMessage message )
		{
			Message = message;
		}

		public ChatUserMessage Message
		{
			get;
			private set;
		}
	}
}
