using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Common;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Converters
{
    public class IsUserIgnoredConverter : ConverterMarkupExtension<IsUserIgnoredConverter>
    {
        public IsUserIgnoredConverter()
        {

        }

        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ChatUser user = values[0] as ChatUser;
            
            if (user != null)
            {
                return Settings.Default.IgnoredUsers.Contains(user.Username);
            }
            return false;
        }
    }
}
