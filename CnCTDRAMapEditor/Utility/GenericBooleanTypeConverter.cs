using MobiusEditor.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

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
        private static readonly Regex NumRegex = new Regex("^\\d+$", RegexOptions.Compiled);

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
            return ConvertFrom(str);
        }

        public bool ConvertFrom(string value)
        {
            return Parse(value);
        }

        public static bool Parse(string value)
        {
            if (value == null)
            {
                return false;
            }
            value = value.Trim();
            // If is numeric, any value higher than 0 us true.
            bool isNumeric = NumRegex.IsMatch(value);
            if (isNumeric)
            {
                value = value.TrimStart('0');
            }
            if (value.Length == 0)
            {
                return false;
            }
            char first = (isNumeric && Int32.Parse(value) != 0) ? '1' : value.ToUpper()[0];
            return (first == 'T') || (first == 'Y') || (first == '1');
        }
    }
}
