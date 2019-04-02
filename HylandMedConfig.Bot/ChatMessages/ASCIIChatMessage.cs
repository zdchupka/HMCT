using System;
using System.Collections.Generic;

namespace HylandMedConfig.Common
{
    [Serializable]
    public class ASCIIChatMessage : ChatUserMessage
	{
        public override string Command
        {
            get
            {
                return ChatUserMessage.Commands.ASCII;
            }
        }

		[Obsolete( "Use parameterless constructor instead" )]
		public ASCIIChatMessage(ChatUser fromUser, string text, List<ChatUser> toUsers = null)
        {
        }

		public ASCIIChatMessage()
		{

		}
    }
}
