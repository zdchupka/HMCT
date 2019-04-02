using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Common
{
	[Serializable]
	internal class LoginArgs
	{
		public string Username { get; set; }
		public string DisplayName { get; set; }
		public string ImageUrl { get; set; }
		public Version Version { get; set; }
		public string Nickname { get; set; }
		public bool IsBot { get; set; }
		public string BotCreatorUserName { get; set; }

		public string PreviousMood { get; set; }
	}
}
