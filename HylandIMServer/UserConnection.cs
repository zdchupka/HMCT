using HylandMedConfig.Common;
using System;
using System.IO;

namespace HylandIMServer
{
	public class UserConnection
	{
		public UserClient Client
		{
			get;
			private set;
		}

		public UserConnection()
		{
			LastActivity = DateTime.Now;
		}

		public DateTime LastActivity
		{
			get;
			internal set;
		}

		public void SetClient( UserClient client )
		{
			Client = client;
		}

		public void SendCommand( string username, TcpCommands command )
		{
			SendArgs( (int)command, username );
		}

		private bool CanWrite()
		{
			return Client != null &&
				Client._Connection != null &&
				Client._bw != null &&
				Client._bw.BaseStream != null &&
				Client._bw.BaseStream.CanWrite &&
				Client.CanReceive;
		}

		public void SendArgs( params object[] args )
		{
			if( args != null && args.Length > 0 && CanWrite() )
			{
				try
				{
					for( int i = 0; i < args.Length; i++ )
					{
						object arg = args[i];
						if( arg is int )
						{
							Client._bw.Write( (int)arg );
						}
						else if( arg is long )
						{
							Client._bw.Write( (long)arg );
						}
						else if( arg is string )
						{
							Client._bw.Write( (string)arg );
						}
						else if( arg is double )
						{
							Client._bw.Write( (double)arg );
						}
						else if( arg is ChatUser[] )
						{
							Client._bw.WriteObject( (ChatUser[])arg );
						}
						else if( arg is ChatUserMessage )
						{
							Client._bw.WriteObject( (ChatUserMessage)arg );
						}
						else if( arg is ChatUser )
						{
							Client._bw.WriteObject( (ChatUser)arg );
						}
						else if( arg is ChatUserStats )
						{
							Client._bw.WriteObject( (ChatUserStats)arg );
						}
						else
						{
							throw new Exception( "Something went wrong" );
						}
					}
					Client._bw.Flush();
				}
				catch( IOException ) { }
			}
		}

		public void SendError( string error )
		{
			SendArgs( (int)TcpCommands.Error, error );
		}

		public void SendError( string format, params object[] args )
		{
			SendError( string.Format( format, args ) );
		}

		public void SendUserCommand( ChatUser user, TcpCommands command )
		{
			SendArgs( (int)command, user );
		}
	}
}
