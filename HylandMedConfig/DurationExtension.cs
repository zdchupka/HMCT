using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using HylandMedConfig.Properties;

namespace HylandMedConfig
{
    public class DurationExtension : MarkupExtension
    {
        private string _duration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        public DurationExtension(string duration)
        {
            _duration = duration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!Settings.Default.EnableAnimations)
            {
                return new Duration(TimeSpan.Zero);
            }
            else
            {
                return new Duration(TimeSpan.Parse(_duration));
            }
        }
    }

    public class TimespanExtension : MarkupExtension
    {
        private string _duration;

        public TimespanExtension(string duration)
        {
            _duration = duration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!Settings.Default.EnableAnimations)
            {
                return TimeSpan.Zero;
            }
            else
            {
                return TimeSpan.Parse(_duration);
            }
        }
    }
}
