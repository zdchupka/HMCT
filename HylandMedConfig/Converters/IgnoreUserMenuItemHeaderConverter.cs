using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Converters
{
    public class IgnoreUserMenuItemHeaderConverter : ConverterMarkupExtension<IgnoreUserMenuItemHeaderConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ChatUser user = values[0] as ChatUser;
            if (user != null)
            {
                if (Settings.Default.IgnoredUsers.Contains(user.Username))
                {
                    return "Un-Ignore User";
                }
                else
                {
                    return "Ignore User";
                }
            }
            return "";
        }
    }

    public class MuteTagItemHeaderConverter : ConverterMarkupExtension<MuteTagItemHeaderConverter>
    {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return "Un-Mute Tag";
            }
            else
            {
                return "Mute Tag";
            }
        }
    }
}
