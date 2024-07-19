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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MobiusEditor.Model;

namespace MobiusEditor.Interface
{
    public interface IArchiveManager : IEnumerable<string>, IEnumerable, IDisposable
    {
        string LoadRoot { get; }

        /// <summary>Gets the currently loaded game type. This can be changed with the <see cref="Reset(GameType, TheaterType)"/> function.</summary>
        GameType CurrentGameType { get; }

        /// <summary>Gets the currently loaded theater. This can be changed with the <see cref="Reset(GameType, TheaterType)"/> function.</summary>
        TheaterType CurrentTheater { get; }

        /// <summary>
        /// Check whether a file exists in the currently loaded files.
        /// </summary>
        /// <param name="path">The internal path of the file to open.</param>
        /// <returns>True if the file exists.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Open a file from the folders or archives as Stream.
        /// </summary>
        /// <param name="path">The internal path of the file to open.</param>
        /// <returns>A stream to read from, or null if the file was not found.</returns>
        Stream OpenFile(string path);

        /// <summary>
        /// Read a file from the folders or archives as bytes.
        /// </summary>
        /// <param name="path">The internal path of the file to open.</param>
        /// <returns>A byte array with the file's contents, or null if the file was not found.</returns>
        Byte[] ReadFile(string path);

        /// <summary>
        /// Check whether a file exists in the currently loaded classic files.
        /// </summary>
        /// <param name="path">The internal path of the file to open.</param>
        /// <returns>True if the file exists.</returns>
        bool ClassicFileExists(string path);

        /// <summary>
        /// Open a file from the classic folders or archives as Stream.
        /// </summary>
        /// <param name="name">The name of the file to open.</param>
        /// <returns>A stream to read from, or null if the file was not found.</returns>
        Stream OpenFileClassic(string name);

        /// <summary>
        /// Read a file from the classic folders or archives as bytes.
        /// </summary>
        /// <param name="name">The name of the file to open.</param>
        /// <returns>A byte array with the file's contents, or null if the file was not found.</returns>
        Byte[] ReadFileClassic(string path);

        /// <summary>
        /// Resets the archive manager for a new context. This may restrict or re-prioritise where certain files are read from.
        /// </summary>
        /// <param name="gameType">Game type for the new context.</param>
        /// <param name="theater">Theater for the new context. If no theater is given, all theater data will be loaded. This is typically only done for the initial test to see if all data can be found.</param>
        void Reset(GameType gameType, TheaterType theater);

    }
}
