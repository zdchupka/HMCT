using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using HylandMedConfig.Common;

namespace HylandMedConfig.Attached
{
    public class ContentElementWhisperBehavior : Behavior<FrameworkContentElement>
    {
        public ChatUser User
        {
            get { return (ChatUser)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(ChatUser), typeof(ContentElementWhisperBehavior), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.Cursor = Cursors.Hand;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Cursor = Cursors.Arrow;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseEventArgs e)
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
}
