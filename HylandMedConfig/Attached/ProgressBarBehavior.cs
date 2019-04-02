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
	public static class ProgressBarBehavior
	{


		public static double GetAnimatedPercentage( DependencyObject obj )
		{
			return (double)obj.GetValue( AnimatedPercentageProperty );
		}

		public static void SetAnimatedPercentage( DependencyObject obj, double value )
		{
			obj.SetValue( AnimatedPercentageProperty, value );
		}

		// Using a DependencyProperty as the backing store for AnimatedPercentage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AnimatedPercentageProperty =
			DependencyProperty.RegisterAttached( "AnimatedPercentage", typeof( double ), typeof( ProgressBarBehavior ), new FrameworkPropertyMetadata( ( sender, args ) =>
			{
				ProgressBar progressBar = sender as ProgressBar;

				double newValue = Convert.ToDouble( args.NewValue );

				if( newValue >= 0d )
				{
					Duration duration = Duration.Automatic;
					if( !Settings.Default.EnableAnimations )
					{
						duration = new Duration( TimeSpan.FromSeconds( 0 ) );
					}
					DoubleAnimation animation = new DoubleAnimation( newValue, duration );
					animation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
					progressBar.BeginAnimation( ProgressBar.ValueProperty, animation );
				}
			} ) );

	}
}
