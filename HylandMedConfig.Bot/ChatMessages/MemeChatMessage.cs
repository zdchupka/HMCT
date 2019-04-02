using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class MemeChatMessage : ChatUserMessage
	{
		public override string Command
		{
			get
			{
				return ChatUserMessage.Commands.MEME;
			}
		}

		private string _imageUrl = string.Empty;
		private string _firstLine = string.Empty;
		private string _secondLine = string.Empty;
		private string _thirdLine = string.Empty;
		private string _fourthLine = string.Empty;

		public string ImageUrl
		{
			get { return _imageUrl; }
			set
			{
				_imageUrl = value;
				OnPropertyChanged();
				RefreshText();
			}
		}

		public string FirstLine
		{
			get
			{
				return _firstLine;
			}
			set
			{
				_firstLine = value;
				OnPropertyChanged();
				RefreshText();
			}
		}

		public string SecondLine
		{
			get
			{
				return _secondLine;
			}
			set
			{
				_secondLine = value;
				OnPropertyChanged();
				RefreshText();
			}
		}

		public string ThirdLine
		{
			get
			{
				return _thirdLine;
			}
			set
			{
				_thirdLine = value;
				OnPropertyChanged();
				RefreshText();
			}
		}

		public string FourthLine
		{
			get
			{
				return _fourthLine;
			}
			set
			{
				_fourthLine = value;
				OnPropertyChanged();
				RefreshText();
			}
		}

		private void RefreshText()
		{
			string text = string.Format( "{0}{1}{2}{1}{3}{1}{4}{1}{5}",
				ImageUrl,
				Environment.NewLine,
				FirstLine,
				SecondLine,
				ThirdLine,
				FourthLine );
			Text = text.TrimEnd( new char[] { '\r', '\n' } );
		}

		public static MemeChatMessage FromString(string text)
		{
			MemeChatMessage message = new MemeChatMessage();

			string[] values = text.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

			if( values.Length == 1 )
			{
				message.ImageUrl = values[0];
			}
			else if( values.Length == 2 )
			{
				message.FirstLine = values[1];
				message.ImageUrl = values[0];
			}
			else if( values.Length == 3 )
			{
				message.FirstLine = values[1];
				message.FourthLine = values[2];
				message.ImageUrl = values[0];
			}
			else if( values.Length == 4 )
			{
				message.FirstLine = values[1];
				message.ThirdLine = values[2];
				message.FourthLine = values[3];
				message.ImageUrl = values[0];
			}
			else if( values.Length == 5 )
			{
				message.FirstLine = values[1];
				message.SecondLine = values[2];
				message.ThirdLine = values[3];
				message.FourthLine = values[4];
				message.ImageUrl = values[0];
			}

			return message;
		}

		[Obsolete("Use FromString static method instead")]
		public MemeChatMessage( ChatUser fromUser, string text, List<ChatUser> toUsers )
		{
			MemeChatMessage message = FromString( text );
			ImageUrl = message.ImageUrl;
			FirstLine = message.FirstLine;
			SecondLine = message.SecondLine;
			ThirdLine = message.ThirdLine;
			FourthLine = message.FourthLine;
		}

		public MemeChatMessage()
		{

		}
	}
}
