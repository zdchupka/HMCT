using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
	public static class YouTubeHelper
	{
		public static bool IsYouTubeLink( string url, out string fixedUrl )
		{
			bool isYouTubeLink = false;
			fixedUrl = url;
			try
			{
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( url );
				request.Method = "HEAD";
				using( HttpWebResponse response = (HttpWebResponse)request.GetResponse() )
				{
					isYouTubeLink = response.ResponseUri.ToString().Contains( "youtube.com" );
					if(isYouTubeLink)
					{
						fixedUrl = response.ResponseUri.AbsoluteUri;
					}
				}
			}
			catch { }
			return isYouTubeLink;
		}
	}
}
