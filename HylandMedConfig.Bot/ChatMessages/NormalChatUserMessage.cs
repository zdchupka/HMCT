using System;
using System.Collections.Generic;
using System.Linq;

namespace HylandMedConfig.Common
{
    [Serializable]
    public class NormalChatUserMessage : ChatUserMessage
    {
		[Obsolete("Use parameterless constructor instead")]
        public NormalChatUserMessage(ChatUser fromUser, string text, List<ChatUser> toUsers = null)
        {
			FromUser = fromUser;
			Text = text;
			ToUsers = toUsers;
        }

		public NormalChatUserMessage()
		{

		}
    }
}
