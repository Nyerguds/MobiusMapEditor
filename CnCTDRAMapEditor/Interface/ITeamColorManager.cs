﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
        Color RemapBaseColor { get; }
        ITeamColor this[string key] { get; }

        /// <summary>Gets a general color representing this team color.</summary>
        /// <param name="key">Color key</param>
        /// <returns>The basic color for this team color.</returns>
        Color GetBaseColor(string key);

        void Load(string path);
        void Reset(GameType gameType, TheaterType theater);
    }

    public interface ITeamColorManager<T> where T : ITeamColor
    {
        T GetItem(string key);
        void RemoveTeamColor(string col);
        void AddTeamColor(T col);
        void RemoveTeamColor(T col);
    }
}
