using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HylandMedConfig.Common
{
	public static class HyperlinkUtility
	{
		public const string UrlRegex = @"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)~?";
		public const string UrlRegex2 = @"\bhttp\S*";
		public const string UncRegex = @"\B\\\\\S*";

		public static bool IsHyperlink( string text )
		{
			foreach( Match m in Regex.Matches( text, UrlRegex ) )
			{
				if( m.Length == text.Length )
				{
					return true;
				}
			}

			foreach( Match m in Regex.Matches( text, UrlRegex2 ) )
			{
				if( m.Length == text.Length )
				{
					return true;
				}
			}

			foreach( Match m in Regex.Matches( text, UncRegex ) )
			{
				if( m.Length == text.Length )
				{
					return true;
				}
			}
			return false;
		}
	}
}
