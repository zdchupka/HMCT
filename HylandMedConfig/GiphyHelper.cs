using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HylandMedConfig.Json.Search;

namespace HylandMedConfig
{
	public static class GiphyHelper
	{
		public const string PublicKey = @"dc6zaTOxFJmzC";
		private static readonly string SearchRequestUrl = string.Format( @"http://api.giphy.com/v1/gifs/search?q={{0}}&api_key={0}&limit={{1}}&rating={{2}}", PublicKey );
		private static Random _random = new Random();

		public static IEnumerable<GiphyResult> Search( string text, int maxResults, string rating )
		{
			IList<GiphyResult> result = GetJsonResponse( string.Format( SearchRequestUrl, HttpUtility.UrlEncode( text ), 100, rating ) );
			return result.OrderBy( r => _random.Next() ).Take( maxResults );
		}

		private static IList<GiphyResult> GetJsonResponse( string requestUrl )
		{
			List<GiphyResult> hits = new List<GiphyResult>();
			try
			{
				WebRequest request = HttpWebRequest.Create( requestUrl );
				request.Method = "GET";
				using( WebResponse response = request.GetResponse() )
				{
					using( Stream responseStream = response.GetResponseStream() )
					{
						DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rootobject ) );
						Rootobject welp = serializer.ReadObject( responseStream ) as Rootobject;

						if( welp != null )
						{
							foreach( Datum data in welp.data )
							{
								hits.Add( new GiphyResult
								{
									SmallImageUrl = data.images.fixed_height_small.url,
									LargeImageUrl = data.images.fixed_height.url,
								} );
							}
						}
					}
				}
			}
			catch( Exception )
			{

			}

			return hits;
		}
	}

	public class GiphyResult
	{
		public string SmallImageUrl
		{
			get;
			set;
		}

		public string LargeImageUrl
		{
			get;
			set;
		}
	}
}
