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
using Steamworks;
using System;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MobiusEditor.Utility;

namespace MobiusEditor.Model
{
    public class SteamSection
    {
        [NonSerializedINIKey]
        public ERemoteStoragePublishedFileVisibility VisibilityAsEnum
        {
            get
            {
                string v = Visibility;
                ERemoteStoragePublishedFileVisibility def = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic;
                if (string.IsNullOrWhiteSpace(v))
                {
                    return def;
                }
                v = v.Trim();
                List<ERemoteStoragePublishedFileVisibility> visTypes =
                    Enum.GetValues(typeof(ERemoteStoragePublishedFileVisibility)).Cast<ERemoteStoragePublishedFileVisibility>().ToList();
                if (Regex.IsMatch(v, "^\\d+$") && int.TryParse(v, out int visib))
                {
                    foreach (ERemoteStoragePublishedFileVisibility vChoice in visTypes)
                    {
                        if (visib == (int)vChoice)
                        {
                            return vChoice;
                        }
                    }
                    return def;
                }
                foreach (ERemoteStoragePublishedFileVisibility vChoice in visTypes)
                {
                    if (v.Equals(vChoice.ToString()))
                    {
                        return vChoice;
                    }
                }
                // Just messing around lol.
                if (v.ToLower().Contains("public"))
                {
                    return ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic;
                }
                if (v.ToLower().Contains("friend"))
                {
                    return ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly;
                }
                if (v.ToLower().Contains("private"))
                {
                    return ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
                }
                return def;
            }
            set { Visibility = ((int)value).ToString(); }
        }

        [DefaultValue(null)]
        public string Title { get; set; }
        
        [DefaultValue(null)]
        public string Author { get; set; }

        [DefaultValue(null)]
        public string Description { get; set; }

        [DefaultValue(null)]
        public string PreviewFile { get; set; }

        [DefaultValue("0")]
        public string Visibility { get; set; }

        [DefaultValue(typeof(ulong), "0")]
        public ulong PublishedFileId { get; set; }

        [DefaultValue(null)]
        public string Tags { get; set; }
    }
}
