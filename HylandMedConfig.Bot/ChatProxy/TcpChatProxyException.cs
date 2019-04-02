using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Common
{

	[Serializable]
	public class TcpChatProxyException : Exception
	{
		public TcpChatProxyException() { }
		public TcpChatProxyException( string message ) : base( message ) { }
		public TcpChatProxyException( string message, Exception inner ) : base( message, inner ) { }
		protected TcpChatProxyException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
	}
}
