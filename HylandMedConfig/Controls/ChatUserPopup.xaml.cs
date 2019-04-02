using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using HylandMedConfig.Common;

namespace HylandMedConfig.Controls
{
	/// <summary>
	/// Interaction logic for ChatUserPopup2xaml.xaml
	/// </summary>
	public partial class ChatUserPopup : Popup
	{
		public string OutOfOfficeStatus
		{
			get { return (string)GetValue( OutOfOfficeStatusProperty ); }
			set { SetValue( OutOfOfficeStatusProperty, value ); }
		}

		public static readonly DependencyProperty OutOfOfficeStatusProperty =
			DependencyProperty.Register( "OutOfOfficeStatus", typeof( string ), typeof( ChatUserPopup ), new PropertyMetadata( "" ) );

		public ChatUser User
		{
			get { return (ChatUser)GetValue( UserProperty ); }
			set { SetValue( UserProperty, value ); }
		}

		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register( "User", typeof( ChatUser ), typeof( ChatUserPopup ), new PropertyMetadata( null ) );

		private DispatcherTimer _timer;
		private DispatcherTimer _timerLeave;

		public ChatUserPopup()
		{
			InitializeComponent();

			Opened += ChatUserPopup2_Opened;

			Loaded += ChatUserPopup2_Loaded;

			_timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromMilliseconds( 400 );
			_timer.Tick += _timer_Tick;

			_timerLeave = new DispatcherTimer();
			_timerLeave.Interval = _timer.Interval;
			_timerLeave.Tick += _timerLeave_Tick;
		}

		private void _timerLeave_Tick( object sender, EventArgs e )
		{
			IsMouseOverTarget = PlacementTarget.IsMouseOver;
			_timerLeave.Stop();
		}

		public bool IsMouseOverTarget
		{
			get { return (bool)GetValue( IsMouseOverTargetProperty ); }
			set { SetValue( IsMouseOverTargetProperty, value ); }
		}

		public static readonly DependencyProperty IsMouseOverTargetProperty =
			DependencyProperty.Register( "IsMouseOverTarget", typeof( bool ), typeof( ChatUserPopup ), new PropertyMetadata( false ) );

		private void _timer_Tick( object sender, EventArgs e )
		{
			IsMouseOverTarget = PlacementTarget.IsMouseOver;
			_timer.Stop();
		}

		private void ChatUserPopup2_Loaded( object sender, RoutedEventArgs e )
		{
			PlacementTarget?.AddHandler( MouseEnterEvent, new MouseEventHandler( MouseEntered ), true );
			PlacementTarget?.AddHandler( MouseLeaveEvent, new MouseEventHandler( MouseLeave2 ), true );
		}


		private void MouseEntered( object sender, MouseEventArgs e )
		{
			_timer.Start();
		}

		private void MouseLeave2( object sender, MouseEventArgs e )
		{
			_timerLeave.Start();
		}

		private async void ChatUserPopup2_Opened( object sender, EventArgs e )
		{
			OutOfOfficeStatus = string.Empty;
			if( User != null && !User.IsBot )
			{
				try
				{
					PART_OOOStatus.Visibility = Visibility.Collapsed;
					PART_OOORefresh.Visibility = Visibility.Visible;

					var employees = await EmployeeSearchHelper.GetAndoverEmployeesAsync();

					var employee = employees.FirstOrDefault( emp => emp.NetworkLogon == User.Username );
					if( employee != null )
					{
						OutOfOfficeStatus = employee.OutOfOfficeStatus;
					}

				}
				finally
				{
					PART_OOOStatus.Visibility = Visibility.Visible;
					PART_OOORefresh.Visibility = Visibility.Collapsed;
				}
			}
		}
	}
}
