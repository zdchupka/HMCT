using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Converters;
using HylandMedConfig.Common;

namespace HylandMedConfig
{
	[Serializable]
	public class ViewModelBase : INotifyPropertyChanged
	{
		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
