using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Utility
{
    public class TrueFalseBooleanTypeConverter : YesNoBooleanTypeConverter
    {
        public TrueFalseBooleanTypeConverter()
        {
            BooleanStringStyle = BooleanStringStyle.TrueFalse;
        }
    }

    public class OneZeroBooleanTypeConverter : YesNoBooleanTypeConverter
    {
        public OneZeroBooleanTypeConverter()
        {
            BooleanStringStyle = BooleanStringStyle.OneZero;
        }
    }

    public class YesNoBooleanTypeConverter : TypeConverter
    {
        public BooleanStringStyle BooleanStringStyle { get; set; } = BooleanStringStyle.YesNo;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (context is MapContext) && (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (context is MapContext) && (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(value is bool boolean) || !CanConvertTo(context, destinationType))
            {
                return null;
            }

            switch (BooleanStringStyle)
            {
                case BooleanStringStyle.TrueFalse:
                    return boolean ? "true" : "false";
                case BooleanStringStyle.YesNo:
                    return boolean ? "yes" : "no";
                case BooleanStringStyle.OneZero:
                    return boolean ? "1" : "0";
                default:
                    throw new InvalidOperationException("GenericBooleanTypeConverter: Unknown boolean string style");
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string str) || !CanConvertFrom(context, value?.GetType()))
            {
                return null;
            }

            int first = (str.Length > 0) ? str.ToUpper()[0] : 0;
            return (first == 'T') || (first == 'Y') || (first == '1');
        }
    }
}
