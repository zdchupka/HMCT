using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace HylandIMServer
{
	[Serializable]
	public class UserXml
	{
		[XmlAttribute( "Username" )]
		public string Username
		{
			get;
			set;
		}

		[XmlAttribute( "DisplayName" )]
		public string DisplayName
		{
			get;
			set;
		}

		[XmlAttribute( "Nickname" )]
		public string Nickname
		{
			get;
			set;
		}

		[XmlAttribute( "ImageUrl" )]
		public string ImageUrl
		{
			get;
			set;
		}
	}

	[Serializable]
	[XmlRoot( "UserConfiguration" )]
	public class UserConfiguration
	{
		[XmlArrayItem( "User" )]
		public ObservableCollection<UserXml> Users
		{
			get;
			set;
		}

		public UserConfiguration()
		{
			Users = new ObservableCollection<UserXml>();
		}
	}
}
