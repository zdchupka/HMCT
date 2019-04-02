using HylandMedConfig.Common;
using System;
namespace HylandMedConfig
{
	[Serializable]
	public class HelpChatMessage : ChatUserMessage
	{
		public HelpChatMessage()
			: base()
		{
			FromUser = SystemChatMessage.SystemUser;
		}
	}
}
