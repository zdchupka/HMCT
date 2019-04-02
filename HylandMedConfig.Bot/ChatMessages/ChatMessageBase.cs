using System;

namespace HylandMedConfig.Common
{
 //   [Serializable]
 //   public abstract class ChatMessageBase : ViewModelBase
 //   {
	//	/// <summary>
	//	/// Gets the message identifier
	//	/// </summary>
	//	public Guid ID
	//	{
	//		get;
	//		private set;
	//	}

	//	/// <summary>
	//	/// Gets the datetime the message was sent
	//	/// </summary>
	//	public DateTime Date
 //       {
 //           get;
 //           internal set;
 //       }

 //       public ChatMessageBase()
 //       {
 //           Date = DateTime.Now;
	//		ID = Guid.NewGuid();
	//	}

	//	public static bool operator ==( ChatMessageBase x, ChatMessageBase y )
	//	{
	//		if( (object)x == null && (object)y == null )
	//		{
	//			return true;
	//		}
	//		else if( (object)x == null || (object)y == null )
	//		{
	//			return false;
	//		}
	//		else
	//		{
	//			return x.Equals( y );
	//		}
	//	}

	//	public static bool operator !=( ChatMessageBase x, ChatMessageBase y )
	//	{
	//		return !( x == y );
	//	}

	//	public override bool Equals( object obj )
	//	{
	//		if( obj is ChatMessageBase )
	//		{
	//			return ID.Equals( ( obj as ChatMessageBase ).ID );
	//		}
	//		return false;
	//	}

	//	public override int GetHashCode()
	//	{
	//		return ID.GetHashCode();
	//	}
	//}
}
