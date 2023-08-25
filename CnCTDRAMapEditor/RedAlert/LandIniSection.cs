using MobiusEditor.Model;
using MobiusEditor.Utility;
using System.ComponentModel;

namespace MobiusEditor.RedAlert
{
    public class LandIniSection
    {
        public LandIniSection(int footSpeed, int trackSpeed, int wheelSpeed, int floatSpeed, bool buildable)
        {
            this.Foot = footSpeed;
            this.Track = trackSpeed;
            this.Wheel = wheelSpeed;
            this.Float = floatSpeed;
            this.Buildable = buildable;
        }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Foot { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Track { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Wheel { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Float { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Buildable { get; set; }
    }
}
