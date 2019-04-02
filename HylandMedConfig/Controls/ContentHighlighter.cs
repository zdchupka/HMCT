using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HylandMedConfig.Controls
{
	public class ContentHighlighter : ContentControl
	{
		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue( CornerRadiusProperty ); }
			set { SetValue( CornerRadiusProperty, value ); }
		}

		public static readonly DependencyProperty CornerRadiusProperty =
			DependencyProperty.Register( "CornerRadius", typeof( CornerRadius ), typeof( ContentHighlighter ), new PropertyMetadata( new CornerRadius() ) );

		public Brush HighlightBrush
		{
			get { return (Brush)GetValue( HighlightBrushProperty ); }
			set { SetValue( HighlightBrushProperty, value ); }
		}

		public static readonly DependencyProperty HighlightBrushProperty =
			DependencyProperty.Register( "HighlightBrush", typeof( Brush ), typeof( ContentHighlighter ), new PropertyMetadata( Brushes.Yellow ) );

		public bool ShowHighlighting
		{
			get { return (bool)GetValue( ShowHighlightingProperty ); }
			set { SetValue( ShowHighlightingProperty, value ); }
		}

		public static readonly DependencyProperty ShowHighlightingProperty =
			DependencyProperty.Register( "ShowHighlighting", typeof( bool ), typeof( ContentHighlighter ), new PropertyMetadata( true ) );

		static ContentHighlighter()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ContentHighlighter ), new FrameworkPropertyMetadata( typeof( ContentHighlighter ) ) );
		}
	}
}
