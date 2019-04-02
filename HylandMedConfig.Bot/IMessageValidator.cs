using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace HylandMedConfig.Common
{

	[Serializable]
	
	public abstract class MessageValidator
	{
		public abstract bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error );
	}

	//internal class CharactersPerTimeSpanRule : IMessageValidator
	//{
	//	private long _maxCharacters;
	//	private TimeSpan _duration;

	//	public CharactersPerTimeSpanRule( long characters, TimeSpan duration )
	//	{
	//		_maxCharacters = characters;
	//		_duration = duration;
	//	}

	//	public bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
	//	{
	//		error = string.Empty;
	//		if( message is NormalChatUserMessage )
	//		{
	//			int previousCharacterCount = allMessages.OfType<NormalChatUserMessage>().Where( m => m.FromUser == message.FromUser && m.Date > DateTime.Now.Add( -_duration ) ).Sum( m => m.Text.Length );
	//			int thisMessageCharacterCount = message.Text.Length;
	//			if( previousCharacterCount + thisMessageCharacterCount > _maxCharacters )
	//			{
	//				error = string.Format( "You are only allowed {0} character(s) per {1} minute(s)", _maxCharacters, _duration.TotalMinutes );
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//}

	//internal class WebBrowserPerTimeSpanRule : IMessageValidator
	//{
	//	private long _limit;
	//	private TimeSpan _duration;

	//	public WebBrowserPerTimeSpanRule( long limit, TimeSpan duration )
	//	{
	//		_limit = limit;
	//		_duration = duration;
	//	}

	//	public bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
	//	{
	//		error = string.Empty;
	//		if( message is WebUrlChatMessage )
	//		{
	//			int imageCount = allMessages.OfType<WebUrlChatMessage>().Where( m => m.FromUser == message.FromUser && m.Date > DateTime.Now.Add( -_duration ) ).Count() + 1;
	//			if( imageCount > _limit )
	//			{
	//				error = string.Format( "You are only allowed {0} web page(s) per {1} minute(s)", _limit, _duration.TotalMinutes );
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//}

	//internal class HtmlMessagesPerTimeSpanRule : IMessageValidator
	//{
	//	private long _limit;
	//	private TimeSpan _duration;

	//	public HtmlMessagesPerTimeSpanRule( long limit, TimeSpan duration )
	//	{
	//		_limit = limit;
	//		_duration = duration;
	//	}

	//	public bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
	//	{
	//		error = string.Empty;
	//		if( message is HTMLChatMessage )
	//		{
	//			int imageCount = allMessages.OfType<HTMLChatMessage>().Where( m => m.FromUser == message.FromUser && m.Date > DateTime.Now.Add( -_duration ) ).Count() + 1;
	//			if( imageCount > _limit )
	//			{
	//				error = string.Format( "You are only allowed {0} html message(s) per {1} minute(s)", _limit, _duration.TotalMinutes );
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//}

	//internal class MessagesInARowRule : IMessageValidator
	//{
	//	private long _limit;

	//	public MessagesInARowRule( long limit )
	//	{
	//		_limit = limit;
	//	}

	//	public bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
	//	{
	//		error = string.Empty;

	//		ChatUserMessage lastReplyToMessage = allMessages.Reverse().FirstOrDefault( u => u.FromUser != message.FromUser && u.ToUsers.Count == message.ToUsers.Count );

	//		DateTime lastReply = lastReplyToMessage == null ? DateTime.MinValue : lastReplyToMessage.Date;

	//		int count = allMessages.Reverse().Where( u => u.Date > lastReply && u.FromUser == message.FromUser && u.ToUsers.SequenceEqual( message.ToUsers ) ).Count() + 1;

	//		if( count > _limit )
	//		{
	//			error = string.Format( "You are only allowed {0} message(s) in a row", _limit );
	//			return false;
	//		}
	//		return true;
	//	}
	//}

	//internal class ImagesPerTimeSpaneRule : IMessageValidator
	//{
	//	private long _limit;
	//	private TimeSpan _duration;

	//	public ImagesPerTimeSpaneRule( long limit, TimeSpan duration )
	//	{
	//		_limit = limit;
	//		_duration = duration;
	//	}

	//	public bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
	//	{
	//		error = string.Empty;
	//		if( !message.IsWhisper && message is ImageUrlChatMessage )
	//		{
	//			int imageCount = allMessages.OfType<ImageUrlChatMessage>().Where( m => m.FromUser == message.FromUser && m.Date > DateTime.Now.Add( -_duration ) && m.Loaded ).Count() + 1;
	//			if( imageCount > _limit )
	//			{
	//				error = string.Format( "You are only allowed {0} image(s) per {1} minute(s)", _limit, _duration.TotalMinutes );
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//}

	[Serializable]
	public class MessagesPerTimeSpaneRule : MessageValidator
	{
		[XmlAttribute( "Limit" )]
		public long Limit
		{
			get;
			set;
		}

		[XmlAttribute( "Minutes" )]
		public long Minutes
		{
			get;
			set;
		}

		public TimeSpan Duration
		{
			get
			{
				return TimeSpan.FromMinutes( Minutes );
			}
		}

		public MessagesPerTimeSpaneRule()
		{

		}

		public override bool Validate( ChatUserMessage message, IEnumerable<ChatUserMessage> allMessages, out string error )
		{
			error = string.Empty;
			if( !message.IsWhisper )
			{
				int messageCount = allMessages.Where( m => m.FromUser == message.FromUser && m.Date > DateTime.Now.Add( -Duration ) ).Count() + 1;
				if( messageCount > Limit )
				{
					error = string.Format( "You are only allowed {0} message(s) per {1} minute(s)", Limit, Duration.TotalMinutes );
					return false;
				}
			}
			return true;
		}
	}
}
