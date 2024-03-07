//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// This class reads C&amp;C Remastered Collection strings file, and allows retrieving requested strings by text ID.
    /// Any string IDs that have no match in the Remastered files should be added in the
    /// <see cref="StartupLoader.AddMissingRemasterText(IGameTextManager)"/> function.
    /// </summary>
    public class GameTextManager: IGameTextManager
    {
        private readonly Dictionary<string, string> gameText = new Dictionary<string, string>();

        public string this[string key]
        {
            get => GetString(key) ?? string.Empty;
            set => gameText[key] = value;
        }

        public string GetString(string key)
        {
            return gameText.TryGetValue(key, out string val) ? val : null;
        }

        public void Reset(GameType gameType)
        {
            // Do nothing; the text for both games is read from the same file.
        }

        public void Dump(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                foreach (string key in gameText.Keys.OrderBy(s => s))
                {
                    sw.WriteLine("{0} = {1}", key, gameText[key].Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\\n"));
                }
            }
        }

        public GameTextManager(IArchiveManager archiveManager, string gameTextFile)
        {
            using (Stream stream = archiveManager.OpenFile(gameTextFile))
            using (BinaryReader reader = new BinaryReader(stream))
            using (BinaryReader unicodeReader = new BinaryReader(stream, Encoding.Unicode))
            using (BinaryReader asciiReader = new BinaryReader(stream, Encoding.ASCII))
            {
                uint numStrings = reader.ReadUInt32();
                (uint textSize, uint idSize)[] stringSizes = new (uint, uint)[numStrings];
                string[] strings = new string[numStrings];
                for (int i = 0; i < numStrings; ++i)
                {
                    reader.ReadUInt32();
                    stringSizes[i] = (reader.ReadUInt32(), reader.ReadUInt32());
                }
                for (int i = 0; i < numStrings; ++i)
                {
                    strings[i] = new string(unicodeReader.ReadChars((int)stringSizes[i].textSize));
                }
                for (var i = 0; i < numStrings; ++i)
                {
                    string textId = new string(asciiReader.ReadChars((int)stringSizes[i].idSize));
                    gameText[textId] = strings[i];
                }
            }
        }
    }
}
