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
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MobiusEditor.Interface
{
    public enum FileType
    {
        None,
        INI,
        BIN,
        MEG,
        PGM
    }

    public interface IGamePlugin : IDisposable
    {
        /// <summary>The game type information object.</summary>
        GameInfo GameInfo { get; }
        /// <summary>Currently edited house.</summary>
        HouseType ActiveHouse { get; set; }
        /// <summary>True if the plugin is initialised to handle a megamap.</summary>
        bool IsMegaMap { get; }
        /// <summary>The map.</summary>
        Map Map { get; }
        /// <summary>The map image to show.</summary>
        Image MapImage { get; }
        /// <summary>Feedback handler that can be attached to the plugin.</summary>
        IFeedBackHandler FeedBackHandler { get; set; }
        /// <summary>True if the currently loaded map was modified.</summary>
        bool Dirty { get; set; }

        /// <summary>Initialises this plugin after it has been created, and all resource managers have been reset.</summary>
        IEnumerable<string> Initialize();

        /// <summary>Get any unmanaged sections from the mission file so they can be edited by the user.</summary>
        /// <returns>All unmanaged sections from the mission file, as text.</returns>
        string GetExtraIniText();

        /// <summary>Store extra ini text after it has been edited by the user.</summary>
        /// <param name="extraIniText">The extra ini text to store</param>
        /// <param name="forFootprintTest">Don't apply changes, just test the result for <paramref name="footPrintsChanged"/></param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the given ini rule tweaks</param>
        /// <returns>Any errors that occurred while parsing <paramref name="extraIniText"/>, or null if nothing went wrong.</returns>
        IEnumerable<string> SetExtraIniText(String extraIniText, out bool footPrintsChanged);

        /// <summary>Test if setting extra ini text will result in footprint changes.</summary>
        /// <param name="extraIniText">The extra ini text to evaluate</param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the given ini rule tweaks</param>
        /// <returns>Any errors that occurred while parsing <paramref name="extraIniText"/>, or null if nothing went wrong.</returns>
        IEnumerable<string> TestSetExtraIniText(String extraIniText, bool isSolo, bool expansionEnabled, out bool footPrintsChanged);

        /// <summary>Create a new map in the chosen theater.</summary>
        /// <param name="theater">The name of the theater to use.</param>
        void New(string theater);

        /// <summary>Load a map.</summary>
        /// <param name="path">Path of the map to load.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be loaded correctly.</param>
        /// <returns>Any issues encountered when loading the map.</returns>
        IEnumerable<string> Load(string path, FileType fileType);

        /// <summary>Save the current map to the given path, with the given file type.</summary>
        /// <param name="path">Path of the map to save.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be saved correctly.</param>
        /// <returns>true if the saving succeeded.</returns>
        bool Save(string path, FileType fileType);

        /// <summary>Save the current map to the given path, with the given file type.</summary>
        /// <param name="path">Path of the map to save.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be saved correctly.</param>
        /// <param name="customPreview">Custom preview given to the map.</param>
        /// <param name="dontResavePreview">True to not resave the preview on disc when doing the save operation.</param>
        /// <returns>true if the saving succeeded.</returns>
        bool Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview);

        /// <summary>Validate the map to see if there are any blocking errors preventing it from saving.</summary>
        /// <param name="forWarnings">true if this is not the actual map validation, but a check that should return any warnings to show that the user can still choose to ignore.</param>
        /// <returns>Null if the validation succeeded, else a string containing the problems that occurred..</returns>
        string Validate(Boolean forWarnings);

        /// <summary>Generates an overview of how many items are on the map and how many are allowed, and does a trigger analysis.</summary>
        /// <returns>The generated map items overview.</returns>
        IEnumerable<string> AssessMapItems();

        /// <summary>Retrieves a hash set of all houses for which production is started by triggers.</summary>
        /// <returns>A hash set of all houses for which production is started by triggers.</returns>
        HashSet<string> GetHousesWithProduction();

        /// <summary>Returns an array containing the reveal radius for each waypoint on the map.</summary>
        /// <param name="forLargeReveal">False for small flare reveal, true for large area reveal.</param>
        /// <returns></returns>
        int[] GetRevealRadiusForWaypoints(bool forLargeReveal);

        /// <summary>
        /// Check whether there are any errors in the currect scripting.
        /// This is not put in GameInfo because it depends on map info like the waypoints.
        /// </summary>
        /// <param name="triggers">List of triggers to check.</param>
        /// <param name="includeExternalData">True to fetch extra data from the map, such as map objects that triggers can be linked to.</param>
        /// <param name="prefixNames">True to prefix the trigger name before every line. If false, each trigger's analysis will be preceded with a header line containing the trigger name.</param>
        /// <param name="fatalOnly">True to report fatal issues only.</param>
        /// <param name="fatal">Returns true if fatal issues were encountered.</param>
        /// <param name="fix">True to fix issues that are encountered whenever possible.</param>
        /// <param name="wasFixed">Returns true if issues were fixed.</param>
        /// <returns>A summation of the encountered issues.</returns>
        IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix, out bool wasFixed);

        /// <summary>Gives a readable string that shows the contents of a trigger.</summary>
        /// <param name="trigger">The trigger to summarise</param>
        /// <param name="currentTriggers">List of current triggers. This makes it usable in the trigger edit screen, taking unsaved data into account.</param>
        /// <returns>The summarisation of the trigger</returns>
        string TriggerSummary(Trigger trigger, List<Trigger> currentTriggers);

        /// <summary>Re-initialises the flag colors for this game.</summary>
        /// <returns>The team colors</returns>
        ITeamColor[] GetFlagColors();

        bool IsLandUnitPassable(LandType landType);
        bool IsBoatPassable(LandType landType);
        bool IsBuildable(LandType landType);
    }
}
