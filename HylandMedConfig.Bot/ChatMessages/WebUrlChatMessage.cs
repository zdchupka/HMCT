using System;
using System.Collections.Generic;

namespace HylandMedConfig.Common
{
    [Serializable]
    public class WebUrlChatMessage : ChatUserMessage
    {
        public override string Command
        {
            get
            {
                return ChatUserMessage.Commands.WEB;
            }
        }

        public string Url
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
		public WebUrlChatMessage(ChatUser fromUser, string url, List<ChatUser> toUsers = null)
		{
			FromUser = fromUser;
			Url = url;
			ToUsers = toUsers;
        }

		public WebUrlChatMessage()
		{

		}
    }
}
