//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MobiusEditor.Model
{
    public enum InfantryStoppingType
    {
        Center     /**/ = 0,
        UpperLeft  /**/ = 1,
        UpperRight /**/ = 2,
        LowerLeft  /**/ = 3,
        LowerRight /**/ = 4
    }

    [DebuggerDisplay("{Type}: {Trigger}")]
    public class Infantry : ITechno, INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public InfantryGroup InfantryGroup { get; set; }

        private InfantryType type;
        public InfantryType Type { get => type; set => SetField(ref type, value); }
        public ITechnoType TechnoType => type;

        private HouseType house;
        public HouseType House { get => house; set => SetField(ref house, value); }

        private int strength = 256;
        public int Strength { get => strength; set => SetField(ref strength, value); }

        private DirectionType direction;
        public DirectionType Direction { get => direction; set => SetField(ref direction, value); }

        private string mission;
        public string Mission { get => mission; set => SetField(ref mission, value); }

        private string trigger = Model.Trigger.None;
        public string Trigger { get => trigger; set => SetField(ref trigger, value); }

        public bool IsPreview { get; set; }
        public int DrawOrderCache { get; set; }
        public int DrawFrameCache { get; set; }
        public bool IsOverlapped { get; set; }

        public Infantry(InfantryGroup infantryGroup)
        {
            InfantryGroup = infantryGroup;
        }

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }

        public Infantry Clone()
        {
            Infantry clone = new Infantry(InfantryGroup);
            clone.CloneDataFrom(this);
            return clone;
        }

        public void CloneDataFrom(Infantry other)
        {
            Type = other.Type;
            House = other.House;
            Strength = other.Strength;
            Direction = other.Direction;
            Trigger = other.Trigger;
            Mission = other.Mission;
            DrawOrderCache = other.DrawOrderCache;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        object ICloneable.Clone()
        {
            return Clone();
        }

        public bool DataEquals(Infantry other)
        {
            return Type == other.Type &&
                House == other.House &&
                Strength == other.Strength &&
                Direction == other.Direction &&
                Trigger == other.Trigger &&
                Mission == other.Mission;
        }
    }

    public class InfantryGroup : ICellOverlapper, ICellOccupier
    {
        private static readonly Point[] stoppingLocations = new Point[Globals.NumInfantryStops];

        public Rectangle OverlapBounds => new Rectangle(-1, -1, 3, 3);
        public bool[,][] OverlapMask => new bool[1, 1][] { { Infantry.Select(loc => loc != null).ToArray() } };
        public Point OverlapMaskOffset => Point.Empty;
        public bool[,][] ContentMask => OverlapMask;
        public Point ContentMaskOffset => Point.Empty;
        public bool[,] OccupyMask => new bool[1, 1] { { true } };
        public bool[,] BaseOccupyMask => new bool[1, 1] { { true } };
        public int ZOrder => Globals.ZOrderDefault;
        public int DrawOrderCache { get; set; }

        public readonly Infantry[] Infantry = new Infantry[Globals.NumInfantryStops];

        static InfantryGroup()
        {
            stoppingLocations[(int)InfantryStoppingType.Center] = new Point(Globals.PixelWidth / 2, Globals.PixelHeight / 2);
            stoppingLocations[(int)InfantryStoppingType.UpperLeft] = new Point(Globals.PixelWidth / 4, Globals.PixelHeight / 4);
            stoppingLocations[(int)InfantryStoppingType.UpperRight] = new Point(3 * Globals.PixelWidth / 4, Globals.PixelHeight / 4);
            stoppingLocations[(int)InfantryStoppingType.LowerLeft] = new Point(Globals.PixelWidth / 4, 3 * Globals.PixelHeight / 4);
            stoppingLocations[(int)InfantryStoppingType.LowerRight] = new Point(3 * Globals.PixelWidth / 4, 3 * Globals.PixelHeight / 4);
        }

        public static IEnumerable<InfantryStoppingType> ClosestStoppingTypes(Point subPixel)
        {
            var stoppingDistances = new (InfantryStoppingType type, float dist)[stoppingLocations.Length];
            for (int i = 0; i < stoppingDistances.Length; ++i)
            {
                stoppingDistances[i] = ((InfantryStoppingType)i, new Vector2(subPixel.X - stoppingLocations[i].X, subPixel.Y - stoppingLocations[i].Y).LengthSquared());
            }
            return stoppingDistances.OrderBy(sd => sd.dist).Select(sd => sd.type);
        }

        public static InfantryStoppingType[] RenderOrder =
        {
            InfantryStoppingType.UpperRight,
            InfantryStoppingType.UpperLeft,
            InfantryStoppingType.Center,
            InfantryStoppingType.LowerRight,
            InfantryStoppingType.LowerLeft,
        };

        public static Point RenderPosition(InfantryStoppingType ist, bool adjust)
        {
            // Start with center
            Point offset = new Point(Globals.PixelWidth / 2, Globals.PixelHeight / 2);
            // Spread out 4 points around center
            switch (ist)
            {
                case InfantryStoppingType.UpperLeft:
                    offset.X -= Globals.PixelWidth / 4;
                    offset.Y -= Globals.PixelHeight / 4;
                    break;
                case InfantryStoppingType.UpperRight:
                    offset.X += Globals.PixelWidth / 4;
                    offset.Y -= Globals.PixelHeight / 4;
                    break;
                case InfantryStoppingType.LowerLeft:
                    offset.X -= Globals.PixelWidth / 4;
                    offset.Y += Globals.PixelHeight / 4;
                    break;
                case InfantryStoppingType.LowerRight:
                    offset.X += Globals.PixelWidth / 4;
                    offset.Y += Globals.PixelHeight / 4;
                    break;
                case InfantryStoppingType.Center:
                    break;
            }
            if (adjust)
            {
                // Add corrections to get feet locations. These values are experimental, from comparing the map editor to game screenshots.
                offset.X -= Globals.PixelWidth / 12; // X minus 2 pixels
                offset.Y += Globals.PixelHeight / 6; // Y plus 4 pixels
            }
            return offset;
        }

        private static readonly string[] StoppingTypeNames =
        {
            "Center",
            "Top left",
            "Top right",
            "Bottom left",
            "Bottom right"
        };

        public static string GetStoppingTypeName(InfantryStoppingType stopLocation)
        {
            int index = (int)stopLocation;
            if (index >= 0 && index < Enum.GetValues(typeof(InfantryStoppingType)).Length)
            {
                return StoppingTypeNames[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the location of a specific object inside this infantry group's <see cref="Infantry"/> array.
        /// </summary>
        /// <param name="infantry">The infantry to look up.</param>
        /// <returns>The index of the given infantry object in the group.</returns>
        public int GetLocation(Infantry infantry)
        {
            int location = -1;
            if (infantry == null || infantry.InfantryGroup == null)
            {
                return location;
            }
            for (int i = 0; i < infantry.InfantryGroup.Infantry.Length; ++i)
            {
                if (ReferenceEquals(infantry.InfantryGroup.Infantry[i], infantry))
                {
                    location = i;
                    break;
                }
            }
            return location;
        }
    }
}
