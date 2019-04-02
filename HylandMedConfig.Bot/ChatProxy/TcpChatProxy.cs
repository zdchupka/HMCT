using HylandMedConfig.Common.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Converters;

namespace HylandMedConfig.Common
{
	public class TcpChatProxy : ViewModelBase, IChatProxy, IDisposable
	{
		#region Declarations

		private object _writeLock = new object();
		private object _readLock = new object();
		private Thread _tcpThread;
		private TcpClient _client;
		private NetworkStream _netStream;
		private SslStream _ssl;
		private BinaryReader _br;
		private BinaryWriter _bw;
		private LoginArgs _loginArgs;
		private ObservableCollection<ChatUser> _users = new ObservableCollection<ChatUser>();
		private ReadOnlyObservableCollection<ChatUser> _usersReadOnly;
		internal ObservableCollection<ChatUserMessage> Messages = new ObservableCollection<ChatUserMessage>();
		private DispatcherTimer _reconnectTimer;
		private DispatcherTimer _remoteDesktopCheckTimer;
		private System.Threading.Timer _reconnectTimerConsole;
		private System.Threading.Timer _remoteDesktopCheckTimerConsole;
		public const int MaxMoodLength = 420;

		#endregion

		#region Construction

		internal TcpChatProxy( LoginArgs loginArgs, INicknameService nicknameService = null, ITagService tagService = null )
		{
			_loginArgs = loginArgs;
			_usersReadOnly = new ReadOnlyObservableCollection<ChatUser>( _users );

			BindingOperations.EnableCollectionSynchronization( Users, _writeLock );

			if( System.Windows.Application.Current == null )
			{
				_reconnectTimerConsole = new System.Threading.Timer( CheckForReconnectConsole, null, -1, -1 );

				if( !loginArgs.IsBot )
				{
					_remoteDesktopCheckTimerConsole = new System.Threading.Timer( CheckForTerminalSessionConsole, null, 5000, -1 );
				}
			}
			else
			{
				_reconnectTimer = new DispatcherTimer( TimeSpan.FromSeconds( 5 ), DispatcherPriority.Normal, CheckForReconnect, Dispatcher.CurrentDispatcher );
				_reconnectTimer.Stop();

				if( !loginArgs.IsBot )
				{
					_remoteDesktopCheckTimer = new DispatcherTimer( TimeSpan.FromSeconds( 5 ), DispatcherPriority.Normal, CheckForTerminalSession, Dispatcher.CurrentDispatcher );
					_remoteDesktopCheckTimer.Start();
				}
			}

			if( !loginArgs.IsBot )
			{
				IsRemote = SystemInformation.TerminalServerSession;
			}
			else
			{
				// Bots are never remote
				IsRemote = false;
			}

			NicknameService = nicknameService;
			TagService = tagService;

			Connect();
		}

		#endregion

		#region Properties

		public INicknameService NicknameService
		{
			get;
			private set;
		}

		public ITagService TagService
		{
			get;
			private set;
		}

		public bool IsConnected
		{
			get
			{
				return _client != null && _client.Connected;
			}
		}

		public bool IsRemote
		{
			get;
			private set;
		}

		private ChatUser _currentUser;
		public ChatUser CurrentUser
		{
			get { return _currentUser; }
			private set
			{
				_currentUser = value;
				OnPropertyChanged();
			}
		}

		public ReadOnlyObservableCollection<ChatUser> Users
		{
			get
			{
				return _usersReadOnly;
			}
		}

		#endregion

		#region Events

		public event EventHandler Reconnected;
		public event EventHandler<ChatErrorEventArgs> Error;
		public event EventHandler Disconnected;
		public event EventHandler<ChatUserEventArgs> UserLoggedOn;
		public event EventHandler<ChatUserEventArgs> UserLoggedOut;
		public event EventHandler<ChatReceivedEventArgs> MessageReceived;
		public event EventHandler<ChatUserEventArgs> UserEnteredText;
		public event EventHandler<ChatUserEventArgs> UserClearedText;
		public event EventHandler<ChatUserEventArgs> UserIdle;
		public event EventHandler<ChatUserEventArgs> UserActive;
		public event EventHandler<ChatUserEventArgs> UserFoozReady;
		public event EventHandler<ChatUserEventArgs> UserNotFoozReady;
		public event EventHandler<FoosballGameReadyEventArgs> FoozGameReady;
		public event EventHandler<ChatUserStatsEventArgs> ChatUserStatsReceived;
		public event EventHandler<ChatUserEventArgs> MoodChanged;
		public event EventHandler<UserVotedEventArgs> UserVoted;
		public event EventHandler<ChatReceivedEventArgs> MessageSentSuccessfully;
		public event EventHandler<UserRatedMessagedEventArgs> UserRatedMessage;
		public event EventHandler<WhiteboardDataEventArgs> WhiteboardDataReceived;
		public event EventHandler<PollClosedEventArgs> PollClosed;


