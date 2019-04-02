using System;
using System.Collections.Generic;

namespace HylandMedConfig.Common
{
    [Serializable]
    public class HTMLChatMessage : ChatUserMessage
    {
        public override string Command
        {
            get
            {
                return ChatUserMessage.Commands.HTML;
            }
        }

        public string HTMLText
        {
            get
			{
				return Text;
			}
			set
			{
				Text = value;
			}
        }

		[Obsolete( "Use parameterless constructor instead" )]
		public HTMLChatMessage(ChatUser fromUser, string htmlText, List<ChatUser> toUsers = null)
        {
			FromUser = fromUser;
            HTMLText = htmlText;
			ToUsers = toUsers;
        }

		public HTMLChatMessage()
		{

		}
    }
}
