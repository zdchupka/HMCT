using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using HylandMedConfig.Attached;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Windows
{
	public partial class NotificationWindow : Window
	{
		public static long _OpenNotificationCount = 0;

		/// <summary>
		/// Gets or sets the message to show
		/// </summary>
		public string Message
		{
			get { return (string)GetValue( MessageProperty ); }
			set { SetValue( MessageProperty, value ); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( string ), typeof( NotificationWindow ), new PropertyMetadata( "" ) );



		public ChatUserMessage UserMessage
		{
			get { return (ChatUserMessage)GetValue( UserMessageProperty ); }
			set { SetValue( UserMessageProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for UserMessage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty UserMessageProperty =
			DependencyProperty.Register( "UserMessage", typeof( ChatUserMessage ), typeof( NotificationWindow ), new PropertyMetadata( null ) );



		/// <summary>
		/// Gets or sets the User
		/// </summary>
		public ChatUser User
		{
			get { return (ChatUser)GetValue( UserProperty ); }
			set { SetValue( UserProperty, value ); }
		}

		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register( "User", typeof( ChatUser ), typeof( NotificationWindow ), new PropertyMetadata( null ) );

		private NotificationWindow()
		{
			InitializeComponent();
		}

		private void NotificationWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			_OpenNotificationCount--;
		}

		/// <summary>
		/// Show new notification window
		/// </summary>
		public static void ShowNotification( string message )
		{
			new NotificationWindow() { Message = message }.Show();
		}

		/// <summary>
		/// Show new user notification window
		/// </summary>
		public static void ShowUserNotification( ChatUser user, string message )
		{
			new NotificationWindow() { User = user, Message = message }.Show();
		}

		/// <summary>
		/// Show new user notification window
		/// </summary>
		public static void ShowUserNotification( ChatUserMessage message )
		{
			new NotificationWindow()
			{
				UserMessage = message,
				User = message.FromUser,
				Message = string.Format( HylandMedConfig.Properties.Resources.STR_USER_SENT_MESSAGE, message.FromUser.DisplayNameResolved )
			}.Show();
		}

		private void Window_MouseDown( object sender, MouseButtonEventArgs e )
		{
			// The user has clicked on the notification, bring the main window back / restore it if minimized
			Window window = Application.Current.MainWindow;
			if( window.WindowState == WindowState.Minimized )
			{
				window.WindowState = WindowState.Normal;
			}
			window.Activate();
			window.Focus();
			Close();
		}

		private void fakeBorder_Loaded( object sender, RoutedEventArgs e )
		{
			Dispatcher.BeginInvoke( new Action( () =>
			  {
				  double bottomMargin = _OpenNotificationCount * 50d;
				  _OpenNotificationCount++;
				// Position the window
				var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
				  var transform = PresentationSource.FromVisual( Application.Current.MainWindow ).CompositionTarget.TransformFromDevice;
				  var corner = transform.Transform( new Point( workingArea.Right, workingArea.Bottom ) );
				  this.Left = corner.X - this.ActualWidth - 10;
				  this.Top = corner.Y - this.ActualHeight - 10 - bottomMargin;


				  if( Settings.Default.EnableAnimations )
				  {
					// Animate the message portion
					Storyboard sb = new Storyboard();
					  DoubleAnimation expandAnimation = new DoubleAnimation( fakeBorder.ActualWidth, new Duration( TimeSpan.FromMilliseconds( 250 ) ) ) { EasingFunction = new QuadraticEase() };
					  DoubleAnimation collapseAnimation = new DoubleAnimation( 20d, new Duration( TimeSpan.FromMilliseconds( 250 ) ) ) { BeginTime = TimeSpan.FromSeconds( 4 ), EasingFunction = new QuadraticEase() };
					  sb.Children.Add( expandAnimation );
					  sb.Children.Add( collapseAnimation );
					  Storyboard.SetTarget( sb, textBorder );
					  Storyboard.SetTargetProperty( sb, new PropertyPath( "Width" ) );
					  sb.Completed += ( s, args ) =>
					  {
						  DoubleAnimation fadeOut = new DoubleAnimation( 0, new Duration( TimeSpan.FromMilliseconds( 300 ) ) );
						  fadeOut.Completed += ( s2, args2 ) => Close();
						  this.BeginAnimation( OpacityProperty, fadeOut );
					  };

					  sb.Begin();
				  }
				  else
				  {
					  textBorder.Width = fakeBorder.ActualWidth;

					  var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 4 ) };
					  timer.Start();
					  timer.Tick += ( s, args ) =>
					  {
						  timer.Stop();
						  Close();
					  };
				  }
			  } ) );
		}
	}
}
