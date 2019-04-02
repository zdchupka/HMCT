using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using HylandMedConfig.Common.Properties;

namespace HylandMedConfig
{
	/// <summary>
	/// Represents an ObservableCollection(T) that includes the AddRange() operation.
	/// </summary>
	public class BulkObservableCollection<T> : ObservableCollection<T>
	{
		#region Declarations

		private bool _suppressCollectionChanged;

		/// <summary>
		/// 
		/// </summary>
		public BulkObservableCollection()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enumerable"></param>
		public BulkObservableCollection( IEnumerable<T> enumerable )
			: base( enumerable )
		{
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if( !_suppressCollectionChanged )
			{
				base.OnCollectionChanged( e );
			}
		}
		#endregion

		#region Methods

		/// <summary>
		/// Adds a list of items to the collection without firing an event for each item.
		/// </summary>
		/// <param name="items">A list of items to add.</param>
		public void AddRange( IEnumerable<T> items )
		{
			try
			{
				// Suppress event while adding items
				BeginBulkOperation();
				foreach( T item in items )
				{
					Add( item );
				}
			}
			finally
			{
				EndBulkOperation();
			}
		}

		/// <summary>
		/// Removes a list of items from the collection without firing an event for each item.
		/// </summary>
		/// <param name="items">A list of items to remove</param>
		public void RemoveRange( IEnumerable<T> items )
		{
			try
			{
				// Suppress event while removing items
				BeginBulkOperation();
				foreach( T item in items )
				{
					Remove( item );
				}
			}
			finally
			{
				EndBulkOperation();
			}
		}

		/// <summary>
		/// Suspends change events on the collection in order to perform a bulk change operation.
		/// </summary>
		public void BeginBulkOperation()
		{
			_suppressCollectionChanged = true;
		}

		/// <summary>
		/// Restores change events on the collection after a bulk change operation has been completed.
		/// </summary>
		public void EndBulkOperation()
		{
			_suppressCollectionChanged = false;
			OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		#endregion
	}

	[MarkupExtensionReturnType( typeof( object ) )]
	public class NicknameBinding : MarkupExtension
	{
		public NicknameBinding()
		{

		}

		public NicknameBinding(PropertyPath path)
		{
			Path = path;
		}

		[ConstructorArgument("path")]
		public PropertyPath Path { get; set; }

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			Binding b = new Binding();
			b.Path = Path;
			b.Source = Settings.Default;
			return b;
		}
	}
}


