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
using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColorManager
    {
        /// <summary>The color used as basis to do recoloring operation on to get a single basic identifying color out of an ITeamColor object.</summary>
        Color BaseColorSource { get; }

        /// <summary>Retrieves the ITeamColor object for the given key, or null if no object was found for the given key.</summary>
        /// <param name="key">Color key</param>
        /// <returns>The ITeamColor object for the given key, or null if no object was found for the given key.</returns>
        ITeamColor this[string key] { get; }

        /// <summary>Gets a general color representing this team color.</summary>
        /// <param name="key">Color key</param>
        /// <returns>The basic color for this team color.</returns>
        Color GetBaseColor(string key);

        /// <summary>Load colors from the given filename. This file will be looked up using the embeded archive manager.</summary>
        /// <param name="path">Path of the file to load.</param>
        void Load(string path);

        /// <summary>Reloads assets for the given game type and theater.</summary>
        /// <param name="gameType">The game type</param>
        /// <param name="theater">The theater</param>
        void Reset(GameType gameType, TheaterType theater);
    }

    public interface ITeamColorManager<T> where T : ITeamColor
    {
        T GetItem(string key);
        void AddTeamColor(T col);
        void RemoveTeamColor(T col);
        void RemoveTeamColor(string col);
    }
}
