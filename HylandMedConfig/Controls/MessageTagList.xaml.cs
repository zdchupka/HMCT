using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HylandMedConfig.Controls
{
	/// <summary>
	/// Interaction logic for MessageTagList.xaml
	/// </summary>
	public partial class MessageTagList : ItemsControl
	{
		public MessageTagList()
		{
			InitializeComponent();
		}

		private void CheckBox_Checked( object sender, RoutedEventArgs e )
		{

			TagFilter filter = ( sender as CheckBox ).DataContext as TagFilter;
			ApplicationViewModel.Current.RefreshTagFilters( filter );

			
		}
	}
}
