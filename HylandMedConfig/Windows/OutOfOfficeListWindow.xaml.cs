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
    public partial class OutOfOfficeListWindow : Window
    {


        private ObservableCollection<string> _userList = new ObservableCollection<string>();

        public ICollectionView UsersView
        {
            get;
            private set;
        }

        public OutOfOfficeListWindow()
        {
            UsersView = CollectionViewSource.GetDefaultView(_userList);
            UsersView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));

            foreach (string tag in Settings.Default.CustomUserList)
            {
                _userList.Add(tag);
            }
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.Default.CustomUserList.Clear();

            foreach (string tag in _userList)
            {
                Settings.Default.CustomUserList.Add(tag);
            }

            Settings.Default.Save();

            DialogResult = true;
        }





        public string NewUserText
        {
            get { return (string)GetValue(NewUserTextProperty); }
            set { SetValue(NewUserTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewUserText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewUserTextProperty =
            DependencyProperty.Register("NewUserText", typeof(string), typeof(OutOfOfficeListWindow), new PropertyMetadata(""));





        private void root_Loaded(object sender, RoutedEventArgs e)
        {
            txtFilterText.Focus();
        }

        private void CommandBinding_CanExecute_2(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrWhiteSpace(NewUserText) && NewUserText.Split(new char[] { ' ' }).Length == 2;
        }

        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {
            string[] names = NewUserText.Split(new char[] { ' ' });
            IEnumerable<EmployeeSearchResult> result;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                result = EmployeeSearchHelper.FindEmployee(names[0], names[1]);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            if (result != null && result.Count() > 0)
            {
                string username = $"{result.First().FirstName} {result.First().LastName}";
                if (_userList.Contains(username))
                {
                    HylandMedConfig.Windows.MedConfigMessageBox.ShowInfo("Employee is already in lisst");
                }
                else
                {
                    _userList.Add($"{result.First().FirstName} {result.First().LastName}");
                    NewUserText = string.Empty;
                }
            }
            else
            {
                HylandMedConfig.Windows.MedConfigMessageBox.ShowError("Could not find employee");
            }

        }

        private void CommandBinding_CanExecute_3(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UsersView.CurrentItem != null;
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            _userList.Remove(System.Convert.ToString(UsersView.CurrentItem));
        }
    }
}
