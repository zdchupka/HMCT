using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Attached;

namespace HylandMedConfig
{
	public class PollClosedMessage : ChatUserMessage
	{
		public PollClosedMessage( PollMessage message )
		{
			FromUser = SystemChatMessage.SystemUser;
			Poll = message;
			Winners = new List<PollChoice>();
			int maxVotes = Poll.Choices.Max( c => c.Votes.Count );

			if( maxVotes > 0 )
			{
				Winners.AddRange( Poll.Choices.Where( c => c.Votes.Count == maxVotes ) );
			}
		}

		public List<PollChoice> Winners
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
}
