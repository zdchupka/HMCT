using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using HylandMedConfig.Common;
using System.Collections;
using WpfAnimatedGif;
using System.Windows.Input;

namespace HylandMedConfig.Controls
{
	public class MyScrollViewer : ScrollViewer
	{
		public void asdf()
		{
			
		}
	}

    public partial class ChatMessageList : ItemsControl
    {
        public ApplicationViewModel ViewModel
        {
            get
            {
                return DataContext as ApplicationViewModel;
            }
        }

        public ChatMessageList()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += (sender, args) =>
                {
                    ViewModel.MessagesChanged += ViewModel_MessagesChanged;
                };
            }
        }

        public double GetVerticalScrollPosition()
        {
            ScrollViewer sv = this.Template.FindName("sv", this) as ScrollViewer;
            if (sv != null)
            {
                return sv.VerticalOffset;
            }
            return 0d;
        }


        public void SetVerticalScrollPosition(double offset)
        {
            ScrollViewer sv = this.Template.FindName("sv", this) as ScrollViewer;
            if (sv != null)
            {
                sv.ScrollToVerticalOffset(offset);
            }
        }

        public void ScrollToBottom(bool force = false)
        {
            ScrollViewer sv = this.Template.FindName("sv", this) as ScrollViewer;
            if (sv != null)
			{
				if( force || ( sv.VerticalOffset == sv.ScrollableHeight ) )
				{
					sv.ScrollToEnd();
				}
			}
        }

		public void ScrollToTop()
		{
			ScrollViewer sv = this.Template.FindName( "sv", this ) as ScrollViewer;
			if( sv != null )
			{
				sv.ScrollToHome();
			}
		}

        private void ViewModel_MessagesChanged(object sender, EventArgs e)
        {

            ScrollToBottom();
        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        private void sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool enableShit = true;
            if (enableShit)
            {
                ScrollViewer sv = sender as ScrollViewer;
                var visibleItems = new List<int>();
                Rect svViewportBounds = new Rect(sv.HorizontalOffset, sv.VerticalOffset, sv.ViewportWidth, sv.ViewportHeight);

                for (int i = 0; i < Items.Count; ++i)
                {
                    ContentPresenter container = this.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;

                    if (container != null)
                    {
                        var offset = VisualTreeHelper.GetOffset(container);
                        var bounds = new Rect(offset.X, offset.Y, container.ActualWidth, container.ActualHeight);
                        ChatUserMessage message = container.Content as ChatUserMessage;

                        if (message != null)
                        {
                            MessageBubble messageBubble = VisualTreeHelper.GetChild(container, 0) as MessageBubble;

                            if (messageBubble != null)
                            {
                                if (svViewportBounds.IntersectsWith(bounds))
                                {
                                    ImageBehavior.SetIsInView(messageBubble, true);
                                }
                                else
                                {
                                    ImageBehavior.SetIsInView(messageBubble, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
