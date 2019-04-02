namespace HylandMedConfig.Common
{
	public interface INicknameService
	{
		string GetNickname( ChatUser user );
		void SaveNickname( ChatUser user, string nickname );
	}
}
