using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HylandMedConfig.Common;

namespace HylandIMServer
{
	public class UserClient
	{
		public static object _lock = new object();
		private Program _Program;
		private TcpClient _TcpClient;
		private NetworkStream _NetStream;  // Raw-data stream of connection.
		public SslStream _Ssl;            // Encrypts connection using SSL.
		public BinaryReader _br;          // Read simple data
		public BinaryWriter _bw;          // Write simple data
		public UserConnection _Connection;
		private ChatUser _User;
		private const double InactivityTimeoutMinutes = 10;
		CancellationTokenSource _inactivityTaskCancelationSource = new CancellationTokenSource();
		private Task _inactivityTask = null;
		private const int _inactivityDelayMS = 30000;
		private const int _MAX_MOOD_LENGTH = 420;

        private static StringDictionary _proxyUsers = new StringDictionary();

        static UserClient()
        {
            _proxyUsers.Add("tstocker2", "tstocker");
        }

		public ChatUser User
		{
			get
			{
				return _User;
			}
		}

		public UserClient( Program p, TcpClient c )
		{
			_Program = p;
			_TcpClient = c;
		}

		private void CheckLastActivity()
		{
			if( _User != null && _Connection != null && _User.Status != UserStatus.Offline )
			{
				bool isIdle = ( DateTime.Now - _Connection.LastActivity ).TotalMinutes > InactivityTimeoutMinutes;

				if( isIdle && _User.Status == UserStatus.Active )
				{
					lock( _lock )
					{
						_User.Status = UserStatus.Inactive;
						_User.IsFoozReady = false;
						_User.IsTyping = false;

						SendArgsToAllUsers( (int)TcpCommands.UserIdle, _User.Username );
					}
				}
			}
		}

		public void SetupConnection()
		{
			_NetStream = _TcpClient.GetStream();
			_Ssl = new SslStream( _NetStream, false );

            try
            {
                _Ssl.AuthenticateAsServer( _Program.cert );

				_br = new BinaryReader( _Ssl, Encoding.UTF8 );
				_bw = new BinaryWriter( _Ssl, Encoding.UTF8 );

				_bw.Write( (int)TcpCommands.Hello );
				_bw.Flush();

				int hello = _br.ReadInt32();

				if( !_Program.ShuttingDown && hello == (int)TcpCommands.Hello )
				{
					if( (TcpCommands)_br.ReadInt32() == TcpCommands.Login )
					{
						LoginArgs args = _br.ReadObject<LoginArgs>();

						lock( _lock )
						{
							if( args.IsBot )
							{
								// If this username is already logged in, report error
								if( _Program.Users.Keys.Where( u => u.Username == args.Username ).Count() > 0 )
								{
									_bw.Write( (int)TcpCommands.Error );
									_bw.Write( "A bot with this username is already logged in" );
									_bw.Flush();
									return;
								}

								// Verify minimum version
								if( args.Version < Program.MinVersion )
								{
									_bw.Write( (int)TcpCommands.Error );
									_bw.Write( string.Format( "This HylandMedConfig.Bot version is not supported.  Please upgrade to version {0}", Program.MinVersion ) );
									_bw.Flush();
									return;
								}

								_User = new ChatUser( args.Username, args.DisplayName, args.Nickname, args.ImageUrl, true, args.BotCreatorUserName );
								if( !string.IsNullOrEmpty( args.PreviousMood ) )
								{
									_User.Mood = args.PreviousMood;
								}
								_User.SetClientVersion( args.Version );
								_Connection = new UserConnection();
								_Program.Users.Add( _User, _Connection );

								_Program.Stats.GetStats( args.Username ).LastLogin = DateTime.Now;

								_Program.Stats.Save();
							}
							// Normal user
							else
							{
								if( args.Version < Program.MinVersion )
								{
									_bw.Write( (int)TcpCommands.Error );
									_bw.Write( string.Format( "This version is not supported.  Please upgrade" ) );
									_bw.Flush();

									ConsoleMessage( "{0} tried to connect with version {1}", args.Username, args.Version );

									return;
								}

								foreach( var user in _Program.Users.Where( u => u.Key.Username == args.Username ) )
								{
									if( user.Value.Client != null )
									{
										_bw.Write( (int)TcpCommands.Error );
										_bw.Write( "A user with this username is already logged in" );
										_bw.Flush();
										return;
									}
									_User = user.Key;
									if( !string.IsNullOrEmpty( args.PreviousMood ) )
									{
										_User.Mood = args.PreviousMood;
									}
									_User.SetClientVersion( args.Version );
									_Connection = user.Value;
									_Program.Stats.GetStats( args.Username ).LastLogin = DateTime.Now;
									_Program.Stats.Save();
								}

								if( _User == null )
								{
									_bw.Write( (int)TcpCommands.Error );
									_bw.Write( "This user does not exist" );
									_bw.Flush();
									return;
								}
							}

						}
					}
					else
					{
						_bw.Write( (int)TcpCommands.Error );
						_bw.Write( "Invalid handshake" );
						_bw.Flush();
						return;
					}

                    // Login is a success, send down the user list and the logged on commend to all users
                    lock (_lock)
                    {
                        _Connection.SetClient( this );
						_Connection.LastActivity = DateTime.Now;
						_User.Status = UserStatus.Active;
						_bw.Write( (int)TcpCommands.OK );
						_bw.WriteObject( _Program.Users.Keys.ToArray() );
						_bw.Flush();

                        SendArgsToOtherUsers((int)TcpCommands.UserLoggedOn, _User);
                    }

                    _inactivityTask = Task.Factory.StartNew(() =>
                   {
                       while (!_inactivityTaskCancelationSource.Token.IsCancellationRequested)
                       {
                           CheckLastActivity();
                           Thread.Sleep(_inactivityDelayMS);
                       }
                   }, _inactivityTaskCancelationSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);

                    Receiver();
                }
            }
            catch( IOException )
			{
				// Thrown when client shuts down right when logging in
				if( _User != null )
				{
					_User.IsFoozReady = false;
					_User.Status = UserStatus.Offline;
					_User.IsTyping = false;
					_Connection.SetClient( null );

					lock( _lock )
					{
						if( _User.IsBot )
						{
							_Program.Users.Remove( _User );
						}

						SendArgsToAllUsers( (int)TcpCommands.UserLoggedOut, _User.Username );
					}

					ConsoleMessage( "{0} Logged Out Crazily", _User.Username );
				}

				CloseConn();
			}
			catch( Exception ex )
			{
				ConsoleMessage( "Exception has occured: {0}", ex.Message );
			}
		}

