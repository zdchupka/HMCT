using HylandMedConfig.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace HylandMedConfig.Attached
{
	public class RadioButtonBehavior : Behavior<RadioButton>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
		}

		private void AssociatedObject_PreviewMouseLeftButtonDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			e.Handled = IsReadOnly;
		}



		public bool IsReadOnly
		{
			get { return (bool)GetValue( IsReadOnlyProperty ); }
			set { SetValue( IsReadOnlyProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsReadOnlyProperty =
			DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( RadioButtonBehavior ), new PropertyMetadata( false ) );


	}
}
