//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

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
}