		public bool CanReceive
		{
			get;
			private set;
		}

		private void SendArgsToAllUsers( params object[] args )
		{
			foreach( UserConnection connection in _Program.Users.Values )
			{
				if( connection.Client != null )
				{
                    AppendLog($"send args to {connection.Client.User}");
					connection.SendArgs( args );
				}
			}
		}

		private void SendArgsToOtherUsers( params object[] args )
		{
			foreach( UserConnection connection in _Program.Users.Values )
			{
				if( connection.Client != null && _User.Username != connection.Client.User.Username )
				{
                    AppendLog($"send args to {connection.Client.User} 2");
					connection.SendArgs( args );
				}
			}
		}

		private void SendArgsToUsers( ChatUser[] users, params object[] args )
		{
			foreach( UserConnection connection in _Program.Users.Values )
			{
				if( connection.Client != null && users.Contains( connection.Client.User ) )
				{
                    AppendLog($"send args to {connection.Client.User} 3");
					connection.SendArgs( args );
				}
			}
		}

		private void TraceInformation( string format, params object[] args )
		{
			format = string.Format( format, args );
			Trace.TraceInformation( string.Format( "[{0}] {1}", DateTime.Now, format ) );
		}

		private void TraceError( string format, params object[] args )
		{
			format = string.Format( format, args );
			Trace.TraceError( string.Format( "[{0}] {1}", DateTime.Now, format ) );
			Console.WriteLine( string.Format( "[{0}] {1}", DateTime.Now, format ) );
		}

		private void TraceWarning( string format, params object[] args )
		{
			format = string.Format( format, args );
			Trace.TraceWarning( string.Format( "[{0}] {1}", DateTime.Now, format ) );
		}

		private void ConsoleMessage( string format, params object[] args )
		{
			format = string.Format( format, args );
			Console.WriteLine( string.Format( "[{0}] {1}", DateTime.Now, format ) );
			TraceInformation( format, args );
		}

        private StringBuilder _detailedMessage = new StringBuilder();

        private void AppendLog(string message)
        {
            _detailedMessage.AppendLine($"DEBUG [{DateTime.Now}] {User.Username} {message}");
        }

        private void ClearLog()
        {
            _detailedMessage.Clear();
            _detailedMessage.AppendLine("***DEBUG INFORMATION ***");
        }

