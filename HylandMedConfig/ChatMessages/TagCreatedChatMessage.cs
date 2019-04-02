using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
	public class TagCreatedChatMessage : ChatUserMessage
	{
		private string _tag = string.Empty;
		/// <summary>
		/// Gets or sets the Tag of the message
		/// </summary>
		public string NewTag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
				OnPropertyChanged();
			}
		}

		public TagCreatedChatMessage( string tag )
			: base()
		{
			NewTag = tag;
		}
	}
}
