using HylandMedConfig.Attached;
using HylandMedConfig.Properties;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.IO;
using System.Text;

namespace HylandMedConfig.Converters
{
	class StringToXamlContentConverter : ConverterMarkupExtension<StringToXamlContentConverter>
	{
		public StringToXamlContentConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value == DependencyProperty.UnsetValue )
			{
				return null;
			}

			string xaml = System.Convert.ToString( value );

			if( !string.IsNullOrWhiteSpace( xaml ) )
			{
				try
				{
					ParserContext context = new ParserContext();
					context.XmlnsDictionary.Add( "", "http://schemas.microsoft.com/winfx/2006/xaml/presentation" );
					context.XmlnsDictionary.Add( "x", "http://schemas.microsoft.com/winfx/2006/xaml" );
					context.XmlnsDictionary.Add( "giftoolkit", "http://wpfanimatedgif.codeplex.com" );
					context.XmlnsDictionary.Add( "hmct", "clr-namespace:HylandMedConfig.XamlControls;assembly=HylandMedConfig" );
					object content = XamlReader.Load( new MemoryStream( Encoding.UTF8.GetBytes( xaml ) ), context );

					Window window = new Window();
					window.Content = content;

					Binding b = new Binding( "MuteXaml" );
					b.Source = Settings.Default;

					FrameworkElement fe = content as FrameworkElement;
					if( fe == null )
					{
						throw new Exception( "Invalid root element" );
					}

					fe.SetBinding( WindowBehavior.MuteMediaElementProperty, b );
					return content;
				}
				catch( Exception ex )
				{
					StackPanel sp = new StackPanel();
					sp.Children.Add( new TextBlock
					{
						Text = string.Format( "Xaml Error: {0}", ex.Message ),
						TextWrapping = TextWrapping.Wrap,
						Foreground = Brushes.Red,
					} );
					return sp;
				}
			}
			return null;
		}
	}
}
