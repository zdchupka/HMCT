using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;
using System.Windows.Interactivity;
using HylandMedConfig.Controls;

namespace HylandMedConfig.Attached
{
    public static class ScrollViewerAttached
    {


        


    }
    public class FrameworkElementWhisperBehavior : Behavior<FrameworkElement>
    {
        public ChatUser User
        {
            get { return (ChatUser)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for User.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(ChatUser), typeof(FrameworkElementWhisperBehavior), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
            AssociatedObject.Cursor = Cursors.Hand;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
            AssociatedObject.Cursor = Cursors.Arrow;
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ApplicationViewModel.Current.AddWhisperUserCommand.Execute(User);
            }
            else
            {
                ApplicationViewModel.Current.StartWhisperToUsersCommand.Execute(new List<ChatUser> { User });
            }
        }
    }

    public class FrameworkElementWhisperUsers : Behavior<FrameworkElement>
    {


        public List<ChatUser> Users
        {
            get { return (List<ChatUser>)GetValue(UsersProperty); }
            set { SetValue(UsersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Users.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsersProperty =
            DependencyProperty.Register("Users", typeof(List<ChatUser>), typeof(FrameworkElementWhisperUsers), new PropertyMetadata(null));




        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
            AssociatedObject.Cursor = Cursors.Hand;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
            AssociatedObject.Cursor = Cursors.Arrow;
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (Users != null)
            {
                element.MouseLeftButtonUp += (s, a) =>
                {
                    ApplicationViewModel.Current.StartWhisperToUsersCommand.Execute(Users);
                };
                element.Cursor = Cursors.Hand;
            }
        }
    }

	public class FrameworkElementSendTaggedMessage : Behavior<FrameworkElement>
	{
		
		public string Tag
		{
			get { return (string)GetValue( TagProperty ); }
			set { SetValue( TagProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Tag.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TagProperty =
			DependencyProperty.Register( "Tag", typeof( string ), typeof( FrameworkElementSendTaggedMessage ), new PropertyMetadata( "" ) );



		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
			AssociatedObject.Cursor = Cursors.Hand;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
			AssociatedObject.Cursor = Cursors.Arrow;
		}

		private void MouseLeftButtonDown( object sender, MouseEventArgs e )
		{
			FrameworkElement element = sender as FrameworkElement;

			ApplicationViewModel.Current.StartTaggedMessageCommand.Execute( Tag );
		}
	}

	public class EmoticonRichTextBoxSendTaggedMessage : Behavior<EmoticonRichTextBox>
	{

		public string Tag
		{
			get { return (string)GetValue( TagProperty ); }
			set { SetValue( TagProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for Tag.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TagProperty =
			DependencyProperty.Register( "Tag", typeof( string ), typeof( EmoticonRichTextBoxSendTaggedMessage ), new PropertyMetadata( "" ) );



		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
			AssociatedObject.Cursor = Cursors.Hand;
		}

		private void AssociatedObject_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			FrameworkElement element = sender as FrameworkElement;

			ApplicationViewModel.Current.StartTaggedMessageCommand.Execute( Tag );
			e.Handled = true;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
			AssociatedObject.Cursor = Cursors.Arrow;
		}

	}

	public class FrameworkElementBehavior
    {
        public static UITheme GetTheme(DependencyObject obj)
        {
            return (UITheme)obj.GetValue(ThemeProperty);
        }

        public static void SetTheme(DependencyObject obj, UITheme value)
        {
            obj.SetValue(ThemeProperty, value);
        }

        public static event EventHandler ThemeChanged;

        public static void OnThemeChanged(object sender)
        {
            if (ThemeChanged != null)
            {
                ThemeChanged(sender, EventArgs.Empty);
            }
        }

        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached("Theme", typeof(UITheme), typeof(FrameworkElementBehavior), new FrameworkPropertyMetadata(UITheme.None, (sender, args) =>
            {
                FrameworkElement element = sender as FrameworkElement;
                UITheme theme = (UITheme)args.NewValue;
                Settings.Default.ThemeNum = (int)theme;

                List<Uri> dictionaryUris = new List<Uri>();

                switch (theme)
                {
                    case UITheme.Metro:
                        dictionaryUris.AddRange(new List<Uri>
                            { 
                                new Uri("ResourceDictionaries/Themes/Metro/Metro.MSControls.Core.Implicit.xaml", UriKind.Relative),
                                new Uri("ResourceDictionaries/Themes/Metro/Metro.HylandMedConfigControls.Implicit.xaml", UriKind.Relative),
                            });
                        break;
                    case UITheme.MetroDark:
                        dictionaryUris.AddRange(new List<Uri>
                            { 
                                new Uri("ResourceDictionaries/Themes/MetroDark/MetroDark.MSControls.Core.Implicit.xaml", UriKind.Relative),
                                new Uri("ResourceDictionaries/Themes/MetroDark/MetroDark.HylandMedConfigControls.Implicit.xaml", UriKind.Relative),
                            });
                        break;
                    case UITheme.IG:
                        dictionaryUris.AddRange(new List<Uri>
                            { 
                                new Uri("ResourceDictionaries/Themes/IG/IG.MSControls.Core.Implicit.xaml", UriKind.Relative),
                                new Uri("ResourceDictionaries/Themes/IG/IG.HylandMedConfigControls.Implicit.xaml", UriKind.Relative),
                            });
                        break;
                }

                double verticalOffset = 0d;
                if (element is MainWindow)
                {
                    // Bug when changing themes with listbox, it causes it to rearrange and scrolls to the top
                    verticalOffset = (element as MainWindow).GetVerticalScrollPosition();
                }

                try
                {
                    List<ThemeResourceDictionary> themeDictionaries = new List<ThemeResourceDictionary>();
                    if (dictionaryUris != null)
                    {
                        foreach (Uri uri in dictionaryUris)
                        {
                            ThemeResourceDictionary themeDictionary = new ThemeResourceDictionary();
                            themeDictionary.Source = uri;

                            // add the new dictionary to the collection of merged dictionaries of the target object
                            element.Resources.MergedDictionaries.Insert(0, themeDictionary);

                            themeDictionaries.Add(themeDictionary);
                        }
                    }

                    // find if the target element already has a theme applied
                    List<ThemeResourceDictionary> existingDictionaries =
                        (from dictionary in element.Resources.MergedDictionaries.OfType<ThemeResourceDictionary>()
                         select dictionary).ToList();

                    // remove the existing dictionaries
                    foreach (ThemeResourceDictionary thDictionary in existingDictionaries)
                    {
                        if (themeDictionaries.Contains(thDictionary)) continue;  // don't remove the newly added dictionary
                        element.Resources.MergedDictionaries.Remove(thDictionary);
                    }
                }
                finally { }

                if (element is MainWindow)
                {
                    // Bug when changing themes with listbox, it causes it to rearrange and scrolls to the top
                    (element as MainWindow).SetVerticalScrollPosition(verticalOffset);
                }
                OnThemeChanged(sender);
            }));
    }
}
