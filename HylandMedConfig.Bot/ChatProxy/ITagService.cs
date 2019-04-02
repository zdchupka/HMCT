using System.Collections.Generic;

namespace HylandMedConfig.Common
{
	public interface ITagService
	{
		bool ContainsHiddenTag( List<string> tags );
		List<string> GetHiddenTags( List<string> tags );
	}
}
