using System;
using System.ComponentModel;
using System.Linq;
using HylandMedConfig.Common;

namespace HylandMedConfig.Converters
{
    public class CollectionViewItemFromIndexConverter : ConverterMarkupExtension<CollectionViewItemFromIndexConverter>
    {
        public CollectionViewItemFromIndexConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ICollectionView view = value as ICollectionView;
            int skip = System.Convert.ToInt32(parameter);
            if (view != null)
            {
                return view.OfType<ChatUser>().Skip(skip).FirstOrDefault();
            }
            return null;
        }
    }
}
