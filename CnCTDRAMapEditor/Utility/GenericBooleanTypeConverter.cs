//
// Copyright 2020 Rami Pasanen
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
