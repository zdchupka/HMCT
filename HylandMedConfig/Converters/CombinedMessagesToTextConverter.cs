using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
    public class CombinedMessagesToTextConverter : ConverterMarkupExtension<CombinedMessagesToTextConverter>
    {
        public CombinedMessagesToTextConverter()
        {
            
        }

        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<NormalChatUserMessage> messages = values[0] as ObservableCollection<NormalChatUserMessage>;

            if (messages != null)
            {
                return string.Join(Environment.NewLine, messages.Select(m => m.Text));
            }
            return string.Empty;
        }
    }
}
