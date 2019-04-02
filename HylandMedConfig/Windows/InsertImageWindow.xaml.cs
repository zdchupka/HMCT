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
	public partial class InsertImageWindow : Window
	{
		private static int _LastResultCount = 5;
		private static string _LastRating = "g";
		public List<int> NumResultsList
		{
			get;
			private set;
		}

		public int NumResults
		{
			get;
			set;
		}

		public string Rating
		{
			get;
			set;
		}

		public bool Accepted
		{
			get;
			private set;
		}

		private ObservableCollection<GiphyResult> _results = new ObservableCollection<GiphyResult>();

		public ICollectionView ResultsView
		{
			get;
			private set;
		}

		public GiphyResult SelectedResult
		{
			get
			{
				return ResultsView.CurrentItem as GiphyResult;
			}
		}

		public List<string> RatingList
		{
			get;
			private set;
		}

		public InsertImageWindow()
		{
			ResultsView = CollectionViewSource.GetDefaultView( _results );
			NumResultsList = new List<int>
			{
				1,
				3,
				5,
				10,
			};
			RatingList = new List<string>
			{
				"y", 
				"g",
				"pg",
				"pg-13",
				"r",
			};
			NumResults = _LastResultCount;
			Rating = _LastRating;

			InitializeComponent();
		}

		private void Button_Click_1( object sender, RoutedEventArgs e )
		{
			Accepted = true;
			this.Close();
		}

		private void CommandBinding_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = e.Parameter != null && !string.IsNullOrWhiteSpace( e.Parameter.ToString() );
		}

		private void CommandBinding_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			string searchText = System.Convert.ToString( e.Parameter );
			_LastResultCount = NumResults;
			_LastRating = Rating;

			_results.Clear();

			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				foreach( GiphyResult result in GiphyHelper.Search( searchText, NumResults, Rating ) )
				{
					_results.Add( result );
				}
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private void CommandBinding_Executed_1( object sender, ExecutedRoutedEventArgs e )
		{

			Accepted = true;
			this.Close();
		}

		private void CommandBinding_CanExecute_1( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = SelectedResult != null;
		}

		private void root_Loaded( object sender, RoutedEventArgs e )
		{
			txtSearchText.Focus();
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			this.Close();
		}
	}
}
