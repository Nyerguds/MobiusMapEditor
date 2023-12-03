using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
// Special. Technically color "JP" exists for this, but it's wrong. Clone Spain instead.

namespace MobiusEditor.Model
{
    /// <summary>
    /// <para>The GameInfo class is meant to get rid of logic checking the <see cref="Model.GameType"/> enum
    /// and centralise it all in a set of game-specific classes instead. The main
    /// principle of this class, as opposed to an <see cref="IGamePlugin"/> implementation, is
    /// that <b>it contains no variable data</b>. Its creation does not initialise anything.
    /// </para><para>
    /// It has properties and methods that fetch game-specific information or execute
    /// game-specific logic, but returns results based either on hardcoded data, or
    /// on unchanging data initialised on program startup. This means its methods
    /// should <i>never</i> use data from an <see cref="IGamePlugin"/> as argument.</para>
    /// </summary>
    public abstract class GameInfo
    {
        #region properties
        public abstract GameType GameType { get; }
        public abstract string Name { get; }
        public abstract string DefaultSaveDirectory { get; }
        public abstract string SaveFilter { get; }
        public abstract string OpenFilter { get; }
        public abstract string DefaultExtension { get; }
        public abstract string ModFolder { get; }
        public abstract string ModIdentifier { get; }
        public abstract string ModsToLoad { get; }
        public abstract string ModsToLoadSetting { get; }
        public abstract string WorkshopTypeId { get; }
        public abstract string ClassicFolder { get; }
        public abstract string ClassicFolderRemaster { get; }
        public abstract string ClassicFolderDefault { get; }
        public abstract string ClassicFolderSetting { get; }
        public abstract string ClassicStringsFile { get; }
        public abstract TheaterType[] AllTheaters { get; }
        public abstract TheaterType[] AvailableTheaters { get; }
        public abstract bool MegamapSupport { get; }
        public abstract bool MegamapOptional { get; }
        public abstract bool MegamapDefault { get; }
        public abstract bool MegamapOfficial { get; }
        public abstract bool HasSinglePlayer { get; }
        public abstract int MaxTriggers { get; }
        public abstract int MaxTeams { get; }
        public abstract int HitPointsGreenMinimum { get; }
        public abstract int HitPointsYellowMinimum { get; }
        #endregion

        #region functions
        public abstract IGamePlugin CreatePlugin(bool mapImage, bool megaMap);
        public abstract void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster);
        public abstract string GetClassicOpposingPlayer(string player);
        public abstract bool SupportsMapLayer(MapLayerFlag mlf);
        public abstract Tile GetWaypointIcon();
        public abstract Tile GetCellTriggerIcon();
        public abstract Bitmap GetSelectIcon();
        /// <summary>Checks whether the briefing has any kind of issues concerning length or supported characters.</summary>
        /// <param name="briefing">The briefing to check</param>
        /// <returns>Null if everything is okay, otherwise any issues to show on the user interface.</returns>
        public abstract string EvaluateBriefing(string briefing);
        /// <summary>Checks whether the name has a default map name that is considered empty by this game plugin.</summary>
        /// <param name="name">Map name to check.</param>
        /// <returns>True if the given name is considered empty by this game plugin.</returns>
        public abstract bool MapNameIsEmpty(string name);
        #endregion

        #region protected functions
        protected Tile GetTile(string remasterSprite, int remastericon, string classicSprite, int classicicon)
        {
            Tile tile;
            if (!Globals.UseClassicFiles && Globals.TheTilesetManager.GetTileData(remasterSprite, remastericon, out tile))
            {
                return tile;
            }
            Globals.TheTilesetManager.GetTileData(classicSprite, classicicon, out tile);
            return tile;
        }

        protected Bitmap GetTexture(string remasterTexturePath, string classicSprite, int classicicon)
        {
            if (!Globals.UseClassicFiles && Globals.TheTilesetManager is TilesetManager tsm)
            {
                // The Texture manager returns a clone of its own cached image. The Tileset manager caches those clones again,
                // and is responsible for their cleanup, but if we use the Texture manager directly, it needs to be disposed.
                return tsm.TextureManager.GetTexture(remasterTexturePath, null, false).Item1;
            }
            else if (Globals.UseClassicFiles)
            {
                if (Globals.TheTilesetManager.GetTileData(classicSprite, classicicon, out Tile tile) && tile != null && tile.Image != null)
                {
                    // Clone so it's equivalent to remaster one and can be used in a Using block:
                    return new Bitmap(tile.Image);
                }
            }
            return null;
        }
        #endregion

        #region overrides
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}
