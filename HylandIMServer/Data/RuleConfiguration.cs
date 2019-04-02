using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HylandIMServer
{
	[Serializable]
	[XmlRoot( "RuleConfiguration" )]
	[XmlInclude( typeof( MessagesPerTimeSpaneRule ) )]
	public class RuleConfiguration
	{
		private string _xmlFile;
		private FileSystemWatcher _fileWatcher;

		public List<MessageValidator> Rules
		{
			get;
			set;
		}

		public RuleConfiguration()
		{

		}

		public RuleConfiguration( string xmlFile )
		{
			_xmlFile = xmlFile;

			Rules = new List<MessageValidator>();

			ReloadConfiguration();

			_fileWatcher = new FileSystemWatcher( Path.GetDirectoryName( _xmlFile ), Path.GetFileName( _xmlFile ) ) { EnableRaisingEvents = true };
			_fileWatcher.Changed += _fileWatcher_Changed;
		}

		private void ReloadConfiguration()
		{
			XmlSerializer xmlSerializer = new XmlSerializer( typeof( RuleConfiguration ) );

			while( true )
			{
				try
				{
					using( StreamReader srdr = new StreamReader( _xmlFile ) )
					{
						RuleConfiguration config = (RuleConfiguration)xmlSerializer.Deserialize( srdr );

						Rules.Clear();

						foreach( MessageValidator rule in config.Rules )
						{
							Rules.Add( rule );
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

		private void _fileWatcher_Changed( object sender, FileSystemEventArgs e )
		{
			ReloadConfiguration();
		}
	}
}
