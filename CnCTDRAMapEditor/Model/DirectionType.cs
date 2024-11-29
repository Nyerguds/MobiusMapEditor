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
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    public class DirectionType
    {
        public byte ID { get; private set; }

        public string Name { get; private set; }

        public FacingType Facing { get; private set; }

        public DirectionType(byte id, string name, FacingType facing)
        {
            ID = id;
            Name = name;
            Facing = facing;
        }

        public DirectionType(byte id, string name)
            : this(id, name, FacingType.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is DirectionType)
            {
                return this == obj;
            }
            else if (obj is byte)
            {
                return ID == (byte)obj;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }
            else if (obj is FacingType)
            {
                return Facing == (FacingType)obj;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Tries to match a given value to the ID of one of the DirectionType objects in the list.
        /// If it fails, <paramref name="output"/> will contain the item with the lowest ID from <paramref name="directions"/>,
        /// so it always results in a valid object.
        /// </summary>
        /// <param name="value">Value to match to an ID.</param>
        /// <param name="directions">Valid directions to pick from.</param>
        /// <param name="output">The found match, or, if it failed, the lowest-ID item from <paramref name="directions"/>.</param>
        /// <returns>True if the matching succeeded, false if the default value was returned.</returns>
        public static bool TryGetDirectionType(int value, IEnumerable<DirectionType> directions, out DirectionType output)
        {
            IEnumerable<DirectionType> sortedDirections = directions.OrderBy(d => d.ID);
            DirectionType foundType = null;
            foreach (DirectionType dirType in sortedDirections)
            {
                if (dirType.ID >= value)
                {
                    foundType = dirType;
                }
            }
            output = foundType ?? sortedDirections.FirstOrDefault();
            return foundType != null;
        }

        /// <summary>
        /// Tries to match a given value to the ID of one of the DirectionType objects in the list.
        /// If it fails, <paramref name="output"/> will contain the given <paramref name="defaultVal"/>.
        /// </summary>
        /// <param name="value">Value to match to an ID.</param>
        /// <param name="directions">Valid directions to pick from.</param>
        /// <param name="defaultVal"></param>
        /// <param name="output">The found match, or, if it failed, <paramref name="defaultVal"/>.</param>
        /// <returns>True if the matching succeeded, false if the default value was returned.</returns>
        public static bool TryGetDirectionType(int value, IEnumerable<DirectionType> directions, DirectionType defaultVal, out DirectionType output)
        {
            bool found = TryGetDirectionType(value, directions, out output);
            if (!found)
            {
                output = defaultVal;
            }
            return found;
        }

        /// <summary>
        /// Tries to match a given value to the ID of one of the DirectionType objects in the list.
        /// If it fails, it returns the item with the lowest ID from <paramref name="directions"/>,
        /// so it always results in a valid object.
        /// </summary>
        /// <param name="value">Value to match to an ID.</param>
        /// <param name="directions">Valid directions to pick from.</param>
        /// <returns>True if the matching succeeded, false if the default value was returned.</returns>
        public static DirectionType GetDirectionType(int value, IEnumerable<DirectionType> directions)
        {
            TryGetDirectionType(value, directions, out DirectionType output);
            return output;
        }

    }
}
