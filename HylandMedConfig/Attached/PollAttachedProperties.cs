using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Attached
{
	public static class PollAttachedProperties
	{


		public static bool GetIsLocked( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsLockedProperty );
		}

		public static void SetIsLocked( DependencyObject obj, bool value )
		{
			obj.SetValue( IsLockedProperty, value );
		}

		// Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsLockedProperty =
			DependencyProperty.RegisterAttached( "IsLocked", typeof( bool ), typeof( PollAttachedProperties ), new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );


	}
}
