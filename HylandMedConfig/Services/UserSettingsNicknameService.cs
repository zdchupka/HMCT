using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Services
{
	public class UserSettingsNicknameService : INicknameService
	{
		private bool _enabled = true;

		public UserSettingsNicknameService()
		{
			if( !_enabled ) return;

			if( Settings.Default.NicknameDictionary == null )
			{
				Settings.Default.NicknameDictionary = new SerializableStringDictionary();
				Settings.Default.Save();
			}
		}

		public string GetNickname( ChatUser user )
		{
			if( !_enabled )
			{
				return user.DisplayName;
			}

			if( Settings.Default.NicknameDictionary.ContainsKey( user.Username ) )
			{
				return Settings.Default.NicknameDictionary[user.Username];
			}
			return null;
		}

		public void SaveNickname( ChatUser user, string nickname )
		{
			if( !_enabled ) return;

			if( !Settings.Default.NicknameDictionary.ContainsKey( user.Username ) )
			{
				Settings.Default.NicknameDictionary.Add( user.Username, nickname );
			}
			else
			{
				Settings.Default.NicknameDictionary[user.Username] = nickname;
			}
			Settings.Default.Save();
		}
	}
}
