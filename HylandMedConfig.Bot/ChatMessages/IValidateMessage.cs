using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Common
{
	public interface IValidateMessage
	{
		bool Validate( out string error );
	}
}
