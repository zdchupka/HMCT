using HylandMedConfig.Common;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using HylandMedConfig.Attached;

namespace HylandMedConfig
{
	public class CombinedNormalChatUserMessages : ChatUserMessage
	{
		public ObservableCollection<NormalChatUserMessage> Messages
		{
			get;
			private set;
		}

		public ICollectionView MessagesView
		{
			get;
			private set;
		}

		public override string GetSpeechText()
		{
			StringBuilder text = new StringBuilder();
			foreach( string m in Messages.Select( m => m.Text ) )
			{
				text.AppendLine( m );
			}
			return string.Format( "{0} says: {1}", FromUser.DisplayNameResolved, text );
		}

		public CombinedNormalChatUserMessages( NormalChatUserMessage initialMessage )
		{
			FromUser = initialMessage.FromUser;
			Text = initialMessage.Text;
			ToUsers = initialMessage.ToUsers;
			Tags = initialMessage.Tags;
			Messages = new ObservableCollection<NormalChatUserMessage>() { initialMessage };

			MessagesView = CollectionViewSource.GetDefaultView( Messages );
		}

		public void AddMessage( NormalChatUserMessage message )
		{
			Date = message.Date;
			Messages.Add( message );
			OnPropertyChanged( nameof( Date ) );
		}
	}
}
