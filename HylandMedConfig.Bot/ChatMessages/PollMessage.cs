using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class PollMessage : ChatUserMessage, IValidateMessage
	{
		private const long MaxTextLength = 155;
		private const long MinChoices = 2;
		private const long MaxChoices = 15;
		private const long MaxChoiceTextLength = 155;

		private bool _isOpen = true;

		public override string Command
		{
			get
			{
				return Commands.POLL;
			}
		}

		public long TotalVotes
		{
			get
			{
				return Choices.Sum( c => c.Votes.Count );
			}
		}

		public List<PollChoice> Choices
		{
			get;
			private set;
		}

		public bool IsOpen
		{
			get
			{
				return _isOpen;
			}
			set
			{
				_isOpen = value;
				OnPropertyChanged();
			}
		}

		public PollMessage()
		{
			Choices = new List<PollChoice>();
		}

		public void AddChoice( string text )
		{
			Choices.Add( new PollChoice( text, ID ) );
		}

		public void Close()
		{
			IsOpen = false;
		}

		internal void Vote( ChatUser user, Guid choiceID )
		{
			PollChoice choice = Choices.FirstOrDefault( c => c.ID == choiceID );

			if( choice == null )
			{
				throw new Exception( "Choice must be present" );
			}

			if( !IsOpen )
			{
				throw new Exception( "Poll is closed" );
			}

			foreach( PollChoice thisChoice in Choices )
			{
				if( thisChoice.Votes.Contains( user ) )
				{
					thisChoice.Votes.Remove( user );
				}
			}

			choice.Votes.Add( user );
			OnPropertyChanged( nameof( TotalVotes ) );
		}

		public static PollMessage FromString( string text )
		{
			PollMessage message = new PollMessage();

			string[] values = text.Split( new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

			for( int i = 0; i < values.Length; i++ )
			{
				if( i == 0 )
				{
					message.Text = values[i];
				}
				else
				{
					message.AddChoice( values[i] );
				}
			}

			return message;
		}

		public override string ToMessageString( bool includeTag = true )
		{
			StringBuilder messageString = new StringBuilder( base.ToMessageString() );
			messageString.AppendLine();
			foreach( PollChoice choice in Choices )
			{
				messageString.AppendLine( choice.Text );
			}
			return messageString.ToString().TrimEnd( new char[] { '\r', '\n' } );
		}

		public bool Validate( out string error )
		{
			error = string.Empty;

			if( string.IsNullOrWhiteSpace( Text ) )
			{
				error = "Please specify text for the poll";
				return false;
			}

			if( Text.Length > MaxTextLength )
			{
				error = string.Format( "Poll text must be {0} characters or less", MaxTextLength );
				return false;
			}

			if( Choices.Count < MinChoices )
			{
				error = string.Format( "Poll must have at least {0} choies", MinChoices );
				return false;
			}

			if( Choices.Count > MaxChoices )
			{
				error = string.Format( "Polls must not have more than {0} choies", MaxChoices );
				return false;
			}

			if( Choices.Any( c => c.Text.Length > MaxChoiceTextLength ) )
			{
				error = string.Format( "All choices must be {0} characters or less", MaxChoiceTextLength );
				return false;
			}

			return true;
		}

		public override string ToString()
		{
			return Text;
		}
	}

	[Serializable]
	public class PollChoice : ViewModelBase
	{
		private static object _lock = new object();

		public Guid PollID
		{
			get;
			internal set;
		}

		public Guid ID
		{
			get;
			private set;
		}

		public string Text
		{
			get;
			private set;
		}

		public ObservableCollection<ChatUser> Votes
		{
			get;
			private set;
		}

		internal PollChoice( string text, Guid pollID )
		{
			Text = text;
			PollID = pollID;
			Votes = new ObservableCollection<ChatUser>();
			ID = Guid.NewGuid();
		}

		public override string ToString()
		{
			return Text;
		}

		public static bool operator ==( PollChoice x, PollChoice y )
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
		public static bool operator !=( PollChoice x, PollChoice y )
		{
			return !( x == y );
		}

		public override bool Equals( object obj )
		{
			if( obj is PollChoice )
			{
				return ID.Equals( ( obj as PollChoice ).ID );
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}
	}
}
