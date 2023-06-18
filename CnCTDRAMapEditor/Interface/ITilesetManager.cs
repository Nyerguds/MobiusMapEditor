using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Interface
{
    public interface ITilesetManager
    {
        /// <summary>
        /// Resets the ITilesetManager instance to prepare it for a new game. This clears cached objects
        /// and makes any subsequent tile reads search specifically for tiles of this game.
        /// </summary>
        /// <param name="theater"></param>
        void Reset(TheaterType theater);

        /// <summary>
        /// Gets team color adapted tile data. If <paramref name="tile"/> is null, nothing was found. If it is not null, a
        /// return value of true indicates the tile was found, while a value of false indicates a dummy graphic was fetched.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <param name="shape">Shape number to fetch.</param>
        /// <param name="teamColor">Team color to recolor the graphic with.</param>
        /// <param name="tile">Output graphic</param>
        /// <param name="generateFallback">True to generate a fallback graphic if nothing was found.</param>
        /// <param name="onlyIfDefined">True to only generate a fallback in case the tile is defined in the tileset manager.</param>
        /// <returns>True if the operation succeeded, false if it failed or is returning a dummy graphic.</returns>
        bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined);

        /// <summary>
        /// Gets team color adapted tile data. If <paramref name="tile"/> is null, nothing was found. If it is not null, a
        /// return value of true indicates the tile was found, while a value of false indicates a dummy graphic was fetched.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <param name="shape">Shape number to fetch.</param>
        /// <param name="teamColor">Team color to recolor the graphic with.</param>
        /// <param name="tile">Output graphic</param>
        /// <returns>True if the operation succeeded, false if it failed or is returning a dummy graphic.</returns>
        bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile);

        /// <summary>
        /// Gets tile data. If <paramref name="tile"/> is null, nothing was found. If it is not null, a
        /// return value of true indicates the tile was found, while a value of false indicates a dummy graphic was fetched.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <param name="shape">Shape number to fetch.</param>
        /// <param name="tile">Output graphic</param>
        /// <param name="generateFallback">True to generate a fallback graphic if nothing was found.</param>
        /// <param name="onlyIfDefined">True to only generate a fallback in case the tile is defined in the tileset manager.</param>
        /// <returns>True if the operation succeeded, false if it failed or is returning a dummy graphic.</returns>
        bool GetTileData(string name, int shape, out Tile tile, bool generateFallback, bool onlyIfDefined);

        /// <summary>
        /// Gets tile data. If <paramref name="tile"/> is null, nothing was found. If it is not null, a
        /// return value of true indicates the tile was found, while a value of false indicates a dummy graphic was fetched.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <param name="shape">Shape number to fetch.</param>
        /// <param name="tile">Output graphic</param>
        /// <returns>True if the operation succeeded, false if it failed or is returning a dummy graphic.</returns>
        bool GetTileData(string name, int shape, out Tile tile);

        /// <summary>
        /// Gets the total amount of shapes available for the requested tile.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <returns>The total amount of shapes available for the requested tile.</returns>
        int GetTileDataLength(string name);
    }
}