		private void Receiver()
		{
			ConsoleMessage( "{0} Logged In", _User.Username );

			try
			{
				while( _TcpClient.Client.Connected )
				{
					CanReceive = true;
                    ClearLog();

                    AppendLog("reading int");
					TcpCommands command = (TcpCommands)_br.ReadInt32();
					_Connection.LastActivity = DateTime.Now;

					lock( _lock )
					{
                        AppendLog("entering lock");
						Stopwatch sw = Stopwatch.StartNew();
						ChatUser user;
						string username;
						ChatUser[] users;
						string mood;
						string pollID;
						string messageID;
						string choiceID;
						string error;
						MessageRating voteStatus;
						string pointData;
						string whiteboardBrush;
						double whiteboardThickness;
						UserConnection userConnection;

						if( _User.Status == UserStatus.Inactive )
						{
                            AppendLog("setting user status back to active");
							_User.Status = UserStatus.Active;
							SendArgsToAllUsers( (int)TcpCommands.UserActive, _User.Username );
						}

						TraceInformation( "{0} command received from {1}", command, _User.Username );

						switch( command )
						{
							case TcpCommands.SendMessage:

								ChatUserMessage message = _br.ReadObject<ChatUserMessage>();

                                if (_proxyUsers.ContainsKey(message.FromUser.Username))
                                {
                                    message.FromUser = _Program.Users.FirstOrDefault(u => u.Key.Username == _proxyUsers[message.FromUser.Username]).Key;
                                }

                                if ( message != null )
								{
									error = string.Empty;

									// Ensure that the 'FromUser' is actually this user
									//if( _User != message.FromUser )
									//{
									//	error = "You are not who you say you are";
									//}

									// Ensure that the IP address is the same as when they logged on
									var fromUser = _Program.Users.FirstOrDefault( u => u.Key == message.FromUser );
									//IPAddress thisAddress = ( (IPEndPoint)_TcpClient.Client.RemoteEndPoint ).Address;
									//IPAddress fromAddress = ( (IPEndPoint)fromUser.Value.Client._TcpClient.Client.RemoteEndPoint ).Address;
									//if( thisAddress != fromAddress )
									//{
									//	error = "You are not who you say you are";
									//}

									// Must re-stamp the date to avoid messages having differing dates based on clients
									message.Date = DateTime.Now;

									foreach( MessageValidator rule in _Program.RuleConfiguration.Rules )
									{
										if( !rule.Validate( message, _Program.Messages, out error ) )
										{
											break;
										}
									}

									foreach( string tag in message.Tags )
									{
										if( tag.Length < 2 || tag.Length > 30 )
										{
											error = "Tags must be between 2 and 30 characters";
											break;
										}
									}

									if( string.IsNullOrEmpty( error ) )
									{
										bool isPublic = message.ToUsers == null || message.ToUsers.Count == 0;

										if( isPublic )
										{
											_Program.Stats.GetStats( User.Username ).PublicMessagesSent++;
										}
										else
										{
											_Program.Stats.GetStats( User.Username ).PrivateMessagesSent++;
										}

										foreach( var recipient in _Program.Users )
										{
											if( recipient.Value.Client != null )
											{
												// This is sent to all users
												if( isPublic )
												{
													if( message.FromUser != recipient.Key )
													{
														_Program.Stats.GetStats( recipient.Key.Username ).PublicMessagesReceived++;
														recipient.Value.SendArgs( (int)TcpCommands.Received, message );
													}
												}
												// Whisper, send to 'ToUsers' and to the 'FromUser'
												else if( message.ToUsers.Contains( recipient.Key ) )
												{
													_Program.Stats.GetStats( recipient.Key.Username ).PrivateMessagesReceived++;
													recipient.Value.SendArgs( (int)TcpCommands.Received, message );
												}
											}
										}

										_Connection.SendArgs( (int)TcpCommands.SendMessageSuccess, message );

										_User.IsTyping = false;
										_Program.Stats.Save();
										_Program.Messages.Add( message );
									}
									else
									{
										_Connection.SendError( error );
									}
								}
								break;

							case TcpCommands.SendWhiteboardPoints:

								messageID = _br.ReadString();
								pointData = _br.ReadString();
								whiteboardBrush = _br.ReadString();
								whiteboardThickness = _br.ReadDouble();
								users = _br.ReadObject<ChatUser[]>();

								if( users == null || users.Length == 0 )
								{
									SendArgsToAllUsers(
										(int)TcpCommands.WhiteboardPointsReceived,
										User.Username,
										messageID,
										pointData,
										whiteboardBrush,
										whiteboardThickness );
								}
								else
								{
									SendArgsToUsers(
										users,
										(int)TcpCommands.WhiteboardPointsReceived,
										User.Username,
										messageID,
										pointData,
										whiteboardBrush,
										whiteboardThickness );
								}

								break;

							case TcpCommands.EnteredText:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );
								users = _br.ReadObject<ChatUser[]>();

								if( user != null && !user.IsTyping )
								{
									user.IsTyping = true;

									foreach( var recipient in _Program.Users.Values )
									{
										if( recipient.Client != null )
										{
											if( users == null || users.Length == 0 )
											{
												recipient.SendCommand( username, TcpCommands.UserEnteredText );
											}
											else if( users.Contains( recipient.Client.User ) )
											{
												recipient.SendCommand( username, TcpCommands.UserEnteredText );
											}
										}
									}
								}
								break;

							case TcpCommands.UserRemote:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									user.IsRemote = true;
									SendArgsToAllUsers( (int)TcpCommands.OnUserRemote, username );
								}
								break;

							case TcpCommands.UserNotRemote:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									user.IsRemote = false;
									SendArgsToAllUsers( (int)TcpCommands.OnUserNotRemote, username );
								}
								break;

							case TcpCommands.ClearedText:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );
								users = _br.ReadObject<ChatUser[]>();

								if( user != null && user.IsTyping )
								{
									user.IsTyping = false;

									foreach( var recipient in _Program.Users.Values )
									{
										if( recipient.Client != null )
										{
											if( users == null || users.Length == 0 )
											{
												recipient.SendCommand( username, TcpCommands.UserClearedText );
											}
											else if( users.Contains( recipient.Client.User ) )
											{
												recipient.SendCommand( username, TcpCommands.UserClearedText );
											}
										}
									}
								}
								break;

							case TcpCommands.ChangeMood:

                                AppendLog("ChangeMood | reading username");
								username = _br.ReadString();
                                AppendLog("ChangeMood | reading mood");
								mood = _br.ReadString();
                                AppendLog("ChangeMood | finding user from list");
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									if( mood.Length > _MAX_MOOD_LENGTH )
									{
										_Connection.SendError( "Mood must be {0} characters or less", _MAX_MOOD_LENGTH );
										break;
									}

									user.Mood = mood;

                                    AppendLog("ChangeMood | getting stats");
									_Program.Stats.GetStats( User.Username ).ChangedMood++;

                                    AppendLog("ChangeMood | saving stats");
									_Program.Stats.Save();

                                    SendArgsToOtherUsers((int)TcpCommands.MoodChanged, username, mood);
								}
								break;