		virtual protected void OnReconnected()
		{
			if( Reconnected != null )
			{
				Invoke( () =>
				{
					Reconnected?.Invoke( this, EventArgs.Empty );
				} );
			}
		}
		virtual protected void OnError( ChatErrorEventArgs e )
		{
			if( Error != null )
			{
				Invoke( () =>
				{
					Error?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnDisconnected()
		{
			if( Disconnected != null )
			{
				Invoke( () =>
				{
					Disconnected?.Invoke( this, EventArgs.Empty );
				} );
			}
		}
		virtual protected void OnUserLoggedOn( ChatUserEventArgs e )
		{
			if( UserLoggedOn != null )
			{
				Invoke( () =>
				{
					UserLoggedOn?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnMessageReceived( ChatReceivedEventArgs e )
		{
			if( MessageReceived != null )
			{
				Invoke( () =>
				{
					MessageReceived?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnMessageSentSuccessfully( ChatReceivedEventArgs e )
		{
			if( MessageSentSuccessfully != null )
			{
				Invoke( () =>
				{
					MessageSentSuccessfully?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserLoggedOut( ChatUserEventArgs e )
		{
			if( UserLoggedOut != null )
			{
				Invoke( () =>
				{
					UserLoggedOut?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserEnteredText( ChatUserEventArgs e )
		{
			if( UserEnteredText != null )
			{
				Invoke( () =>
				{
					UserEnteredText?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserClearedText( ChatUserEventArgs e )
		{
			if( UserClearedText != null )
			{
				Invoke( () =>
				{
					UserClearedText?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserIdle( ChatUserEventArgs e )
		{
			if( UserIdle != null )
			{
				Invoke( () =>
				{
					UserIdle?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserActive( ChatUserEventArgs e )
		{
			if( UserActive != null )
			{
				Invoke( () =>
				{
					UserActive?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserStatsReceived( ChatUserStatsEventArgs e )
		{
			if( ChatUserStatsReceived != null )
			{
				Invoke( () =>
				{
					ChatUserStatsReceived?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserFoozReady( ChatUserEventArgs e )
		{
			if( UserFoozReady != null )
			{
				Invoke( () =>
				{
					UserFoozReady?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnUserNotFoozReady( ChatUserEventArgs e )
		{
			if( UserNotFoozReady != null )
			{
				Invoke( () =>
				{
					UserNotFoozReady?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnFoozGameReady( FoosballGameReadyEventArgs e )
		{
			if( FoozGameReady != null )
			{
				Invoke( () =>
				{
					FoozGameReady?.Invoke( this, e );
				} );
			}
		}
		virtual protected void OnMoodChanged( ChatUserEventArgs e )
		{
			if( MoodChanged != null )
			{
				Invoke( () =>
				{
					MoodChanged?.Invoke( this, e );
				} );
			}
		}

		virtual protected void OnUserVoted( UserVotedEventArgs e )
		{
			if( UserVoted != null )
			{
				Invoke( () =>
				{
					UserVoted?.Invoke( this, e );
				} );
			}
		}

		virtual protected void OnWhiteboardDataReceived( WhiteboardChatMessage message, ChatUser user, string pointData, string brushData, double thickness )
		{
			Invoke( () =>
			{
				WhiteboardEntry entry = new WhiteboardEntry()
				{
					User = user,
					Points = (PointCollection)new PointCollectionValueSerializer().ConvertFromString( pointData, null ),
					StrokeThickness = thickness,
					Stroke = new BrushConverter().ConvertFromString( brushData ) as Brush,
				};
				message.Entries.Add( entry );
				WhiteboardDataReceived?.Invoke( this, new WhiteboardDataEventArgs() { Entry = entry, Message = message } );
			} );
		}

		virtual protected void OnWhiteboardLocked( WhiteboardChatMessage message )
		{
			Invoke( () =>
			{
				if( message != null )
				{
					message.IsLocked = true;
				}
			} );
		}

		virtual protected void OnWhiteboardUnlocked( WhiteboardChatMessage message )
		{
			Invoke( () =>
			{
				if( message != null )
				{
					message.IsLocked = false;
				}
			} );
		}

		virtual protected void OnWhiteboardCleared( WhiteboardChatMessage message )
		{
			Invoke( () =>
			{
				if( message != null )
				{
					message.Entries.Clear();
				}
			} );
		}

		virtual protected void OnUserRatedMessage( UserRatedMessagedEventArgs e )
		{
			if( UserRatedMessage != null )
			{
				Invoke( () =>
				 {
					 UserRatedMessage?.Invoke( this, e );
				 } );
			}
		}

		virtual protected void OnPollClosed( PollClosedEventArgs e )
		{
			if( PollClosed != null )
			{
				Invoke( () =>
				{
					PollClosed?.Invoke( this, e );
				} );
			}
		}

		#endregion

		private void CheckForTerminalSession( object sender, EventArgs e )
		{
			_remoteDesktopCheckTimer.Stop();

			if( _disposed )
			{
				return;
			}

			bool isRemote = SystemInformation.TerminalServerSession;

			if( CurrentUser != null && isRemote != CurrentUser.IsRemote )
			{
				if( isRemote )
				{
					NotifyUserRemote();
				}
				else
				{
					NotifyUserNotRemote();
				}
			}

			_remoteDesktopCheckTimer.Start();
		}

		private void CheckForTerminalSessionConsole( object state )
		{
			if( _disposed )
			{
				return;
			}

			try
			{

				bool isRemote = SystemInformation.TerminalServerSession;

				if( CurrentUser != null && isRemote != CurrentUser.IsRemote )
				{
					if( isRemote )
					{
						NotifyUserRemote();
					}
					else
					{
						NotifyUserNotRemote();
					}
				}
			}
			finally
			{
				_remoteDesktopCheckTimerConsole.Change( 5000, -1 );
			}
		}

		private void CheckForReconnect( object sender, EventArgs e )
		{
			_reconnectTimer.Stop();

			if( _disposed )
			{
				return;
			}

			try
			{
				Connect();
			}
			catch( ChatUserLoginException ex )
			{
				OnError( new ChatErrorEventArgs( ex.Message ) );
			}
		}

		private void CheckForReconnectConsole( object state )
		{
			_reconnectTimerConsole.Change( -1, -1 );

			if( _disposed )
			{
				return;
			}

			try
			{
				Connect();
			}
			catch( ChatUserLoginException ex )
			{
				OnError( new ChatErrorEventArgs( ex.Message ) );
			}
		}

		private void Connect()
		{
			try
			{
				int port = Settings.Default.Port;
#if DEBUG
				port = Settings.Default.DebugPort;
#endif
				_client = new TcpClient( Settings.Default.IPAddress, port );

				_netStream = _client.GetStream();
				_ssl = new SslStream( _netStream, false, new RemoteCertificateValidationCallback( ValidateCert ) );

				_ssl.AuthenticateAsClient( "InstantMessengerServer" );

				_br = new BinaryReader( _ssl, Encoding.UTF8 );
				_bw = new BinaryWriter( _ssl, Encoding.UTF8 );


				lock( _readLock )
				{
					lock( _writeLock )
					{
						int hello = _br.ReadInt32();
						if( hello == (int)TcpCommands.Hello )
						{
							_bw.Write( (int)TcpCommands.Hello );
							_bw.Flush();

							_bw.Write( (int)TcpCommands.Login );
							_bw.WriteObject( _loginArgs );
							_bw.Flush();

							int ans = _br.ReadInt32();
							if( ans == (int)TcpCommands.OK )
							{
								_users.Clear();
								foreach( ChatUser user in _br.ReadObject<ChatUser[]>() )
								{
									user.Nickname = NicknameService?.GetNickname( user );
									_users.Add( user );
								}

								CurrentUser = _users.FirstOrDefault( u => u.Username == _loginArgs.Username );

								OnReconnected();

								_tcpThread = new Thread( new ParameterizedThreadStart( Receiver ) );
								_tcpThread.Start( _loginArgs );
							}
							else
							{
								string error = _br.ReadString();
								throw new ChatUserLoginException( error );
							}
						}
					}
				}
			}
			catch( SocketException )
			{
				// If this is the first time we are connecting throw exception
				if( _client == null )
				{
					throw new ChatUserLoginException( "The server is down, try again later" );
				}

				if( _client.Connected )
				{
					_client.Close();
					_br.Close();
					_bw.Close();
					_ssl.Close();
					_netStream.Close();
				}

				// When we become disconnected, do not check for reconnect or idleness of the current user
				_reconnectTimer?.Start();
				_reconnectTimerConsole?.Change( 5000, -1 );
			}
			catch( IOException )
			{
				// If this is the first time we are connecting throw exception
				if( _client == null )
				{
					throw new ChatUserLoginException( "The server is down, try again later" );
				}

				if( _client.Connected )
				{
					_client.Close();
					_br.Close();
					_bw.Close();
					_ssl.Close();
					_netStream.Close();
				}

				// When we become disconnected, do not check for reconnect or idleness of the current user
				_reconnectTimer?.Start();
				_reconnectTimerConsole?.Change( 5000, -1 );
			}
		}

		public void SendMessage( ChatUserMessage message )
		{
			if( _client == null || !_client.Connected )
			{
				OnError( new ChatErrorEventArgs( "You are not connected" ) );
				return;
			}

			// Validation must be client side as xaml messages cannot be verified on the server side do to threading issues
			if( message is IValidateMessage )
			{
				string error;
				if( !( message as IValidateMessage ).Validate( out error ) )
				{
					OnError( new ChatErrorEventArgs( error ) );
					return;
				}
			}

			//Trace.TraceInformation( "Sending Message" );
			if( message is HyperlinkChatMessage )
			{
				( message as HyperlinkChatMessage ).DownloadMetadata();
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					lock( _writeLock )
					{
						_bw.Write( (int)TcpCommands.SendMessage );
						_bw.WriteObject( message );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed to send
			}
			catch( IOException )
			{
				// Do nothing, failed
			}
		}

		public void Vote( PollChoice choice )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					lock( _writeLock )
					{
						//Trace.TraceInformation( "Sending UserVotedOnPoll" );
						_bw.Write( (int)TcpCommands.UserVotedOnPoll );
						_bw.Write( CurrentUser.Username );
						_bw.Write( choice.PollID.ToString() );
						_bw.Write( choice.ID.ToString() );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public void ClosePoll( PollMessage message )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			if( message.FromUser != CurrentUser )
			{
				OnError( new ChatErrorEventArgs( "You must be the creator to close this poll" ) );
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					lock( _writeLock )
					{
						_bw.Write( (int)TcpCommands.ClosePoll );
						_bw.Write( message.ID.ToString() );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public void RateMessage( ChatUserMessage message, MessageRating rating )
		{
			if( _client == null || !_client.Connected )
			{
				OnError( new ChatErrorEventArgs( "You are not connected" ) );
				return;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					lock( _writeLock )
					{
						//Trace.TraceInformation( "Sending UserRatedMessage" );
						_bw.Write( (int)TcpCommands.UserRatedMessage );
						_bw.Write( CurrentUser.Username );
						_bw.Write( message.ID.ToString() );
						_bw.Write( (int)rating );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public bool SetMood( string mood )
		{
			if( _client == null || !_client.Connected )
			{
				OnError( new ChatErrorEventArgs( "You are not connected" ) );
				return false;
			}

			if( string.IsNullOrEmpty( mood ) )
			{
				mood = string.Empty;
			}

			if( mood.Length > MaxMoodLength )
			{
				OnError( new ChatErrorEventArgs( $"Mood must be {MaxMoodLength} characters or less" ) );
				return false;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					lock( _writeLock )
					{
						//Trace.TraceInformation( "Sending MoodChanged" );
						_bw.Write( (int)TcpCommands.ChangeMood );
						_bw.Write( CurrentUser.Username );
						_bw.Write( mood );
						_bw.Flush();
					}

					if( CurrentUser != null )
					{
						_loginArgs.PreviousMood = mood;
					}
					CurrentUser.Mood = mood;
					return true;
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
			catch( IOException )
			{
				// Do nothing, failed
			}
			return false;
		}

		public void NotifyTextEntered( IList<ChatUser> users = null )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			if( users == null )
			{
				users = new List<ChatUser>();
			}

			if( !CurrentUser.IsTyping )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						//Trace.TraceInformation( "Sending EnteredText" );

						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.EnteredText );
							_bw.Write( CurrentUser.Username );
							_bw.WriteObject( users.ToArray() );
							_bw.Flush();
						}
						CurrentUser.IsTyping = true;
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void NotifyTextCleared( IList<ChatUser> users = null )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			if( users == null )
			{
				users = new List<ChatUser>();
			}

			if( CurrentUser.IsTyping )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						//Trace.TraceInformation( "Sending ClearedText" );

						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.ClearedText );
							_bw.Write( CurrentUser.Username );
							_bw.WriteObject( users.ToArray() );
							_bw.Flush();
						}

						CurrentUser.IsTyping = false;
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void NotifyActive()
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			if( CurrentUser != null && CurrentUser.Status != UserStatus.Active )
			{
				//Trace.TraceInformation( "Sending UserActive" );
				//_bw.Write( (int)TcpCommands.UserActive );
				//_bw.Write( CurrentUser.Username );
				//_bw.Flush();
			}
		}

		public bool NotifyFoozReady()
		{
			if( _client == null || !_client.Connected )
			{
				return false;
			}

#if !DEBUG
			if( SystemInformation.TerminalServerSession )
			{
				CurrentUser.IsFoozReady = false;
				OnError( new ChatErrorEventArgs( "You may not +1 from a Remote Desktop Session" ) );
				return false;
			}
#endif

			if( !CurrentUser.IsFoozReady )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						//Trace.TraceInformation( "Sending UserReadyForFooz" );

						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.UserReadyForFooz );
							_bw.Write( CurrentUser.Username );
							_bw.Flush();
						}
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
			return true;
		}

		public void NotifyNotFoozReady()
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			if( CurrentUser.IsFoozReady )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						//Trace.TraceInformation( "Sending UserNotReadyForFooz" );

						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.UserNotReadyForFooz );
							_bw.Write( CurrentUser.Username );
							_bw.Flush();
						}
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void NotifyUserRemote()
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					//Trace.TraceInformation( "Sending UserRemote" );
					lock( _writeLock )
					{
						_bw.Write( (int)TcpCommands.UserRemote );
						_bw.Write( CurrentUser.Username );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public void NotifyUserNotRemote()
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					//Trace.TraceInformation( "Sending UserNotRemote" );

					lock( _writeLock )
					{
						_bw.Write( (int)TcpCommands.UserNotRemote );
						_bw.Write( CurrentUser.Username );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public void RequestStats( string username )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			try
			{
				if( _bw.BaseStream.CanWrite )
				{
					//Trace.TraceInformation( "Sending RequestUserStats" );

					lock( _writeLock )
					{
						_bw.Write( (int)TcpCommands.RequestUserStats );
						_bw.Write( username );
						_bw.Flush();
					}
				}
			}
			catch( SocketException )
			{
				// Do nothing, it failed
			}
		}

		public void SendWhiteboardData( ChatUserMessage message, WhiteboardEntry entry )
		{
			if( _client == null || !_client.Connected )
			{
				return;
			}

			WhiteboardChatMessage whiteboardMessage = message as WhiteboardChatMessage;
			if( whiteboardMessage != null && ( !whiteboardMessage.IsLocked || whiteboardMessage.FromUser == CurrentUser ) )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						string brush = new BrushConverter().ConvertToString( entry.Stroke );
						string hex = ( (int)( 0xFF * entry.Stroke.Opacity ) ).ToString( "X" );
						brush = brush.Replace( "#FF", "#" + hex );
						List<ChatUser> toUsers = new List<ChatUser>();
						if( message.ToUsers != null && message.ToUsers.Count > 0 )
						{
							toUsers = message.ToUsers;

							if( !toUsers.Contains( message.FromUser ) )
							{
								toUsers.Add( message.FromUser );
							}

							if( toUsers.Contains( CurrentUser ) )
							{
								toUsers.Remove( CurrentUser );
							}
						}

						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.SendWhiteboardPoints );
							_bw.Write( message.ID.ToString() );
							_bw.Write( new PointCollectionValueSerializer().ConvertToString( entry.Points, null ) );
							_bw.Write( brush );
							_bw.Write( entry.StrokeThickness );
							_bw.WriteObject( toUsers.ToArray() );
							_bw.Flush();
						}
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void LockWhiteboard( WhiteboardChatMessage message )
		{
			if( _client == null || !_client.Connected || message == null )
			{
				return;
			}

			if( !message.IsLocked && message.FromUser == CurrentUser )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.LockWhiteboard );
							_bw.Write( message.ID.ToString() );
							_bw.Flush();
						}
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void UnlockWhiteboard( WhiteboardChatMessage message )
		{
			if( _client == null || !_client.Connected || message == null )
			{
				return;
			}

			if( message.IsLocked && message.FromUser == CurrentUser )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.UnlockWhiteboard );
							_bw.Write( message.ID.ToString() );
							_bw.Flush();
						}
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		public void ClearWhiteboard( WhiteboardChatMessage message )
		{
			if( _client == null || !_client.Connected || message == null )
			{
				return;
			}

			if( message.FromUser == CurrentUser )
			{
				try
				{
					if( _bw.BaseStream.CanWrite )
					{
						lock( _writeLock )
						{
							_bw.Write( (int)TcpCommands.ClearWhiteboard );
							_bw.Write( message.ID.ToString() );
							_bw.Flush();
						}

						message.Entries.Clear();
					}
				}
				catch( SocketException )
				{
					// Do nothing, it failed
				}
			}
		}

		private void Receiver( object args = null )
		{
			try
			{
				while( _client != null && _client.Connected && _br != null && _br.BaseStream != null && !_disposed )
				{
					TcpCommands command = (TcpCommands)_br.ReadInt32();

					lock( _readLock )
					{
						string username;
						ChatUser user;
						string mood;
						string choiceID;
						string pollID;
						string error;
						string messageID;
						MessageRating rating;
						ChatUserMessage message;
						string whiteboardBrush;
						double whiteboardThickness;

						//Trace.TraceInformation( "Command Received: {0}", command.ToString() );

						switch( command )
						{
							case TcpCommands.Received:
								try
								{
									message = _br.ReadObject<ChatUserMessage>();

									user = _users.FirstOrDefault( u => u == message.FromUser );
									if( user != null )
									{
										user.IsTyping = false;
									}

									message.FromUser = user;

									if( message.ToUsers != null )
									{
										for( int i = 0; i < message.ToUsers.Count; i++ )
										{
											user = _users.FirstOrDefault( u => u == message.ToUsers[i] );
											if( user != null )
											{
												message.ToUsers[i] = user;
											}
										}
									}

									if( TagService != null )
									{
										message.IsHidden = TagService.ContainsHiddenTag( message.Tags );
									}

									Messages.Add( message );

									OnMessageReceived( new ChatReceivedEventArgs( message ) );
								}
								catch( SerializationException )
								{
									OnError( new ChatErrorEventArgs( "Unrecognized message received, please upgrade" ) );
								}
								break;

							case TcpCommands.UserLoggedOn:
								user = _br.ReadObject<ChatUser>();

								if( !_users.Contains( user ) )
								{
									user.Nickname = NicknameService?.GetNickname( user );
									_users.Add( user );
								}
								else
								{
									ChatUser existingUser = _users.FirstOrDefault( u => u == user );
									_users.Remove( existingUser );
									existingUser.Mood = user.Mood;
									existingUser.IsRemote = user.IsRemote;
									existingUser.Status = UserStatus.Active;
									user = existingUser;
									_users.Add( user );
								}

								OnUserLoggedOn( new ChatUserEventArgs( user ) );
								break;

							case TcpCommands.UserLoggedOut:
								username = _br.ReadString();

								user = Users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									_users.Remove( user );
									if( !user.IsBot )
									{
										user.Status = UserStatus.Offline;
										user.IsTyping = false;
										user.IsFoozReady = false;
										_users.Add( user );
									}

									OnUserLoggedOut( new ChatUserEventArgs( user ) );
								}
								break;

							case TcpCommands.AppShutdown:
								//HandleDisconnected();
								break;
							case TcpCommands.UserEnteredText:
								username = _br.ReadString();

								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									if( user == CurrentUser )
									{
										CurrentUser.IsTyping = true;
									}
									else
									{
										// Removing / adding here so that any sort for the users view is automatically updated
										_users.Remove( user );
										user.IsTyping = true;
										_users.Add( user );
										OnUserEnteredText( new ChatUserEventArgs( user ) );
									}
								}

								break;

							case TcpCommands.UserClearedText:
								username = _br.ReadString();

								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									if( user == CurrentUser )
									{
										CurrentUser.IsTyping = false;
									}
									else
									{
										_users.Remove( user );
										user.IsTyping = false;
										_users.Add( user );
										OnUserClearedText( new ChatUserEventArgs( user ) );
									}
								}
								break;

							case TcpCommands.UserIdle:
								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									_users.Remove( user );
									user.Status = UserStatus.Inactive;
									user.IsFoozReady = false;
									user.IsTyping = false;
									_users.Add( user );
									OnUserIdle( new ChatUserEventArgs( user ) );
								}
								break;

							case TcpCommands.UserActive:
								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									_users.Remove( user );
									user.Status = UserStatus.Active;
									_users.Add( user );
									OnUserActive( new ChatUserEventArgs( user ) );
								}
								break;

							case TcpCommands.UserReadyForFooz:
								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									_users.Remove( user );
									user.IsFoozReady = true;
									_users.Add( user );
									OnUserFoozReady( new ChatUserEventArgs( user ) );
								}
								break;
							case TcpCommands.UserNotReadyForFooz:
								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									_users.Remove( user );
									user.IsFoozReady = false;
									_users.Add( user );
									OnUserNotFoozReady( new ChatUserEventArgs( user ) );
								}
								break;
							case TcpCommands.UserStatsReceived:
								user = _br.ReadObject<ChatUser>();
								user = _users.FirstOrDefault( u => u == user );
								ChatUserStats stats = _br.ReadObject<ChatUserStats>();
								OnUserStatsReceived( new ChatUserStatsEventArgs( user, stats ) );
								break;
							case TcpCommands.FoozGameReady:
								ChatUser[] users = _br.ReadObject<ChatUser[]>();
								OnFoozGameReady( new FoosballGameReadyEventArgs(
									users.Take( 1 ).FirstOrDefault(),
									users.Skip( 1 ).Take( 1 ).FirstOrDefault(),
									users.Skip( 2 ).Take( 1 ).FirstOrDefault(),
									users.Skip( 3 ).Take( 1 ).FirstOrDefault() ) );
								break;

							case TcpCommands.MoodChanged:
								username = _br.ReadString();
								mood = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									user.Mood = mood;
									OnMoodChanged( new ChatUserEventArgs( user ) );
								}
								break;

							case TcpCommands.SendMessageSuccess:

								message = _br.ReadObject<ChatUserMessage>();
								message.FromUser = CurrentUser;
								Messages.Add( message );
								OnMessageSentSuccessfully( new ChatReceivedEventArgs( message ) );
								break;

							case TcpCommands.UserVotedOnPoll:

								username = _br.ReadString();
								pollID = _br.ReadString();
								choiceID = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									foreach( PollMessage poll in Messages.OfType<PollMessage>().Where( m => m.ID == Guid.Parse( pollID ) ) )
									{
										PollChoice choice = poll.Choices.FirstOrDefault( c => c.ID == Guid.Parse( choiceID ) );
										if( choice != null )
										{
											OnUserVoted( new UserVotedEventArgs( user, poll, choice ) );
										}
									}
								}
								break;

							case TcpCommands.OnUserRemote:

								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									user.IsRemote = true;
								}
								break;

							case TcpCommands.OnUserNotRemote:

								username = _br.ReadString();
								user = _users.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									user.IsRemote = false;
								}
								break;

							case TcpCommands.WhiteboardPointsReceived:

								username = _br.ReadString();
								pollID = _br.ReadString();
								string pointData = _br.ReadString();
								whiteboardBrush = _br.ReadString();
								whiteboardThickness = _br.ReadDouble();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									foreach( WhiteboardChatMessage whiteboardMessage in Messages.OfType<WhiteboardChatMessage>().Where( m => m.ID == Guid.Parse( pollID ) ) )
									{
										OnWhiteboardDataReceived( whiteboardMessage, user, pointData, whiteboardBrush, whiteboardThickness );
									}
								}

								break;

							case TcpCommands.WhiteboardLocked:

								messageID = _br.ReadString();

								foreach( WhiteboardChatMessage whiteboardMessage in Messages.OfType<WhiteboardChatMessage>().Where( m => m.ID == Guid.Parse( messageID ) ) )
								{
									OnWhiteboardLocked( whiteboardMessage );
								}

								break;

							case TcpCommands.WhiteboardUnlocked:

								messageID = _br.ReadString();

								foreach( WhiteboardChatMessage whiteboardMessage in Messages.OfType<WhiteboardChatMessage>().Where( m => m.ID == Guid.Parse( messageID ) ) )
								{
									OnWhiteboardUnlocked( whiteboardMessage );
								}

								break;

							case TcpCommands.WhiteboardCleared:

								messageID = _br.ReadString();

								foreach( WhiteboardChatMessage whiteboardMessage in Messages.OfType<WhiteboardChatMessage>().Where( m => m.ID == Guid.Parse( messageID ) ) )
								{
									OnWhiteboardCleared( whiteboardMessage );
								}

								break;

							case TcpCommands.UserRatedMessage:

								username = _br.ReadString();
								messageID = _br.ReadString();
								rating = (MessageRating)_br.ReadInt32();
								user = _users.FirstOrDefault( u => u.Username == username );
								if( user != null )
								{
									foreach( ChatUserMessage m in Messages.Where( m => m.ID == Guid.Parse( messageID ) ) )
									{
										MessageRating previousRating = MessageRating.Neutral;
										Invoke( new Action( () => m.ChangeRatingForUser( user, rating, out previousRating ) ) );
										OnUserRatedMessage( new UserRatedMessagedEventArgs( user, m, rating, previousRating ) );
									}
								}
								break;

							case TcpCommands.ClosePoll:

								pollID = _br.ReadString();
								Guid guidPollID = Guid.Parse( pollID );

								foreach( PollMessage m in Messages.OfType<PollMessage>().Where( m => m.ID == guidPollID ) )
								{
									m.Close();
									OnPollClosed( new PollClosedEventArgs( m ) );
								}
								break;

							case TcpCommands.Error:
								error = _br.ReadString();
								OnError( new ChatErrorEventArgs( error ) );
								break;

						}
					}
				}
			}
			catch( IOException )
			{
				HandleDisconnected();
			}
			catch( ObjectDisposedException )
			{
				HandleDisconnected();
			}
			catch( ThreadAbortException )
			{
				// do nothing, shutting down
			}

			HandleDisconnected();
		}

		private void HandleDisconnected()
		{
			if( _disposed ) { return; }

			// If we aren't connected
			if( _client == null || !_client.Connected )
			{
				_reconnectTimer?.Start();
				_reconnectTimerConsole?.Change( 5000, -1 );

				OnDisconnected();
				return;
			}

			_client.Close();
			_br.Close();
			_bw.Close();
			_ssl.Close();
			_netStream.Close();

			// When we become disconnected, do not check for reconnect or idleness of the current user
			_reconnectTimer?.Start();
			_reconnectTimerConsole?.Change( 5000, -1 );

			OnDisconnected();
		}

		private void Invoke( Action action )
		{
			try
			{
				if( System.Windows.Application.Current != null )
				{
					System.Windows.Application.Current.Dispatcher.Invoke( action );
				}
				else
				{
					action();
				}
			}
			catch( TaskCanceledException )
			{

			}
		}

		private static bool ValidateCert( object sender, X509Certificate certificate,
			  X509Chain chain, SslPolicyErrors sslPolicyErrors )
		{
			// Allow untrusted certificates.
			return true;
		}

		#region IDisposable Support
		private bool _disposed = false; // To detect redundant calls

		protected virtual void Dispose( bool disposing )
		{
			if( !_disposed )
			{
				//lock( _readLock )
				{
					if( !_disposed )
					{
						if( disposing )
						{
							try
							{
								_tcpThread.Abort();
							}
							catch( Exception )
							{

							}
							if( _br != null )
							{
								_br.Dispose();
							}
							if( _bw != null )
							{
								_bw.Dispose();
							}
							if( _ssl != null )
							{
								_ssl.Dispose();
							}
							if( _netStream != null )
							{
								_netStream.Dispose();
							}
							if( _client != null )
							{
								_client.Close();
							}
							if( _reconnectTimer != null )
							{
								_reconnectTimer.Stop();
							}
							_reconnectTimerConsole?.Dispose();
							_remoteDesktopCheckTimerConsole?.Dispose();
						}

						_disposed = true;
					}
				}
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose( true );
		}

		#endregion
	}
}
