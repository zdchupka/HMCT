using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HylandMedConfig.Common;

namespace HylandMedConfig.Controls
{
	public partial class ChatUserToolTip : ToolTip
	{
		#region Declarations
		private TextBlock _PART_OOOStatus;
		private UIElement _PART_OOORefresh;
		#endregion

		#region Contruction

		static ChatUserToolTip()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ChatUserToolTip ), new FrameworkPropertyMetadata( typeof( ChatUserToolTip ) ) );
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the user for the tooltip
		/// </summary>
		public ChatUser User
		{
			get { return (ChatUser)GetValue( UserProperty ); }
			set { SetValue( UserProperty, value ); }
		}

		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register( "User", typeof( ChatUser ), typeof( ChatUserToolTip ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets the out of office status of the user
		/// </summary>
		public string OutOfOfficeStatus
		{
			get { return (string)GetValue( OutOfOfficeStatusProperty ); }
			set { SetValue( OutOfOfficeStatusProperty, value ); }
		}

		public static readonly DependencyProperty OutOfOfficeStatusProperty =
			DependencyProperty.Register( "OutOfOfficeStatus", typeof( string ), typeof( ChatUserToolTip ), new PropertyMetadata( "" ) );

		#endregion

		#region Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_PART_OOOStatus = this.EnsureTemplateChild<TextBlock>( "PART_OOOStatus" );
			_PART_OOORefresh = this.EnsureTemplateChild<UIElement>( "PART_OOORefresh" );

			Opened += ChatUserToolTip_Opened;
		}

		private async void ChatUserToolTip_Opened( object sender, RoutedEventArgs e )
		{
			OutOfOfficeStatus = string.Empty;
			if( User != null && !User.IsBot )
			{
				try
				{
					_PART_OOOStatus.Visibility = Visibility.Collapsed;
					_PART_OOORefresh.Visibility = Visibility.Visible;

					var employees = await EmployeeSearchHelper.GetAndoverEmployeesAsync();

					var employee = employees.FirstOrDefault( emp => emp.NetworkLogon == User.Username );
					if( employee != null )
					{
						OutOfOfficeStatus = employee.OutOfOfficeStatus;
					}

				}
				finally
				{
					_PART_OOOStatus.Visibility = Visibility.Visible;
					_PART_OOORefresh.Visibility = Visibility.Collapsed;
				}
			}
		}

		#endregion
	}
}
