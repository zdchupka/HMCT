using HylandMedConfig.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace HylandMedConfig.Windows
{
	public partial class IgnoreTagSettingsWindow : Window
	{


		private ObservableCollection<string> _ignoreTags = new ObservableCollection<string>();

		public ICollectionView IgnoreTagsView
		{
			get;
			private set;
		}

		public IgnoreTagSettingsWindow()
		{
			IgnoreTagsView = CollectionViewSource.GetDefaultView( _ignoreTags );
			IgnoreTagsView.SortDescriptions.Add( new SortDescription( "", ListSortDirection.Ascending ) );

			foreach( string tag in Settings.Default.IgnoredTags )
			{
				_ignoreTags.Add( tag );
			}
			InitializeComponent();
		}

		private void CommandBinding_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandBinding_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			Settings.Default.IgnoredTags.Clear();

			foreach( string tag in _ignoreTags )
			{
				Settings.Default.IgnoredTags.Add( tag );
			}

			Settings.Default.Save();

			DialogResult = true;
		}



		public string IgnoreFilterText
		{
			get { return (string)GetValue( IgnoreFilterTextProperty ); }
			set { SetValue( IgnoreFilterTextProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for IgnoreFilterText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IgnoreFilterTextProperty =
			DependencyProperty.Register( "IgnoreFilterText", typeof( string ), typeof( IgnoreTagSettingsWindow ), new PropertyMetadata( "" ) );



		private void root_Loaded( object sender, RoutedEventArgs e )
		{
			txtFilterText.Focus();
		}

		private void CommandBinding_CanExecute_2( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = !string.IsNullOrWhiteSpace( IgnoreFilterText );
		}

		private void CommandBinding_Executed_2( object sender, ExecutedRoutedEventArgs e )
		{
			_ignoreTags.Add( IgnoreFilterText.Trim() );
			IgnoreFilterText = string.Empty;
		}

		private void CommandBinding_CanExecute_3( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = IgnoreTagsView.CurrentItem != null;
		}

		private void CommandBinding_Executed_3( object sender, ExecutedRoutedEventArgs e )
		{
			_ignoreTags.Remove( System.Convert.ToString( IgnoreTagsView.CurrentItem ) );
		}
	}
}
