using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;

namespace HylandMedConfig.Common
{
	internal static class ExtensionMethods
	{
		internal static T ReadObject<T>( this BinaryReader br ) where T : class
		{
			T obj;
			int size = br.ReadInt32();
			byte[] bytes = br.ReadBytes( size );
			IFormatter formatter = new BinaryFormatter();
			using( MemoryStream stream = new MemoryStream( bytes ) )
			{
				object test = formatter.Deserialize( stream );
				obj = test as T;
			}
			return obj;
		}

		internal static bool WriteObject<T>( this BinaryWriter bw, T obj ) where T : class
		{
			byte[] bytes;
			IFormatter formatter = new BinaryFormatter();
			using( MemoryStream stream = new MemoryStream() )
			{
				formatter.Serialize( stream, obj );
				bytes = stream.ToArray();
			}

			try
			{
				bw.Write( bytes.Length );
				bw.Write( bytes );
				return true;
			}
			catch( SocketException )
			{
				return false;
			}
		}

		public static void DownloadMetadata( this HyperlinkChatMessage message )
		{
			WebResponse response = null;
			StreamReader reader = null;
			string result = string.Empty;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create( message.Url );
				request.Method = "GET";
				response = request.GetResponse();
				reader = new StreamReader( response.GetResponseStream(), Encoding.UTF8 );
				result = reader.ReadToEnd();
				bool hasMetaData = false;

				HtmlDocument doc = new HtmlDocument();

				doc.LoadHtml( result );

				var imageNode = doc.DocumentNode.SelectSingleNode( "//meta[@property='og:image']" );

				if( imageNode != null )
				{
					message.ImageUrl = imageNode.Attributes["content"].Value;
				}

				var siteName = doc.DocumentNode.SelectSingleNode( "//meta[@property='og:site_name']" );
				if( siteName != null )
				{
					message.SiteName = HttpUtility.HtmlDecode( siteName.Attributes["content"].Value.ToUpper() );
				}


				var title = doc.DocumentNode.SelectSingleNode( "//meta[@property='og:title']" );
				if( title != null )
				{

					message.Title = HttpUtility.HtmlDecode( title.Attributes["content"].Value );
					hasMetaData = true;
				}

				var description = doc.DocumentNode.SelectSingleNode( "//meta[@property='og:description']" );
				if( description != null )
				{
					message.Description = HttpUtility.HtmlDecode( description.Attributes["content"].Value );
				}
				else
				{
					message.Description = "(No description)";
				}
				message.HasMetaData = hasMetaData;
			}
			catch
			{
			}
			finally
			{
				if( reader != null )
					reader.Close();
				if( response != null )
					response.Close();
			}
		}
	}
}
