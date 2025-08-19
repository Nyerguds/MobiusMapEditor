//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MobiusEditor.Model
{
    /// <summary>
    /// <para>The GameInfo class is meant to get rid of most of the logic checking the
    /// <see cref="Model.GameType"/> enum and instead centralise it all in a set of game-specific
    /// classes implementating this one. The main principle of this class, as opposed to an
    /// <see cref="IGamePlugin"/> implementation, is that <b>it contains no variable data</b>.
    /// Its creation does not initialise anything.
    /// </para><para>
    /// It has properties and methods that fetch game-specific information or execute
    /// game-specific logic, but returns results based either on hardcoded data, or
    /// on unchanging data initialised on program startup. This means its methods
    /// should <i>never</i> use data from an <see cref="IGamePlugin"/> as argument.</para>
    /// </summary>
    public abstract class GameInfo
    {
        public const string TRIG_ARG_FORMAT = "{0}: {1}";
        public string[] PERSISTENCE_NAMES = { "first triggered", "all triggered", "each triggering" };
        #region properties
        /// <summary>GameType enum for this game.</summary>
        public abstract GameType GameType { get; }
        /// <summary>Name used for this game.</summary>
        public abstract string Name { get; }
        /// <summary>Short name used for this game.</summary>
        public abstract string ShortName { get; }
        /// <summary>Name used for this game in the mixcontent.ini definition.</summary>
        public abstract string IniName { get; }
        /// <summary>The Steam ID for this game.</summary>
        public abstract string SteamId { get; }
        /// <summary>True if the Steam map handling uses a mirror server that requires the maps to be public on the Steam workshop.</summary>
        public abstract bool PublishedMapsUseMirrorServer { get; }
        /// <summary>The Steam game name linked to this Steam ID.</summary>
        public abstract string SteamGameName { get; }
        /// <summary>Short version of the Steam game name linked to this Steam ID.</summary>
        public abstract string SteamGameNameShort { get; }
        /// <summary>Filename extension given to the uploaded singleplayer map file on the Steam workshop.</summary>
        public abstract string SteamFileExtensionSolo { get; }
        /// <summary>Filename extension given to the uploaded singleplayer map file on the Steam workshop.</summary>
        public abstract string SteamFileExtensionMulti { get; }
        /// <summary>File save type for the Steam workshop upload.</summary>
        public abstract FileType SteamFileType { get; }
        /// <summary>Default tags put on maps for this game.</summary>
        public abstract string[] SteamDefaultTags { get; }
        /// <summary>Tags put on singleplayer maps for this game.</summary>
        public abstract string[] SteamSoloTags { get; }
        /// <summary>Tags put on multiplayer maps for this game.</summary>
        public abstract string[] SteamMultiTags { get; }
        /// <summary>Extra tags this game's singleplayer maps can use.</summary>
        public abstract string[] SteamSoloExtraTags { get; }
        /// <summary>Extra tags this game's multiplayer maps can use.</summary>
        public abstract string[] SteamMultiExtraTags { get; }
        /// <summary>Default remaster folder for saving maps of this type.</summary>
        public abstract string DefaultSaveDirectory { get; }
        /// <summary>Types that can be opened and saved by this plugin.</summary>
        public abstract FileTypeInfo[] SupportedFileTypes { get; }
        /// <summary>Default extension used for maps of this game.</summary>
        public abstract FileType DefaultSaveType { get; }
        /// <summary>Default extension used for maps of this game when they were loaded from inside a .mix file.</summary>
        public abstract FileType DefaultSaveTypeFromMix { get; }
        /// <summary>Default extension used for maps of this game when they were loaded from inside a .pgm file.</summary>
        public abstract FileType DefaultSaveTypeFromPgm { get; }
        /// <summary>Location to look for mods for this game in the user's Documents folder.</summary>
        public abstract string ModFolder { get; }
        /// <summary>Identifier for this game in mod definition json files.</summary>
        public abstract string ModIdentifier { get; }
        /// <summary>Mods to load for this game.</summary>
        public abstract string ModsToLoad { get; }
        /// <summary>Name of the setting that configures this game's mods to load.</summary>
        public abstract string ModsToLoadSetting { get; }
        /// <summary>Gives the list of remastered .meg files to load for this game.</summary>
        public abstract string[] RemasterMegFiles { get; }
        /// <summary>Configured folder for this this game's Classic files</summary>
        public abstract string ClassicFolder { get; }
        /// <summary>Root folder of this game's Classic files under in the Remastered Collection's "Data" folder.</summary>
        public abstract string ClassicFolderRemaster { get; }
        /// <summary>Location of this game's Classic files to use under in the Remastered Collection's "Data" folder.</summary>
        public abstract string ClassicFolderRemasterData { get; }
        /// <summary>Default for this this game's Classic folder</summary>
        public abstract string ClassicFolderDefault { get; }
        /// <summary>Name of the setting that configures this game's Classic folder</summary>
        public abstract string ClassicFolderSetting { get; }
        /// <summary>File name of the classic strings file for this game.</summary>
        public abstract string ClassicStringsFile { get; }
        /// <summary>Map size for normal maps of this type.</summary>
        public abstract Size MapSize { get; }
        /// <summary>Map size for megamaps of this type.</summary>
        public abstract Size MapSizeMega { get; }
        /// <summary>Lists all theaters theoretically supported by this type.</summary>
        public abstract TheaterType[] AllTheaters { get; }
        /// <summary>Lists all theaters supported by this type which are actually found.</summary>
        public abstract TheaterType[] AvailableTheaters { get; }
        /// <summary>Whether megamaps are supported for this game.</summary>
        public abstract bool MegamapIsSupported { get; }
        /// <summary>Whether megamaps are optional for this game.</summary>
        public abstract bool MegamapIsOptional { get; }
        /// <summary>Whether maps of this game default to being megamaps.</summary>
        public abstract bool MegamapIsDefault { get; }
        /// <summary>Whether megamap support for this game is official, or only supported through mods.</summary>
        public abstract bool MegamapIsOfficial { get; }
        /// <summary>Whether single player scenarios exist for this game.</summary>
        public abstract bool HasSinglePlayer { get; }
        /// <summary>Whether mix files of this game can be in the new mix format</summary>
        public abstract bool CanUseNewMixFormat { get; }
        /// <summary>Maximum length of the saved ini data for a map of this game.</summary>
        public abstract long MaxDataSize { get; }
        /// <summary>Maximum amount of aircraft that can be added into a map of this game.</summary>
        public abstract int MaxAircraft { get; }
        /// <summary>Maximum amount of vessels that can be added into a map of this game.</summary>
        public abstract int MaxVessels { get; }
        /// <summary>Maximum amount of buildings that can be added into a map of this game.</summary>
        public abstract int MaxBuildings { get; }
        /// <summary>Maximum amount of infantry that can be added into a map of this game.</summary>
        public abstract int MaxInfantry { get; }
        /// <summary>Maximum amount of terrain that can be added into a map of this game.</summary>
        public abstract int MaxTerrain { get; }
        /// <summary>Maximum amount of units that can be added into a map of this game.</summary>
        public abstract int MaxUnits { get; }
        /// <summary>Maximum amount of triggers that can be added into a map of this game.</summary>
        public abstract int MaxTriggers { get; }
        /// <summary>Maximum amount of teams that can be added into a map of this game.</summary>
        public abstract int MaxTeams { get; }
        /// <summary>Threshold (1-256) at which the health bar colour changes from yellow to green in this game.</summary>
        public abstract int HitPointsGreenMinimum { get; }
        /// <summary>Threshold (1-256) at which the health bar colour changes from red to yellow in this game.</summary>
        public abstract int HitPointsYellowMinimum { get; }
        /// <summary>Returns the viewport size around the Home waypoint, for DOS resolution, in old pixels.</summary>
        public abstract Size ViewportSizeSmall { get; }
        /// <summary>Returns the extra sidebar part of the viewport size, for DOS resolution, in old pixels. This is always added to the left of the main viewport.</summary>
        public abstract Size ViewportSidebarSmall { get; }
        /// <summary>Returns the offset of the top-left corner of the viewport from the Home waypoint for DOS resolution, in old pixels.</summary>
        public abstract Point ViewportOffsetSmall { get; }
        /// <summary>Returns the viewport size around the Home waypoint, for Win95 resolution, in old pixels.</summary>
        public abstract Size ViewportSizeLarge { get; }
        /// <summary>Returns the extra sidebar part of the viewport size, for Win95 resolution, in old pixels. This is always added to the left of the main viewport.</summary>
        public abstract Size ViewportSidebarLarge { get; }
        /// <summary>Returns the offset of the top-left corner of the viewport from the Home waypoint for Win95 resolution, in old pixels.</summary>
        public abstract Point ViewportOffsetLarge { get; }
        /// <summary>Preferred type of overlay to use as UI icon.</summary>
        public abstract OverlayTypeFlag OverlayIconType { get; }
        /// <summary>Generic image usable as Steam thumbnail.</summary>
        public abstract Bitmap WorkshopPreviewGeneric { get; }
        /// <summary>Generic but game-specific image usable as Steam thumbnail.</summary>
        public abstract Bitmap WorkshopPreviewGenericGame { get; }

        #endregion

        #region functions
        /// <summary>
        /// Identifies if a given map is for this game, and returns the correct type for it.
        /// </summary>
        /// <param name="iniContents">Contents of the loaded ini file.</param>
        /// <param name="binContents">Contents of the loaded bin file.</param>
        /// <param name="contentWasSwapped">True if the primary opened file was the .bin one.</param>
        /// <param name="isMegaMap">Returns whether the given map is a megamap.</param>
        /// <param name="theater">Returns the theater of the map.</param>
        /// <returns></returns>
        public abstract FileType IdentifyMap(INI iniContents, byte[] binContents, bool contentWasSwapped, out bool isMegaMap, out string theater);
        /// <summary>
        /// Create game plugin for this game.
        /// </summary>
        /// <param name="mapImage">True if a map image should be created.</param>
        /// <param name="megaMap">True if this plugin will be handling a megamap.</param>
        /// <returns></returns>
        public abstract IGamePlugin CreatePlugin(bool mapImage, bool megaMap);
        /// <summary>
        /// Initialises the classic files for this game. If <paramref name="forRemaster"/> is false, this should do more detailed checks on which assets are required.
        /// </summary>
        /// <param name="mfm">MixfileManager to load the archives into.</param>
        /// <param name="loadErrors">List of load errors. </param>
        /// <param name="fileLoadErrors">A list to collect errors into.</param>
        /// <param name="forRemaster">Indicates if this init is for remastered mode.</param>
        public abstract void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster);
        /// <summary>Retrieves the typical opposing player for the given House name, e.g. for TD, GoodGuy will give BadGuy.</summary>
        /// <param name="player">The player to get the opposing player for.</param>
        /// <returns>The typical opposing player for the given House.</returns>
        public abstract string GetClassicOpposingPlayer(string player);
        /// <summary>Checks if the given map layer is relevant for this game type.</summary>
        /// <param name="mlf">The map layer flag to check.</param>
        /// <returns>True if the given layer is used for this game.</returns>
        public abstract bool SupportsMapLayer(MapLayerFlag mlf);
        /// <summary>Fetches the Waypoints-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The waypoints UI icon.</returns>
        public abstract Bitmap GetWaypointIcon();
        /// <summary>Fetches the Celltriggers-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The celltriggers UI icon.</returns>
        public abstract Bitmap GetCellTriggerIcon();
        /// <summary>Fetches the Select-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The select mode UI icon.</returns>
        public abstract Bitmap GetSelectIcon();
        /// <summary>Fetches the Capture icon for the building properties UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>An icon indicating this object is capturable.</returns>
        public abstract Bitmap GetCaptureIcon();
        /// <summary>Checks whether the briefing has any kind of issues concerning length or supported characters.</summary>
        /// <param name="briefing">The briefing to check</param>
        /// <returns>Null if everything is okay, otherwise any issues to show on the user interface.</returns>
        public abstract string EvaluateBriefing(string briefing);
        /// <summary>Checks whether the map has a default map name that is considered empty by this game type.</summary>
        /// <param name="name">Map name to check.</param>
        /// <returns>True if the given name is considered empty by this game type.</returns>
        public abstract bool MapNameIsEmpty(string name);
        /// <summary>Retrieves classic font info from this game to use for the requested role.</summary>
        public abstract string GetClassicFontInfo(ClassicFont font, TilesetManagerClassic tsmc, Color textColor, out bool crop, out Color[] palette);
        /// <summary>Get the Tile for the classic Fake label. If this returns nothing, the text is drawn using the FakeLables font.</summary>
        public abstract Tile GetClassicFakeLabel(TilesetManagerClassic tsm);
        /// <summary>Gets a filename without extension to call the items used for uploading to the Steam workshop</summary>
        /// <returns>A filename, without extension, to use as basis for how to call the items in the workshop upload.</returns>
        public abstract string GetSteamWorkshopFileName(IGamePlugin plugin);
        #endregion

        #region protected functions

        /// <summary>
        /// Creates a palette for a specific font.
        /// </summary>
        /// <param name="textColor">Requested color for the text. Probably won't match exactly since it is looked up in the palette.</param>
        /// <param name="clearIndices">Indices on the graphics that need to be cleared to transparent (index 0).</param>
        /// <returns>A color palette for the given color font.</returns>
        protected Color[] GetClassicFontPalette(Color textColor, params int[] clearIndices)
        {
            Color[] palette = Enumerable.Repeat(Color.Empty, 0x01)
                .Concat(Enumerable.Repeat(textColor, 0x0F))
                .Concat(Enumerable.Repeat(Color.Empty, 0xF0)).ToArray();
            for (int i = 0; i < clearIndices.Length; ++i)
            {
                int index = clearIndices[i];
                if (index > 0 && index < 0x10)
                {
                    palette[index] = Color.Empty;
                }
            }
            return palette;
        }

        protected static string GetMissionName(char side, int number, string suffix)
        {
            return GetMissionName(side, number, suffix, '\0');
        }

        protected static string GetMissionName(char side, int number, string suffix, char aftermathLetter)
        {
            const string formatNormal = "sc{0}{1:00}{2}";
            const string formatAm = "sc{0}{3}{1}{2}";
            return String.Format(aftermathLetter == '\0' ? formatNormal : formatAm, side, number, suffix, aftermathLetter);
        }

        public static bool IsCnCIni(INI iniContents)
        {
            return iniContents != null && INITools.CheckForIniInfo(iniContents, "Map") && INITools.CheckForIniInfo(iniContents, "Basic");
        }

        public static string GetTheater(INI iniContents)
        {
            return !INITools.CheckForIniInfo(iniContents, "Map") ? String.Empty : (iniContents["Map"].TryGetValue("Theater") ?? String.Empty).ToLower();
        }
        #endregion

        #region overrides
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }

    public enum ClassicFont
    {
        /// <summary>Font used for Waypoints.</summary>
        Waypoints,
        /// <summary>Font used for waypoints with longer names. Separate because it needs a smaller font to fit inside one cell.</summary>
        WaypointsLong,
        /// <summary>Font used for cell triggers.</summary>
        CellTriggers,
        /// <summary>Font used for techno triggers on multi-cell objects.</summary>
        TechnoTriggers,
        /// <summary>Font used for one-cell techno triggers. Separate because it might need to be smaller.</summary>
        TechnoTriggersSmall,
        /// <summary>Font used for rebuild priority numbers on buildings.</summary>
        RebuildPriority,
        /// <summary>Font used for "FAKE" labels on buildings.</summary>
        FakeLabels
    }
}
