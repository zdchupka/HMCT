using HylandMedConfig.Properties;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace HylandMedConfig.Controls
{
	public class ChatMessageCallout : ContentControl
    {
        static ChatMessageCallout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChatMessageCallout), new FrameworkPropertyMetadata(typeof(ChatMessageCallout)));
        }
    }

    public class ChatExpander : HeaderedContentControl
    {
        private ToggleButton HeaderSite;
        private ContentPresenter ExpandSite;

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ChatExpander), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (s, a) =>
                {
                    
                    ChatExpander thisControl = s as ChatExpander;
                    thisControl.OnExpandChanged(EventArgs.Empty);
                    thisControl.Animate();
                }));

        public event EventHandler ExpandChanged;

        protected void OnExpandChanged(EventArgs e)
        {
			ExpandChanged?.Invoke( this, e );
		}

        public bool EnableAnimation
        {
            get { return (bool)GetValue(EnableAnimationProperty); }
            set { SetValue(EnableAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableAnimationProperty =
            DependencyProperty.Register("EnableAnimation", typeof(bool), typeof(ChatExpander), new PropertyMetadata(true));



        public bool IsAnimating
        {
            get { return (bool)GetValue(IsAnimatingProperty); }
            set { SetValue(IsAnimatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAnimating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAnimatingProperty =
            DependencyProperty.Register("IsAnimating", typeof(bool), typeof(ChatExpander), new PropertyMetadata(false));


        private void Animate()
        {
            if (ExpandSite != null)
            {
                double toValue = IsExpanded ? 301 : 0;
                double fromValue = IsExpanded ? 0 : 300;

                if (Settings.Default.EnableAnimations)
                {
                    Duration duration = new Duration(TimeSpan.FromMilliseconds(150));
                    DoubleAnimation animation = new DoubleAnimation(toValue, duration);
                    animation.From = fromValue;
                    animation.Completed += animation_Completed;
                    animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                    IsAnimating = true;
                    ExpandSite.BeginAnimation(ContentPresenter.WidthProperty, animation);
                }
                else
                {
                    IsAnimating = false;
                    Duration duration = new Duration(TimeSpan.FromMilliseconds(0));
                    DoubleAnimation animation = new DoubleAnimation(toValue, duration);
                    animation.From = fromValue;
                    ExpandSite.BeginAnimation(ContentPresenter.WidthProperty, animation);
                }
            }
        }

        static ChatExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChatExpander), new FrameworkPropertyMetadata(typeof(ChatExpander)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HeaderSite = this.EnsureTemplateChild<ToggleButton>("HeaderSite");
            ExpandSite = this.EnsureTemplateChild<ContentPresenter>("ExpandSite");

            Animate();
        }

        void animation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }
    }
}