							case TcpCommands.UserReadyForFooz:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null && !user.IsFoozReady )
								{
									List<UserConnection> readyUsers = _Program.Users.Select( u => u.Value ).Where( u => u.Client != null && u.Client.User.IsFoozReady ).ToList();
									if( readyUsers.Count < 4 )
									{
										UserConnection info;
										if( _Program.Users.TryGetValue( user, out info ) )
										{
											bool gameReady = readyUsers.Count == 3 && !readyUsers.Select( u => u.Client.User.Username ).Contains( info.Client.User.Username );
											info.Client.User.IsFoozReady = true;
											ChatUserStats stats = _Program.Stats.GetStats( info.Client.User.Username );
											stats.FoozGameAttempts++;

											if( gameReady )
											{
												readyUsers.Add( info );
												readyUsers.Shuffle();

												readyUsers.ForEach( u =>
												{
													_Program.Stats.GetStats( u.Client.User.Username ).FoozGamesRegistered++;
												} );
											}

											foreach( var recipient in _Program.Users.Values )
											{
												if( recipient.Client != null )
												{
													recipient.SendCommand( username, TcpCommands.UserReadyForFooz );

													if( gameReady )
													{
														users = readyUsers.Select( u => u.Client.User ).ToArray();
														recipient.SendArgs( (int)TcpCommands.FoozGameReady, users );
													}
												}
											}
										}
									}
									else
									{
										_Connection.SendError( "There are already 4 players ready for foosball" );
									}
								}

								break;

