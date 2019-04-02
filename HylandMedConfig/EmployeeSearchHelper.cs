using HylandMedConfig.Json.OnBaseEmployeeSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HylandMedConfig
{
	public static class EmployeeSearchHelper
	{
		private const string _searchAndoverUrl = @"http://pages.onbase.net/Service.svc/GetEmployeeListSearchResults?location=Andover";
		private const string _searchAndoverUrl2 = @"http://pages.onbase.net/Service.svc/GetEmployeeListSearchResults?firstname={0}&lastname={1}";
        private const string _allEmployeesUrl = @"http://pages.onbase.net/Service.svc/GetEmployeeListSearchResults";

		public static IEnumerable<EmployeeSearchResult> FindEmployee( string firstname, string lastname )
		{
			List<EmployeeSearchResult> employees = new List<EmployeeSearchResult>();

			try
			{
				string url = string.Format( _searchAndoverUrl2, firstname, lastname );
				WebClient wc = new WebClient();
				wc.UseDefaultCredentials = true;
				string rawText = wc.DownloadString( url );

				string escapedText = Regex.Unescape( rawText );
				escapedText = escapedText.Substring( 1, escapedText.Length - 2 );

				escapedText = "{\"employees\" : " + escapedText + "}";

				using( MemoryStream mStrm = new MemoryStream( Encoding.UTF8.GetBytes( escapedText ) ) )
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rootobject ) );
					Rootobject rootObject = serializer.ReadObject( mStrm ) as Rootobject;

					foreach( Employee data in rootObject.employees )
					{
						EmployeeSearchResult result = new EmployeeSearchResult
						{
							DisplayName = data.DisplayName,
							FirstName = data.FirstName,
							LastName = data.LastName,
							NetworkLogon = data.NetworkLogon,
							OutOfOfficeStatus = string.IsNullOrEmpty( data.OutOfOfficeStatus ) ? "IN OFFICE" : data.OutOfOfficeStatus,
							OutOfOfficeEndDate = string.IsNullOrEmpty( data.OutOfOfficeEndDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeEndDate ) ),
							OutOfOfficeStartDate = string.IsNullOrEmpty( data.OutOfOfficeStartDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeStartDate ) ),
						};
						if( result.OutOfOfficeStartDate.HasValue )
						{
							//if( result.OutOfOfficeStartDate.Value == result.OutOfOfficeEndDate.Value )
							{
								result.OutOfOfficeStatus += string.Format( " ({0:d} - {1:d})", data.OutOfOfficeStartDate, data.OutOfOfficeEndDate );
							}
						}

						employees.Add( result );
					}
				}
			}
			catch( Exception )
			{

			}
			return employees;
		}

        public static IEnumerable<EmployeeSearchResult> GetEmployees(string[] employees)
        {
            if (employees.Length > 0)
            {
                var allEmployees = GetAllEmployees();

                foreach (string employee in employees)
                {
                    string[] names = employee.Split(new char[] { ' ' });
                    yield return allEmployees.Where(e => e.FirstName == names[0] && e.LastName == names[1]).FirstOrDefault();
                }
            }
        }

        public static List<EmployeeSearchResult> GetAllEmployees()
        {
            List<EmployeeSearchResult> employees = new List<EmployeeSearchResult>();
			try
			{
				WebClient wc = new WebClient();
				wc.UseDefaultCredentials = true;
				string rawText = wc.DownloadString( _allEmployeesUrl );

				string escapedText = Regex.Unescape( rawText );
				escapedText = escapedText.Substring( 1, escapedText.Length - 2 );

				escapedText = "{\"employees\" : " + escapedText + "}";

				using( MemoryStream mStrm = new MemoryStream( Encoding.UTF8.GetBytes( escapedText ) ) )
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rootobject ) );
					Rootobject rootObject = serializer.ReadObject( mStrm ) as Rootobject;

					foreach( Employee data in rootObject.employees )
					{
						EmployeeSearchResult result = new EmployeeSearchResult
						{
							DisplayName = data.DisplayName,
							FirstName = data.FirstName,
							LastName = data.LastName,
							NetworkLogon = data.NetworkLogon,
							OutOfOfficeStatus = string.IsNullOrEmpty( data.OutOfOfficeStatus ) ? "IN OFFICE" : data.OutOfOfficeStatus,
							OutOfOfficeEndDate = string.IsNullOrEmpty( data.OutOfOfficeEndDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeEndDate ) ),
							OutOfOfficeStartDate = string.IsNullOrEmpty( data.OutOfOfficeStartDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeStartDate ) ),
						};

						if( result.OutOfOfficeStartDate.HasValue )
						{
							//if( result.OutOfOfficeStartDate.Value == result.OutOfOfficeEndDate.Value )
							{
								result.OutOfOfficeStatus += string.Format( " ({0:d} - {1:d})", data.OutOfOfficeStartDate, data.OutOfOfficeEndDate );
							}
						}

						employees.Add( result );
					}
				}
			}
			catch( Exception )
			{

			}

			return employees;
        }

		public static IEnumerable<EmployeeSearchResult> GetAndoverEmployees()
		{
			List<EmployeeSearchResult> employees = new List<EmployeeSearchResult>();
			try
			{
				WebClient wc = new WebClient();
				wc.UseDefaultCredentials = true;
				string rawText = wc.DownloadString( _searchAndoverUrl );

				string escapedText = Regex.Unescape( rawText );
				escapedText = escapedText.Substring( 1, escapedText.Length - 2 );

				escapedText = "{\"employees\" : " + escapedText + "}";

				using( MemoryStream mStrm = new MemoryStream( Encoding.UTF8.GetBytes( escapedText ) ) )
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rootobject ) );
					Rootobject rootObject = serializer.ReadObject( mStrm ) as Rootobject;

					foreach( Employee data in rootObject.employees )
					{
						EmployeeSearchResult result = new EmployeeSearchResult
						{
							DisplayName = data.DisplayName,
							FirstName = data.FirstName,
							LastName = data.LastName,
							NetworkLogon = data.NetworkLogon,
							OutOfOfficeStatus = string.IsNullOrEmpty( data.OutOfOfficeStatus ) ? "IN OFFICE" : data.OutOfOfficeStatus,
							OutOfOfficeEndDate = string.IsNullOrEmpty( data.OutOfOfficeEndDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeEndDate ) ),
							OutOfOfficeStartDate = string.IsNullOrEmpty( data.OutOfOfficeStartDate ) ? null : new DateTime?( DateTime.Parse( data.OutOfOfficeStartDate ) ),
						};

						if( result.OutOfOfficeStartDate.HasValue )
						{
							//if( result.OutOfOfficeStartDate.Value == result.OutOfOfficeEndDate.Value )
							{
								result.OutOfOfficeStatus += string.Format( " ({0:d} - {1:d})", data.OutOfOfficeStartDate, data.OutOfOfficeEndDate );
							}
						}

						employees.Add( result );
					}
				}
			}
			catch( Exception )
			{

			}

			return employees;
		}

		public static async Task<IEnumerable<EmployeeSearchResult>> GetAndoverEmployeesAsync()
		{
			//Task<IEnumerable<EmployeeSearchResult>> t = new Task<IEnumerable<EmployeeSearchResult>>( () =>
			// {
			//	 return GetAndoverEmployees();
			// } );
			//var result = await t;
			//return result;

			var junk = await Task<IEnumerable<EmployeeSearchResult>>.Run( () =>
			 {
				 return GetAndoverEmployees();
			 } );

			return junk;
		}
	}

	public class EmployeeSearchResult
	{
		public string DisplayName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string NetworkLogon { get; set; }
		public string OutOfOfficeStatus { get; set; }
		public DateTime? OutOfOfficeStartDate { get; set; }
		public DateTime? OutOfOfficeEndDate { get; set; }
	}
}
