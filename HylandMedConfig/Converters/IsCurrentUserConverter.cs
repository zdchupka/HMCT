using System;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
    public class IsCurrentUserConverter : ConverterMarkupExtension<IsCurrentUserConverter>
    {
        public IsCurrentUserConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool inverse = string.Equals(parameter, "NOT");
            ChatUser user = value as ChatUser;
            bool isCurrentUser = false;

            if (user != null)
            {
                isCurrentUser = user == ApplicationViewModel.Current.ChatProxy.CurrentUser;
            }

            if (inverse)
            {
                isCurrentUser = !isCurrentUser;
            }

            return isCurrentUser;
        }
    }
}
