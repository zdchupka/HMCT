using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HylandMedConfig.Controls
{
	class ThumbsDown : Button
	{
		static ThumbsDown()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ThumbsDown ), new FrameworkPropertyMetadata( typeof( ThumbsDown ) ) );
		}



		public bool IsChecked
		{
			get { return (bool)GetValue( IsCheckedProperty ); }
			set { SetValue( IsCheckedProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register( "IsChecked", typeof( bool ), typeof( ThumbsDown ), new PropertyMetadata( false ) );
	}
}
