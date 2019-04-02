using HylandIMServer.Data;
using HylandIMServer.Properties;
using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Threading;

namespace HylandIMServer
{
	public class Program
	{
		#region Declarations

		public static readonly Version MinVersion = Version.Parse( "1.0.0.79" );
		public X509Certificate2 cert = new X509Certificate2( "hmct.pfx", "" );
		public TcpListener server;
		private static readonly string _IPAddress = GetLocalIPAddress();
		public Dictionary<ChatUser, UserConnection> Users = new Dictionary<ChatUser, UserConnection>();
		private List<ChatUserMessage> _messages = new List<ChatUserMessage>();
		private const string _UsersXmlPath = @".\Config\Users.xml";
		private const string _RulesXmlPath = @".\Config\Rules.xml";
		public ApplicationUserStats Stats = new ApplicationUserStats();
		#endregion

		#region Construction

		public Program()
		{
			Console.Title = "Hyland IM Server";

			LoadUsers();

			if( !Stats.Initialize( Users.Keys ) )
			{
				Shutdown();
				return;
			}

			RuleConfiguration = new RuleConfiguration( _RulesXmlPath );

			Console.WriteLine( string.Format( "----- Hyland IM Server -----" ) );

			server = new TcpListener( IPAddress.Parse( _IPAddress ), Settings.Default.Port );
			Console.WriteLine( "IP Address: {0}, Port: {1}", _IPAddress, Settings.Default.Port.ToString() );

			server.Start();
			Listen();
		}

		#endregion

		#region Properties

		public bool ShuttingDown
		{
			get;
			private set;
		}

		public RuleConfiguration RuleConfiguration
		{
			get;
			private set;
		}

		public IList<ChatUserMessage> Messages
		{
			get
			{
				return _messages;
			}
		}

		#endregion

		#region Public Methods

		public void Shutdown()
		{
			ShuttingDown = true;
			foreach( UserConnection connection in Users.Values )
			{
				if( connection.Client != null )
				{
                    // asdfasdf
					try
					{
						//connection.Client._bw.Write( (int)TcpCommands.AppShutdown );
						//connection.Client._bw.Flush();
					}
					catch( Exception ex )
					{
						Console.WriteLine( "Exception when sending AppShutdown to " + connection.Client.User.Username );
						Console.WriteLine( ex.Message );
					}
				}
			}
		}

		public static string GetLocalIPAddress()
		{
            // TODO: Fix, not working because somehow docker is interfering here
            return "10.52.1.117";
			//var host = Dns.GetHostEntry( Dns.GetHostName() );
			//foreach( var ip in host.AddressList )
			//{
			//	if( ip.AddressFamily == AddressFamily.InterNetwork )
			//	{
			//		return ip.ToString();
			//	}
			//}
			//throw new Exception( "Local IP Address Not Found!" );
		}

		#endregion

		#region ** Main **

		static void Main( string[] args )
		{
			Program p = new Program();
			Console.WriteLine();
			Console.WriteLine( "Press enter to close program." );
			Console.ReadLine();
			p.Shutdown();
		}

		#endregion

		#region Private Methods

		private void Listen()
		{
			server.BeginAcceptTcpClient( OnAcceptTcpCliet, null );
		}

		private void LoadUsers()
		{
			XmlSerializer xmlSerializer = new XmlSerializer( typeof( UserConfiguration ) );

			using( StreamReader srdr = new StreamReader( _UsersXmlPath ) )
			{
				UserConfiguration config = (UserConfiguration)xmlSerializer.Deserialize( srdr );

				foreach( UserXml user in config.Users )
				{
					Users.Add( new ChatUser( user.Username, user.DisplayName, user.Nickname, user.ImageUrl ), new UserConnection() );
				}
			}
		}

		private void OnAcceptTcpCliet( IAsyncResult result )
		{
			TcpClient client = server.EndAcceptTcpClient( result );

			UserClient userClient = new UserClient( this, client );
			Task.Factory.StartNew( userClient.SetupConnection );
			server.BeginAcceptTcpClient( OnAcceptTcpCliet, null );
		}

		#endregion
	}
}

