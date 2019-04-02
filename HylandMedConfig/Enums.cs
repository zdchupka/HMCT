using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
    public enum UITheme : int
    {
        [Description("None")]
        None = -1,

        [Description("Infragistics")]
        IG = 0,

        [Description("Metro")]
        Metro = 1,

        [Description("Metro Dark")]
        MetroDark = 2,
    }

    public class DescriptionAttribute : Attribute
    {
        public string Description
        {
            get;
            private set;
        }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
