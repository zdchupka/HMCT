using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HylandMedConfig.Controls
{
	public class LockButton : Button
	{
		static LockButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( LockButton ), new FrameworkPropertyMetadata( typeof( LockButton ) ) );
		}



		public bool IsLocked
		{
			get { return (bool)GetValue( IsLockedProperty ); }
			set { SetValue( IsLockedProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsLockedProperty =
			DependencyProperty.Register( "IsLocked", typeof( bool ), typeof( LockButton ), new PropertyMetadata( false ) );


	}
}
