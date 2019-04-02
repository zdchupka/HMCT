using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Common
{
	[Serializable]
	public class ChatUserLoginException : Exception
	{
		public ChatUserLoginException() { }
		public ChatUserLoginException( string message ) : base( message ) { }
		public ChatUserLoginException( string message, Exception inner ) : base( message, inner ) { }
		protected ChatUserLoginException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}
}
