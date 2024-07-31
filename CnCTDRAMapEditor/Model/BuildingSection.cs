using MobiusEditor.Utility;
using System.ComponentModel;

namespace MobiusEditor.Model
{
    public class BuildingSection
    {
        [DefaultValue(0)]
        public int Power { get; set; }

        [DefaultValue(0)]
        public int Storage { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Bib { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Capturable { get; set; }
    }
}
