﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HylandMedConfig.Converters
{
    public class ThicknessToDoubleConverter : ConverterMarkupExtension<ThicknessToDoubleConverter>
    {
        public ThicknessToDoubleConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            return thickness.Left;
        }
    }
}
