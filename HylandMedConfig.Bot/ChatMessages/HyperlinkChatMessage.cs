using System;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class HyperlinkChatMessage : ChatUserMessage
	{
		public override string Command
		{
			get
			{
				return Commands.LINK;

			}
		}

		public string Url
		{
			get { return Text; }
			set { Text = value; }
		}

		public string ImageUrl
		{
			get;
			internal set;
		}

		public string Title
		{
			get;
			internal set;
		}

		public string Description
		{
			get;
			internal set;
		}

		public string SiteName
		{
			get;
			internal set;
		}

		public bool HasMetaData
		{
			get;
			internal set;
		}

		private bool? _IsYouTubeLink = null;

		public bool IsYouTubeLink
		{
			get
			{
				if( !_IsYouTubeLink.HasValue )
				{
					string fixedUrl;
					_IsYouTubeLink = YouTubeHelper.IsYouTubeLink( Url, out fixedUrl );
					if( _IsYouTubeLink.Value )
					{
						Text = fixedUrl;
					}
				}
				return _IsYouTubeLink.Value;
			}
		}

		public HyperlinkChatMessage( string url )
		{
			Url = url;
		}
	}
}
