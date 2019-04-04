using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Services
{
	public class UserSettingsTagService : ITagService
	{
		public UserSettingsTagService()
		{
			if( Settings.Default.HiddenTags == null )
			{
				Settings.Default.HiddenTags = new System.Collections.Specialized.StringCollection();
				Settings.Default.Save();
			}
		}

		public bool ContainsHiddenTag( List<string> tags )
		{
			foreach( string tag in Settings.Default.HiddenTags )
			{
				if( tags.Contains( tag ) )
				{
					return true;
				}
			}
			return false;
		}

		public List<string> GetHiddenTags( List<string> tags )
		{
			List<string> hiddenTags = new List<string>();
			foreach( string tag in Settings.Default.HiddenTags )
			{
				if( tags.Contains( tag ) )
				{
					hiddenTags.Add( tag );
				}
			}
			return hiddenTags;
		}
	}
}
