using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HylandMedConfig.Controls
{
	public class OutlinedText : Shape
	{
		static OutlinedText()
		{
			FillProperty.OverrideMetadata( typeof( OutlinedText ), new FrameworkPropertyMetadata( Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender ) );
			StrokeProperty.OverrideMetadata( typeof( OutlinedText ), new FrameworkPropertyMetadata( Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender ) );
		}
		private Geometry _textGeometry;

		public string Text
		{
			get { return (string)GetValue( TextProperty ); }
			set { SetValue( TextProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register( "Text", typeof( string ), typeof( OutlinedText ), new FrameworkPropertyMetadata( "", FrameworkPropertyMetadataOptions.AffectsRender ) );



		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue( FontFamilyProperty ); }
			set { SetValue( FontFamilyProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontFamilyProperty =
			DependencyProperty.Register( "FontFamily", typeof( FontFamily ), typeof( OutlinedText ), new PropertyMetadata( new FontFamily( "Impact" ) ) );

		public double FontSize
		{
			get { return (double)GetValue( FontSizeProperty ); }
			set { SetValue( FontSizeProperty, value ); }
		}

		public static readonly DependencyProperty FontSizeProperty =
			DependencyProperty.Register( "FontSize", typeof( double ), typeof( OutlinedText ), new FrameworkPropertyMetadata( 15.0d, FrameworkPropertyMetadataOptions.AffectsRender ) );

		protected override void OnRender( DrawingContext drawingContext )
		{
			// Draw the outline based on the properties that are set.
			CreateText();
			Pen pen = new Pen( Stroke, StrokeThickness )
			{
				DashCap = PenLineCap.Round,
				EndLineCap = PenLineCap.Round,
				LineJoin = PenLineJoin.Round,
				StartLineCap = PenLineCap.Round
			};
			drawingContext.DrawGeometry( Fill, pen, _textGeometry );
		}


		public void CreateText()
		{
			System.Windows.FontStyle fontStyle = FontStyles.Normal;
			FontWeight fontWeight = FontWeights.Bold;

			//if (Bold == true) fontWeight = FontWeights.Bold;
			//if (Italic == true) fontStyle = FontStyles.Italic;

			string upperCaseText = Text.ToUpper();


			// Create the formatted text based on the properties set.
			FormattedText formattedText = new FormattedText(
				upperCaseText,
				CultureInfo.GetCultureInfo( "en-us" ),
				FlowDirection.LeftToRight,
				new Typeface(
					FontFamily,
					fontStyle,
					fontWeight,
					FontStretches.Normal ),
				FontSize,
				System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
				);

			// Build the geometry object that represents the text.
			_textGeometry = formattedText.BuildGeometry( new System.Windows.Point( 0, 0 ) );

			// Build the geometry object that represents the text hightlight.
			//if (Highlight == true)
			//{
			//    _textHighLightGeometry = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));
			//}
		}



		protected override Geometry DefiningGeometry
		{
			get
			{
				CreateText();
				return _textGeometry;
			}
		}
	}
}
