using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
	[Serializable]
	public class SystemChatMessage : ChatUserMessage
	{
		public static ChatUser SystemUser = new ChatUser( "system", "System", "" );
		public SystemChatMessage( string text )
			: base()
		{
			Text = text;
			FromUser = SystemUser;
		}
	}
}
