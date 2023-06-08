//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace MobiusEditor.Model
{
    public class TheaterTypeConverter : TypeConverter
    {
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
            if (!(value is TheaterType) || !CanConvertTo(context, destinationType))
            {
                return null;
            }

            return (value as TheaterType)?.Name;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!CanConvertFrom(context, value?.GetType()))
            {
                return null;
            }
            return ConvertFrom(context as MapContext, value as string);
        }

        public TheaterType ConvertFrom(MapContext context, string value)
        {
            var map = context.Map;
            return map.TheaterTypes.Where(t => t.Equals(value)).FirstOrDefault() ?? map.TheaterTypes.First();
        }
    }

    public class MapSection : NotifiableIniSection
    {
        private readonly int fullWidth;
        private readonly int fullHeight;

        public MapSection(int fullWidth, int fullHeight)
        {
            this.fullWidth = fullWidth;
            this.fullHeight = fullHeight;
        }

        public MapSection(Size fullSize)
        {
            this.fullWidth = fullSize.Width;
            this.fullHeight = fullSize.Height;
        }

        public void FixBounds()
        {
            int fixedX = Math.Max(1, Math.Min(fullWidth - 2, x));
            int fixedY = Math.Max(1, Math.Min(fullHeight - 2, y));
            int fixedWidth = Math.Max(1, Math.Min(fullWidth - x - 1, width));
            int fixedHeight = Math.Max(1, Math.Min(fullHeight - y - 1, height));
            // Assign fixed values.
            X = fixedX;
            Y = fixedY;
            Width = fixedWidth;
            Height = fixedHeight;
        }

        private int x;
        [DefaultValue(1)]
        public int X
        {
            get { return x; }
            set
            {
                value = Math.Max(1, Math.Min(fullWidth - 2, value));
                SetField(ref x, value);
            }
        }

        private int y;
        [DefaultValue(1)]
        public int Y
        {
            get { return y; }
            set
            {
                value = Math.Max(1, Math.Min(fullWidth - 2, value));
                SetField(ref y, value);
            }
        }

        private int width;
        [DefaultValue(1)]
        public int Width
        {
            get { return width; }
            set
            {
                value = Math.Max(1, Math.Min(fullWidth - 2, value));
                SetField(ref width, value);
            }
        }

        private int height;
        [DefaultValue(1)]
        public int Height
        {
            get { return height; }
            set
            {
                value = Math.Max(1, Math.Min(fullHeight - 2, value));
                SetField(ref height, value);
            }
        }

        private TheaterType theater;
        [TypeConverter(typeof(TheaterTypeConverter))]
        [DefaultValue(null)]
        public TheaterType Theater { get => theater; set => SetField(ref theater, value); }
    }
}
