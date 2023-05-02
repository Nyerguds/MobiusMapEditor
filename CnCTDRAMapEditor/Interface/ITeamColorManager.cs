//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColorManager
    {
        string[] ExpandModPaths { get; set; }
        Color RemapBaseColor { get; }
        ITeamColor this[string key] { get; }

        void Load(string path);
        void Reset(GameType gameType);
    }
}