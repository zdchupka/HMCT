using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HylandMedConfig.Common;

namespace HylandMedConfig.Controls
{
	public partial class ChatUserImage : UserControl
	{
		public ChatUser User
		{
			get { return (ChatUser)GetValue( UserProperty ); }
			set { SetValue( UserProperty, value ); }
		}

		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register( "User", typeof( ChatUser ), typeof( ChatUserImage ), new PropertyMetadata( null ) );

		public ChatUserImage()
		{
			InitializeComponent();

			Loaded += ChatUserImage_Loaded;

			ToolTipService.AddToolTipOpeningHandler( this, new ToolTipEventHandler( ( sender, args ) =>
			  {
				  
			  } ) );
		}

		private void ChatUserImage_Loaded( object sender, RoutedEventArgs e )
		{
			// Make sure this has a Window, we do not want to fall into a recursion problem if a user's mood 
			// has a username in it.
			if( this.FindParent<Window>() != null )
			{
				userTooltipPopup.Content = new ChatUserPopup { User = User, PlacementTarget = userEllipse };
			}
		}
	}

	public class ChatUserImageImageCenterConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value == DependencyProperty.UnsetValue )
			{
				return new Point();
			}

			double height = System.Convert.ToDouble( value );
			return new Point( height / 2d, height / 2d );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}
}
