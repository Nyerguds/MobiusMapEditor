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
using System;

namespace MobiusEditor.Interface
{
    public interface IGameTextManager
    {
        /// <summary>
        /// Action to execute after a reset to adjust the read strings file to missing strings.
        /// </summary>
        Action<IGameTextManager, GameType> AddMissing { get; set; }

        /// <summary>
        /// Gets or overrides the value for the given key in the strings file.
        /// </summary>
        /// <param name="key">string key of the text to get or set.</param>
        /// <returns>The found text, or an empty string if not found.</returns>
        string this[string key] { get; set; }

        /// <summary>
        /// Gets the value for the given key in the strings file, or null if it wasn't found.
        /// </summary>
        /// <param name="key">string key of the text to get.</param>
        /// <returns>The found text, or null if not found.</returns>
        string GetString(string key);

        /// <summary>
        /// Reset the game text manager for the given game type. This requires that the archive manager
        /// is already reset for this game type, so the game text manager can read the correct files.
        /// </summary>
        /// <param name="gameType">The game type to initialise the game text manager for.</param>
        void Reset(GameType gameType);
    }
}
