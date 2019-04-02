using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class XamlChatMessage : ChatUserMessage
	{
		public override string Command
		{
			get
			{
				return ChatUserMessage.Commands.XAML;
			}
		}

		public string Xaml
		{
			get
			{
				return Text;
			}
			set
			{
				Text = value;
			}
		}

		[Obsolete( "Use FromString static method instead" )]
		public XamlChatMessage( ChatUser fromUser, string xaml, List<ChatUser> toUsers = null )
		{
			FromUser = fromUser;
			Xaml = xaml;
			ToUsers = toUsers;
		}

		public XamlChatMessage()
		{

		}

		/// <summary>
		/// Validate that the xaml is valid
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool ValidateXaml( out string error )
		{
			string localError = string.Empty;
			Invoke( () =>
			 {
				 try
				 {
					 ParserContext context = new ParserContext();
					 context.XmlnsDictionary.Add( "", "http://schemas.microsoft.com/winfx/2006/xaml/presentation" );
					 context.XmlnsDictionary.Add( "x", "http://schemas.microsoft.com/winfx/2006/xaml" );
					 context.XmlnsDictionary.Add( "giftoolkit", "http://wpfanimatedgif.codeplex.com" );
					 context.XmlnsDictionary.Add( "hmct", "clr-namespace:HylandMedConfig.XamlControls;assembly=HylandMedConfig" );
					 object content = XamlReader.Load( new MemoryStream( Encoding.UTF8.GetBytes( Text ) ), context );

					 Window window = new Window();
					 window.Content = content;
				 }
				 catch( Exception ex )
				 {
					 localError = ex.Message;
				 }
			 } );
			error = localError;
			return string.IsNullOrEmpty( error );
		}

		private static void Invoke( Action action )
		{
			Dispatcher.CurrentDispatcher.Invoke(
					DispatcherPriority.Normal,
					(Action)delegate ()
					{
						action();
					} );
		}
	}
}
