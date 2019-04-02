using HylandMedConfig.Common;
using System.Collections;

namespace HylandMedConfig
{
	public class ChatUserComparer : IComparer
	{
		private ChatUser _currentUser;

		public ChatUserComparer( ChatUser currentUser )
		{
			_currentUser = currentUser;
		}

		public int Compare( object x, object y )
		{
			ChatUser objX = x as ChatUser;
			ChatUser objY = y as ChatUser;

			if( _currentUser == null )
			{
				return -1;
			}

			if( objX.Username == _currentUser.Username )
			{
				return -1;
			}
			else if( objY.Username == _currentUser.Username )
			{
				return 1;
			}

			int compareValue = objY.IsFoozReady.CompareTo( objX.IsFoozReady );
			if( compareValue == 0 )
			{
				compareValue = objY.IsOnline.CompareTo( objX.IsOnline );
			}
			if( compareValue == 0 )
			{
				compareValue = objX.IsBot.CompareTo( objY.IsBot );
			}
			if( compareValue == 0 )
			{
				compareValue = objY.IsActive.CompareTo( objX.IsActive );
			}
			if( compareValue == 0 )
			{
				compareValue = objX.DisplayNameResolved.CompareTo( objY.DisplayNameResolved );
			}
			return compareValue;
		}
	}
}
