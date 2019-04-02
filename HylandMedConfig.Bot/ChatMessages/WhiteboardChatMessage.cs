using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Media;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class WhiteboardChatMessage : ChatUserMessage
	{
		private bool _isLocked = true;

		public override string Command
		{
			get
			{
				return ChatUserMessage.Commands.WHITEBOARD;
			}
		}

		public string ImageUrl
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

		/// <summary>
		/// Gets a boolean indicating if this image loaded on the client
		/// </summary>
		public bool Loaded
		{
			get;
			internal set;
		}

		[Obsolete( "Use parameterless constructor instead" )]
		public WhiteboardChatMessage( ChatUser fromUser, string imageUrl, List<ChatUser> toUsers = null )
			: this()
		{
			FromUser = fromUser;
			ImageUrl = imageUrl;
			ToUsers = toUsers;
		}

		public bool IsLocked
		{
			get
			{
				return _isLocked;
			}
			internal set
			{
				_isLocked = value;
				OnPropertyChanged();
			}

		}

		private ObservableCollection<WhiteboardEntry> _entries = new ObservableCollection<WhiteboardEntry>();

		public ObservableCollection<WhiteboardEntry> Entries
		{
			get
			{
				if( _entries == null )
				{
					_entries = new ObservableCollection<WhiteboardEntry>();
				}
				return _entries;
			}
		}

		public WhiteboardChatMessage()
		{
			PropertyChanged += WhiteboardChatMessage_PropertyChanged;
		}

		private void WhiteboardChatMessage_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if( e.PropertyName == nameof( FromUser ) )
			{
				if( FromUser.IsBot )
				{
					IsLocked = false;
				}
			}
		}
	}

	[Serializable]
	public class WhiteboardEntry : ViewModelBase
	{
		public Guid ID
		{
			get;
			private set;
		}

		public ChatUser User
		{
			get;
			set;
		}

		public PointCollection Points
		{
			get;
			set;
		}

		public Brush Stroke
		{
			get;
			set;
		}

		public double StrokeThickness
		{
			get;
			set;
		}

		public WhiteboardEntry()
		{
			ID = Guid.NewGuid();
			Stroke = Brushes.Black;
			StrokeThickness = 4d;
		}
	}
}
