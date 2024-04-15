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
namespace MobiusEditor.RedAlert
{
    /// <summary>
    /// The remap info to change C1.SHP and C2.SHP into graphics for all civilian types.
    /// The actual mappings were reverse engineered from CONST.CPP in the original Red Alert source code:
    /// https://github.com/electronicarts/CnC_Remastered_Collection/blob/master/REDALERT/CONST.CPP
    /// The Einstein one is newly added.
    /// </summary>
    public static class InfantryClassicRemap
    {
        /// <summary>Remap table for civilian C2.</summary>
        public static readonly byte[] RemapCiv2 = BuildRemap((007, 209), (014, 012), (118, 187), (119, 188), (159, 209), (187, 167), (188, 013));
        /// <summary>Remap table for civilian C4.</summary>
        public static readonly byte[] RemapCiv4 = BuildRemap((007, 187), (109, 118), (111, 119), (206, 188), (210, 182));
        /// <summary>Remap table for civilian C5.</summary>
        public static readonly byte[] RemapCiv5 = BuildRemap((007, 109), (012, 131), (109, 177), (111, 178), (200, 111), (206, 111), (210, 182));
        /// <summary>Remap table for civilian C6.</summary>
        public static readonly byte[] RemapCiv6 = BuildRemap((007, 120), (014, 238), (118, 236), (119, 206), (159, 111));
        /// <summary>Remap table for civilian C7.</summary>
        public static readonly byte[] RemapCiv7 = BuildRemap((014, 131), (118, 157), (119, 212), (159, 007), (187, 118), (188, 119));
        /// <summary>Remap table for civilian C8.</summary>
        public static readonly byte[] RemapCiv8 = BuildRemap((007, 182), (014, 131), (118, 215), (119, 007), (159, 182), (187, 198), (188, 199), (200, 111));
        /// <summary>Remap table for civilian C9.</summary>
        public static readonly byte[] RemapCiv9 = BuildRemap((014, 007), (118, 163), (119, 165), (159, 200), (187, 111), (188, 013));
        /// <summary>Remap table for civilian C10.</summary>
        public static readonly byte[] RemapCiv10 = BuildRemap((007, 137), (014, 015), (118, 129), (119, 131), (159, 137), (187, 163), (188, 165));
        /// <summary>Remap table to fix the fact the DOS graphics of Einstein use the Mobius graphics.</summary>
        public static readonly byte[] RemapEinstein = BuildRemap((014, 120), (130, 138), (131, 119));

        public static byte[] BuildRemap(params (byte index, byte remap)[] tuples)
        {
            byte[] table = new byte[0x100];
            for (int i = 0; i < 0x100; ++i)
            {
                table[i] = (byte)i;
            }
            for (int i = 0; i < tuples.Length; ++i)
            {
                (byte index, byte remap) = tuples[i];
                table[index] = remap;
            }
            return table;
        }
    }
}
