using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Globalization;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
	public class WhiteboardEntryToLineConverter : ConverterMarkupExtension<WhiteboardEntryToLineConverter>
	{
		public WhiteboardEntryToLineConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value == DependencyProperty.UnsetValue )
			{
				return null;
			}
			WhiteboardEntry entry = value as WhiteboardEntry;

			PointCollection points = entry.Points;
			if(points.Count == 1)
			{
				points.Add( points[0] );
			}
			if( entry != null )
			{
				Polyline line = new Polyline();
				line.StrokeThickness = entry.StrokeThickness;
				line.Stroke = entry.Stroke;
				line.StrokeStartLineCap = PenLineCap.Round;
				line.StrokeEndLineCap = PenLineCap.Round;
				line.StrokeLineJoin = PenLineJoin.Round;
				line.Points = entry.Points;
				return line;
			}
			return null;
		}
	}
}
