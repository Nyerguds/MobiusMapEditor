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
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    public class TheaterType
    {
        public sbyte ID { get; private set; }
        public string Name { get; private set; }
        /// <summary>Basic filename without extension of the classic tileset files.</summary>
        public string MainTileset { get; private set; }
        /// <summary>Basic filename without extension of the classic tileset files.</summary>
        public string ClassicTileset { get; private set; }
        /// <summary>File extension of the files inside the classic .mix archive.</summary>
        public string ClassicExtension { get; private set; }
        /// <summary>True if the theater is not an official one.</summary>
        public bool IsModTheater { get; set; }
        /// <summary>Used to store the status of the startup checks that see if the theater mix file was found.</summary>
        public bool IsClassicMixFound { get; set; }
        /// <summary>Used to store the status of the startup checks that see if the remastered tileset definition for this theater was found.</summary>
        public bool IsRemasterTilesetFound { get; set; }

        public IEnumerable<string> Tilesets { get; private set; }

        public TheaterType(sbyte id, string name, string classicTileset, string classicExt, bool modTheater, string mainTileSet, IEnumerable<string> tilesets)
        {
            ID = id;
            Name = name;
            ClassicTileset = classicTileset;
            MainTileset = mainTileSet;
            ClassicExtension = classicExt;
            IsClassicMixFound = false;
            IsRemasterTilesetFound = false;
            IsModTheater = modTheater;
            Tilesets = mainTileSet.Yield().Concat(tilesets?.Distinct() ?? new List<String>());
        }

        public TheaterType(sbyte id, string name, string classicTileset, string classicExt, bool modTheater, string mainTileSet, params string[] tilesets)
            : this(id, name, classicTileset, classicExt, modTheater, mainTileSet, (IEnumerable<String>)tilesets)
        { }

        public TheaterType(sbyte id, string name, string classicTileset, string classicExt, string mainTileSet, IEnumerable<string> tilesets)
            : this(id, name, classicTileset, classicExt, false, mainTileSet, tilesets)
        { }

        public TheaterType(sbyte id, string name, string classicTileset, string classicExt, string mainTileSet, params string[] tilesets)
            : this(id, name, classicTileset, classicExt, false, mainTileSet, (IEnumerable<String>)tilesets)
        { }

        public override bool Equals(object obj)
        {
            if (obj is TheaterType)
            {
                return this == obj;
            }
            else if (obj is sbyte)
            {
                return ID == (sbyte)obj;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public bool IsAvailable()
        {
            return Globals.UseClassicFiles ? this.IsClassicMixFound : this.IsRemasterTilesetFound;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
