﻿//
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
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

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

    public enum GameType
    {
        None,
        TiberianDawn,
        RedAlert,
        SoleSurvivor
    }

    public interface IGamePlugin : IDisposable
    {
        /// <summary>Name of the game.</summary>
        string Name { get; }

        /// <summary>The map.</summary>
        string DefaultExtension { get; }

        /// <summary>The game type as enum.</summary>
        GameType GameType { get; }

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

        /// <summary>Extra ini text that can be freely edited by the user.</summary>
        string ExtraIniText { get; set; }

        /// <summary>
        /// Create a new map in the chosen theater.
        /// </summary>
        /// <param name="theater">The name of the theater to use.</param>
        void New(string theater);

        /// <summary>
        /// Load a map.
        /// </summary>
        /// <param name="path">Path of the map to load.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be loaded correctly.</param>
        /// <returns>Any issues encountered when loading the map.</returns>
        IEnumerable<string> Load(string path, FileType fileType);

        /// <summary>
        /// Save the current map to the given path, with the given file type.
        /// </summary>
        /// <param name="path">Path of the map to save.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be saved correctly.</param>
        /// <returns>true if the saving succeeded.</returns>
        bool Save(string path, FileType fileType);

        /// <summary>
        /// Save the current map to the given path, with the given file type.
        /// </summary>
        /// <param name="path">Path of the map to save.</param>
        /// <param name="fileType">File type of the actual file in the path, so accompanying files can be saved correctly.</param>
        /// <param name="customPreview">Custom preview given to the map.</param>
        /// <param name="dontResavePreview">True to not resave the preview on disc when doing the save operation.</param>
        /// <returns>true if the saving succeeded.</returns>
        bool Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview);

        /// <summary>
        /// Validate the map to see if there are any blocking errors preventing it from saving.
        /// </summary>
        /// <param name="forWarnings">true if this is not the actual map validation, but a check that should return any warnings to show that the user can still choose to ignore.</param>
        /// <returns>true if the validation succeeded.</returns>
        string Validate(Boolean forWarnings);

        /// <summary>
        /// Generates an overview of how many items are on the map and how many are allowed, and does a trigger analysis.
        /// </summary>
        /// <returns>The generated map items overview.</returns>
        IEnumerable<string> AssessMapItems();

        /// <summary>
        /// Retrieves a hash set of all houses for which production is started by triggers.
        /// </summary>
        /// <returns>A hash set of all houses for which production is started by triggers.</returns>
        HashSet<string> GetHousesWithProduction();

        /// <summary>
        /// Returns an array containing the reveal radius for each waypoint on the map.
        /// </summary>
        /// <param name="map">The map to gheck the waypoints on.</param>
        /// <param name="forLargeReveal">False for small flare reveal, true for large area reveal.</param>
        /// <returns></returns>
        int[] GetRevealRadiusForWaypoints(Map map, bool forLargeReveal);

        /// <summary>
        /// Check whether there are any errors in the currect scripting.
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

        /// <summary>
        /// Checks whether the name has a default map name that is considered empty by this game plugin.
        /// </summary>
        /// <param name="name">Map name to check.</param>
        /// <returns>True if the given name is considered empty by this game plugin.</returns>
        bool MapNameIsEmpty(string name);

        /// <summary>
        /// Checks whether the briefing has any kind of issues concerning length or supported characters.
        /// </summary>
        /// <param name="briefing">The briefing to check</param>
        /// <returns>Null if everything is okay, otherwise any issues to show on the user interface.</returns>
        string EvaluateBriefing(string briefing);

        /// <summary>
        /// Re-initialises the flag colors for this game.
        /// </summary>
        /// <returns></returns>
        ITeamColor[] GetFlagColors();

        bool IsLandUnitPassable(LandType landType);
        bool IsBoatPassable(LandType landType);
        bool IsBuildable(LandType landType);
    }
}
