using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HylandMedConfig.Common
{
	public class EmoticonLibraryItem
	{
		[TypeConverter( typeof( ArrayTypeConverter ) )]
		public string[] Keys
		{
			get; set;
		}

		public ImageSource ImageSource
		{
			get; set;
		}

		public EmoticonLibraryItem()
		{
			IsCustom = false;
		}

		public bool IsCustom
		{
			get;
			internal set;
		}
	}

	public class EmoticonLibrary : List<EmoticonLibraryItem>
	{
		public static ComponentResourceKey EmoticonsKey = new ComponentResourceKey( typeof( EmoticonLibrary ), "EmoticonsKey" );

		public static EmoticonLibrary _current = null;

		static EmoticonLibrary()
		{
			if( Application.Current != null )
			{
				_current = Application.Current.Resources[EmoticonsKey] as EmoticonLibrary;
			}
			else
			{
				_current = new EmoticonLibrary();
			}
		}

		public static EmoticonLibrary Current
		{
			get
			{
				return _current;
			}
		}
	}

	public class ArrayTypeConverter : TypeConverter
	{
		public override object ConvertFrom(
			ITypeDescriptorContext context, CultureInfo culture, object value )
		{
			string list = value as string;
			if( list != null )
				return list.Split( ',' );

			return base.ConvertFrom( context, culture, value );
		}

		public override bool CanConvertFrom(
			ITypeDescriptorContext context, Type sourceType )
		{
			if( sourceType == typeof( string ) )
				return true;

			return base.CanConvertFrom( context, sourceType );
		}
	}
}
