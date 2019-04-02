using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HylandIMServer.Data
{

	[Serializable]
	[XmlRoot( "Stats" )]
	public class ApplicationUserStats
	{
		private const string _xmlFile = @"stats.xml";
		public List<ChatUserStats> Stats { get; set; }

		public ApplicationUserStats()
		{
			Stats = new List<ChatUserStats>();
		}

		public bool Initialize( IEnumerable<ChatUser> users )
		{
			Stats = new List<ChatUserStats>();

			foreach( ChatUser user in users )
			{
				Stats.Add( new ChatUserStats { Username = user.Username } );
			}

			try
			{
				LoadStatsFromDisk();
				return true;
			}
			catch( Exception ex )
			{
				Trace.TraceError( ex.Message );
				return false;
			}
		}

		public void Save()
		{
			XmlSerializer xmlSerializer = new XmlSerializer( typeof( ApplicationUserStats ) );
			XmlDocument xmlDocument = new XmlDocument();
			using( MemoryStream stream = new MemoryStream() )
			{
				xmlSerializer.Serialize( stream, this );
				stream.Position = 0;
				xmlDocument.Load( stream );
				xmlDocument.Save( _xmlFile );
			}
		}

		public ChatUserStats GetStats( string username )
		{
			ChatUserStats stats = Stats.FirstOrDefault( s => s.Username == username );
			if( stats == null )
			{
				stats = new ChatUserStats { Username = username };
				Stats.Add( stats );
				Save();
			}
			return stats;
		}

		private void LoadStatsFromDisk()
		{
			XmlSerializer xmlSerializer = new XmlSerializer( typeof( ApplicationUserStats ) );

			// If the stats do not exist, just write out the empty stats
			if( !File.Exists( _xmlFile ) )
			{
				XmlDocument xmlDocument = new XmlDocument();
				using( MemoryStream stream = new MemoryStream() )
				{
					xmlSerializer.Serialize( stream, this );
					stream.Position = 0;
					xmlDocument.Load( stream );
					xmlDocument.Save( _xmlFile );
					stream.Close();
				}
				return;
			}

			while( true )
			{
				try
				{
					using( StreamReader srdr = new StreamReader( _xmlFile ) )
					{
						ApplicationUserStats config = (ApplicationUserStats)xmlSerializer.Deserialize( srdr );

						Stats.Clear();

						foreach( ChatUserStats rule in config.Stats )
						{
							Stats.Add( rule );
						}
					}
					break;
				}
				catch( IOException )
				{
					// Keep retrying if there is an access error
				}
			}
		}
	}
}
