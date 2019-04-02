using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HylandMedConfig.Controls
{
	public partial class ColorPicker : UserControl
	{
		public Color SelectedColor
		{
			get { return (Color)GetValue( SelectedColorProperty ); }
			set { SetValue( SelectedColorProperty, value ); }
		}

		public static readonly DependencyProperty SelectedColorProperty =
			DependencyProperty.Register( "SelectedColor", typeof( Color ), typeof( ColorPicker ),
			new FrameworkPropertyMetadata( SystemColors.DesktopColor, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public ColorPicker()
		{
			InitializeComponent();
		}
	}
}
