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

namespace MobiusEditor.Model
{
    public class TypeItem<T>: System.IEquatable<TypeItem<T>>, IComparable<TypeItem<T>>
    {
        public string Name { get; private set; }

        public T Type { get; private set; }

        public TypeItem(string name, T type)
        {
            Name = name;
            Type = type;
        }

        public bool Equals(TypeItem<T> other)
        {
            return other != null
                && ((this.Type == null && other.Type == null) || this.Type.Equals(other.Type))
                && String.Equals(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int CompareTo(TypeItem<T> other)
        {
            if (this.Equals(other))
                return 0;
            return String.Compare(this.Name ?? String.Empty, other.Name ?? String.Empty);
        }
    }
}
