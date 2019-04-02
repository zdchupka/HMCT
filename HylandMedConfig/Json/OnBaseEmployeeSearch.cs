using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig.Json.OnBaseEmployeeSearch
{
	public class Rootobject
	{
		public Employee[] employees { get; set; }
	}

	public class Employee
	{
		public string DisplayName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string StartDate { get; set; }
		public string NetworkLogon { get; set; }
		public string JobTitle { get; set; }
		public string DeptName { get; set; }
		public string Location { get; set; }
		public string OfficePhone { get; set; }
		public string CellPhone { get; set; }
		public string Email { get; set; }
		public string OutOfOfficeStatus { get; set; }
		public string OutOfOfficeStartDate { get; set; }
		public string OutOfOfficeEndDate { get; set; }
	}
}
