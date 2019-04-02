using HylandMedConfig.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Attached;
using System.ComponentModel;
using System.Windows.Data;

namespace HylandMedConfig.ChatMessages
{
	public class ChatUserListStatsMessage : ChatUserMessage
	{
		public override string ToMessageString( bool includeTag = true )
		{
			StringBuilder sb = new StringBuilder();
			foreach( EmployeeSearchResult employee in EmployeesView )
			{
				sb.AppendLine( $"{employee.DisplayName}\t{employee.OutOfOfficeStatus}" );
			}
			return sb.ToString();
		}
		private BulkObservableCollection<EmployeeSearchResult> _employees = new BulkObservableCollection<EmployeeSearchResult>();

		public ChatUserListStatsMessage( IEnumerable<EmployeeSearchResult> employees )
		{
			_employees.AddRange( employees );
			EmployeesView = CollectionViewSource.GetDefaultView( _employees );

			EmployeesView.SortDescriptions.Add( new SortDescription( nameof( EmployeeSearchResult.DisplayName ), ListSortDirection.Ascending ) );
		}

		public ICollectionView EmployeesView
		{
			get;
			private set;
		}
	}
}
