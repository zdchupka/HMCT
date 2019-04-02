using System;
using System.Collections.Generic;
using System.Net;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class VideoUrlChatMessage : ChatUserMessage
	{
		public override string Command
		{
			get
			{
				return ChatUserMessage.Commands.VIDEOURL;
			}
		}

		public string VideoUrl
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
		public VideoUrlChatMessage( ChatUser fromUser, string videoUrl, List<ChatUser> toUsers = null )
		{
			FromUser = fromUser;
			VideoUrl = videoUrl;
			ToUsers = toUsers;
		}

		public VideoUrlChatMessage()
		{

		}
	}
}
