using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HylandMedConfig.Controls
{
	public class TagListButton : Button
	{
		static TagListButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( TagListButton ), new FrameworkPropertyMetadata( typeof( TagListButton ) ) );
		}
	}
}
