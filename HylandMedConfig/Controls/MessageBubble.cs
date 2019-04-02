using System.Windows;
using System.Windows.Controls;
using HylandMedConfig.Common;

namespace HylandMedConfig.Controls
{
	public class MessageBubble : ContentControl
	{
		/// <summary>
		/// Gets or sets the Message to be shown
		/// </summary>
		public ChatUserMessage Message
		{
			get { return (ChatUserMessage)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( ChatUserMessage ), typeof( MessageBubble ), new PropertyMetadata( null ) );

		public bool ShowCallout
		{
			get { return (bool)GetValue( ShowCalloutProperty ); }
			set { SetValue( ShowCalloutProperty, value ); }
		}

		public static readonly DependencyProperty ShowCalloutProperty =
			DependencyProperty.Register( "ShowCallout", typeof( bool ), typeof( MessageBubble ), new PropertyMetadata( true ) );

		static MessageBubble()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( MessageBubble ), new FrameworkPropertyMetadata( typeof( MessageBubble ) ) );
		}

	}
}
