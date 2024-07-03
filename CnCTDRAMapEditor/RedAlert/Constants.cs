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
using System.Drawing;

namespace MobiusEditor.RedAlert
{
    public static class Constants
    {
        public static readonly Size MaxSize = new Size(128, 128);

        public const string FileFilter = "Red Alert files (*.mpr;*.ini)|*.mpr;*.ini";

        public const int MaxBriefLengthClassic = 1022;
        public const int BriefLineCutoffClassic = 74;

        public const int DefaultGoldValue = 25;
        public const int DefaultGemValue = 50;
        public const int DefaultDropZoneRadius = 4;
        public const int DefaultGapRadius = 10;
        public const int DefaultJamRadius = 15;
        public const string EmptyMapName = "<none>";

        public const int MaxAircraft  /**/ = 100;
        public const int MaxVessels   /**/ = 100;
        public const int MaxBuildings /**/ = 500;
        public const int MaxInfantry  /**/ = 500;
        public const int MaxTerrain   /**/ = 500;
        public const int MaxUnits     /**/ = 500;
        public const int MaxTeams     /**/ = 60;
        public const int MaxTriggers  /**/ = 80;
        // The length of the globals array is 30, so this is the maximum index.
        public const int HighestGlobal   /**/ = 29;
    }
}