							case TcpCommands.UserNotReadyForFooz:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null && user.IsFoozReady )
								{
									user.IsFoozReady = false;
									SendArgsToAllUsers( (int)TcpCommands.UserNotReadyForFooz, username );
								}
								break;

							case TcpCommands.LockWhiteboard:

								messageID = _br.ReadString();

								SendArgsToAllUsers( (int)TcpCommands.WhiteboardLocked, messageID );

								break;

							case TcpCommands.UnlockWhiteboard:

								messageID = _br.ReadString();

								SendArgsToAllUsers( (int)TcpCommands.WhiteboardUnlocked, messageID );

								break;

							case TcpCommands.ClearWhiteboard:

								messageID = _br.ReadString();

								SendArgsToAllUsers( (int)TcpCommands.WhiteboardCleared, messageID );

								break;

							case TcpCommands.RequestUserStats:

								username = _br.ReadString();
								user = _Program.Users.Keys.FirstOrDefault( u => u.Username == username );

								if( user != null )
								{
									ChatUserStats stats = _Program.Stats.GetStats( username );
									_Connection.SendArgs( (int)TcpCommands.UserStatsReceived, user, stats );
								}
								else
								{
									_Connection.SendError( "Could not find user" );
								}

								break;

							case TcpCommands.UserVotedOnPoll:
								username = _br.ReadString();
								pollID = _br.ReadString();
								choiceID = _br.ReadString();

								SendArgsToAllUsers( (int)TcpCommands.UserVotedOnPoll, username, pollID, choiceID );

								break;

							case TcpCommands.ClosePoll:

								pollID = _br.ReadString();

								SendArgsToAllUsers( (int)TcpCommands.ClosePoll, pollID );

								break;

							case TcpCommands.UserRatedMessage:
								username = _br.ReadString();
								messageID = _br.ReadString();
								voteStatus = (MessageRating)_br.ReadInt32();

								userConnection = _Program.Users.Where( u => u.Key.Username == username ).Select( u => u.Value ).FirstOrDefault();
								if( voteStatus == MessageRating.ThumbsDown )
								{
									_Program.Stats.GetStats( username ).ThumbsDownGiven++;
								}
								else if( voteStatus == MessageRating.ThumbsUp )
								{
									_Program.Stats.GetStats( username ).ThumbsUpGiven++;
								}


								// Find the message that they rated
								ChatUserMessage messageRated = _Program.Messages.FirstOrDefault( m => m.ID == Guid.Parse( messageID ) );
								if( messageRated != null )
								{
									UserConnection ratedUserConnection = _Program.Users.Where( u => u.Key.Username == messageRated.FromUser.Username ).Select( u => u.Value ).FirstOrDefault();
									if( voteStatus == MessageRating.ThumbsDown )
									{
										_Program.Stats.GetStats( messageRated.FromUser.Username ).ThumbsDownReceived++;
									}
									else if( voteStatus == MessageRating.ThumbsUp )
									{
										_Program.Stats.GetStats( messageRated.FromUser.Username ).ThumbsUpReceived++;
									}

									SendArgsToAllUsers( (int)TcpCommands.UserRatedMessage, username, messageID, (int)voteStatus );
								}
								break;
							default:

								break;
						}
						if( sw.ElapsedMilliseconds > 500 )
						{
							ConsoleMessage( "{0} from user {1} took {2}ms", command, _User.Username, sw.ElapsedMilliseconds );
                            ConsoleMessage(_detailedMessage.ToString());
						}
					}
				}
			}
			catch( IOException ) { }
			catch( Exception ex )
			{
				TraceError( "Unexpected Error 487546: {0}", ex.Message );
			}

			if( !_Program.ShuttingDown )
			{
				_User.IsFoozReady = false;
				_User.Status = UserStatus.Offline;
				_User.IsTyping = false;
				_Connection.SetClient( null );

				lock( _lock )
				{
					if( _User.IsBot )
					{
						_Program.Users.Remove( _User );
					}

					SendArgsToAllUsers( (int)TcpCommands.UserLoggedOut, _User.Username );
				}

				ConsoleMessage( "{0} Logged Out", _User.Username );
			}

			CloseConn();
		}

		private void CloseConn() // Close connection
		{
			try
			{
				_br?.Close();
			}
			catch( Exception ex )
			{
				TraceError( "Error while _br.Close(): {0}", ex.Message );
			}

			try
			{
				_bw?.Close();
			}
			catch( Exception ex )
			{
				TraceError( "Error while _bw.Close(): {0}", ex.Message );
			}

			if( _Connection != null )
			{
				_Connection = null;
			}

			try
			{
				_Ssl?.Dispose();
			}
			catch( Exception ex )
			{
				TraceError( "Error while _Ssl.Dispose(): {0}", ex.Message );
			}
			try
			{
				_NetStream?.Dispose();
			}
			catch( Exception ex )
			{
				TraceError( "Error while _NetStream.Dispose(): {0}", ex.Message );
			}
			try
			{
				_TcpClient?.Close();
			}
			catch( Exception ex )
			{
				TraceError( "Error while _TcpClient.Close(): {0}", ex.Message );
			}

			if( _inactivityTask != null )
			{
				_inactivityTaskCancelationSource.Cancel();
				try
				{
					_inactivityTask.Wait();
				}
				catch( Exception ) { }
				_inactivityTask.Dispose();
			}
		}
	}
}
