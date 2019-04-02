using System;
using System.Collections.Generic;
using System.Net;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class ImageUrlChatMessage : ChatUserMessage
	{
		public override string Command
		{
			get
			{
				return ChatUserMessage.Commands.IMAGEURL;
			}
		}

		public string ImageUrl
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

		/// <summary>
		/// Gets a boolean indicating if this image loaded on the client
		/// </summary>
		public bool Loaded
		{
			get;
			internal set;
		}

		[Obsolete( "Use parameterless constructor instead" )]
		public ImageUrlChatMessage( ChatUser fromUser, string imageUrl, List<ChatUser> toUsers = null )
		{
			FromUser = fromUser;
			ImageUrl = imageUrl;
			ToUsers = toUsers;
		}

		public ImageUrlChatMessage()
		{

		}
	}
}
