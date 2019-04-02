using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Attached
{
    public static class WindowBehavior
    {


        public static bool GetMuteMediaElement(DependencyObject obj)
        {
            return (bool)obj.GetValue(MuteMediaElementProperty);
        }

        public static void SetMuteMediaElement(DependencyObject obj, bool value)
        {
            obj.SetValue(MuteMediaElementProperty, value);
        }

        // Using a DependencyProperty as the backing store for MuteMediaElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MuteMediaElementProperty =
            DependencyProperty.RegisterAttached("MuteMediaElement", typeof(bool), typeof(WindowBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, (s, a) =>
                {
                    MediaElement m = s as MediaElement;
                    if (m != null)
                    {
                        m.IsMuted = System.Convert.ToBoolean(a.NewValue);
                    }
                }));

        
    }
}
