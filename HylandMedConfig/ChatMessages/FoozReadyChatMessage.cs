using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
	[Serializable]
	public class FoozReadyChatMessage : ChatUserMessage
	{
		public ChatUser BlackFront
		{
			get;
			private set;
		}

		public ChatUser BlackRear
		{
			get;
			private set;
		}

		public ChatUser YellowFront
		{
			get;
			private set;
		}

		public ChatUser YellowRear
		{
			get;
			private set;
		}

		public FoozReadyChatMessage( ChatUser yellowFront, ChatUser yellowRear, ChatUser blackFront, ChatUser blackRear )
		{
			YellowFront = yellowFront;
			YellowRear = yellowRear;
			BlackFront = blackFront;
			FromUser = SystemChatMessage.SystemUser;
			BlackRear = blackRear;
		}
	}
}
