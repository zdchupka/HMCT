using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Attached;

namespace HylandMedConfig.ChatMessages
{
	public class ChatUserStatsMessage : ChatUserMessage
	{
		public ChatUserStatsMessage( ChatUser user, ChatUserStats stats, EmployeeSearchResult employee )
		{
			User = user;
			Stats = stats;

			if( employee != null )
			{
				DisplayName = employee.DisplayName;
			}
			else if( User != null )
			{
				DisplayName = User.DisplayNameResolved;
			}
			Employee = employee;
		}

		public string DisplayName
		{
			get;
			private set;
		}

		public ChatUser User
		{
			get;
			private set;
		}

		public ChatUserStats Stats
		{
			get;
			private set;
		}

		public EmployeeSearchResult Employee
		{
			get;
			private set;
		}
	}
}
