using HylandMedConfig.Common;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace HylandMedConfig.Windows
{
	public partial class EditWhiteboardWindow : Window
	{
		public WhiteboardChatMessage Message
		{
			get { return (WhiteboardChatMessage)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( WhiteboardChatMessage ), typeof( EditWhiteboardWindow ), new PropertyMetadata( null ) );

		public EditWhiteboardWindow()
		{
			InitializeComponent();
		}
	}
}
