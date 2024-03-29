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
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;

namespace MobiusEditor.Interface
{
    public interface ITilesetManager: IDisposable
    {
        /// <summary>
        /// Resets the ITilesetManager instance to prepare it for a new game. This clears cached objects
        /// and makes any subsequent tile reads search specifically for tiles of this game.
        /// </summary>
        /// <param name="gameType">Game type.</param>
        /// <param name="theater">Theater type.</param>
        void Reset(GameType gameType, TheaterType theater);

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
        /// Gets team color adapted tile data. If <paramref name="tile"/> is null, nothing was found. If it is not null, a
        /// return value of true indicates the tile was found, while a value of false indicates a dummy graphic was fetched.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <param name="shape">Shape number to fetch.</param>
        /// <param name="teamColor">Team color to recolor the graphic with.</param>
        /// <param name="ignoreShadow">Do not account for shadow colour filtering when fetching the sprite.</param>
        /// <param name="tile">Output graphic</param>
        /// <returns>True if the operation succeeded, false if it failed or is returning a dummy graphic.</returns>
        bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, bool ignoreShadow, out Tile tile);

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
        /// Gets the total amount of shapes available for the requested tile, or -1 if no tileset information was found.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <returns>The total amount of shapes available for the requested tile.</returns>
        /// <remarks>For the Remaster, this is based on xml info. In the classic files, this simply looks at the actual sprite file.</remarks>
        int GetTileDataLength(string name);

        /// <summary>
        /// Gets the total amount of shapes available for the requested tile, or -1 if no tileset information was found.
        /// </summary>
        /// <param name="name">Name of the tile to fetch.</param>
        /// <returns>The total amount of shapes available for the requested tile.</returns>
        /// <remarks>For the Remaster, this is based on xml info. In the classic files, this simply looks at the actual sprite file.</remarks>
        bool TileExists(string name);
    }
}
