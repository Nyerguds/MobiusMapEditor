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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MobiusEditor.Utility
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct SubFileData
    {
        public ushort Flags;
        public uint CRCValue;
        public int SubfileIndex;
        public uint SubfileSize;
        public uint SubfileImageDataOffset;
        public ushort SubfileNameIndex;

        public static readonly uint Size = (uint)Marshal.SizeOf(typeof(SubFileData));
    }

    public class Megafile : IEnumerable<string>, IEnumerable, IDisposable
    {
        public string MegaFileName { get; private set; }
        private readonly MemoryMappedFile megafileMap;
        private readonly string[] stringTable;
        private readonly Dictionary<string, SubFileData> fileTable = new Dictionary<string, SubFileData>();

        public Megafile(string megafilePath)
        {
            MegaFileName = megafilePath;
            megafileMap = MemoryMappedFile.CreateFromFile(
                new FileStream(megafilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);

            var numFiles = 0U;
            var numStrings = 0U;
            var stringTableSize = 0U;
            var fileTableSize = 0U;

            var readOffset = 0U;
            using (Stream stream = CreateReadOnlyStream(megafileMap, 4, stringTableSize))
            using (var magicNumberReader = new BinaryReader(stream))
            {
                var magicNumber = magicNumberReader.ReadUInt32();
                if ((magicNumber == 0xFFFFFFFF) || (magicNumber == 0x8FFFFFFF))
                {
                    // Skip header size and version
                    readOffset += 8;
                }
            }
            readOffset += 4U;
            using (Stream stream = CreateReadOnlyStream(megafileMap, 12, stringTableSize))
            using (BinaryReader headerReader = new BinaryReader(stream))
            {
                numFiles = headerReader.ReadUInt32();
                numStrings = headerReader.ReadUInt32();
                stringTableSize = headerReader.ReadUInt32();
                fileTableSize = numFiles * SubFileData.Size;
            }
            readOffset += 12U;
            using (Stream stream = CreateReadOnlyStream(megafileMap, readOffset, stringTableSize))
            using (BinaryReader stringReader = new BinaryReader(stream))
            {
                stringTable = new string[numStrings];
                for (var i = 0U; i < numStrings; ++i)
                {
                    var stringSize = stringReader.ReadUInt16();
                    stringTable[i] = new string(stringReader.ReadChars(stringSize));
                }
            }
            readOffset += stringTableSize;
            if (fileTableSize > 0)
            {
                using (var subFileAccessor = megafileMap.CreateViewAccessor(readOffset, fileTableSize, MemoryMappedFileAccess.Read))
                {
                    for (var i = 0U; i < numFiles; ++i)
                    {
                        subFileAccessor.Read(i * SubFileData.Size, out SubFileData subFile);
                        var fullName = stringTable[subFile.SubfileNameIndex];
                        fileTable[fullName] = subFile;
                    }
                }
            }
        }

        public Stream OpenFile(string path)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("MegaFile");
            }
            if (!fileTable.TryGetValue(path, out SubFileData subFile))
            {
                return null;
            }
            return CreateReadOnlyStream(megafileMap, subFile.SubfileImageDataOffset, subFile.SubfileSize);
        }

        /// <summary>
        /// Creates a view stream on the current Mega file at the requested read offset, with the requested length.
        /// </summary>
        /// <param name="megafileMap">The MemoryMappedFile of the Mega file.</param>
        /// <param name="dataReadOffset">Read position inside the Mega file.</param>
        /// <param name="dataReadLength">Length of the data to read.</param>
        /// <returns>A Stream containing the contents of the requested section of the MemoryMappedFile, or an empty MemoryStream if <paramref name="dataReadLength"/> is 0.</returns>
        private Stream CreateReadOnlyStream(MemoryMappedFile megafileMap, uint dataReadOffset, uint dataReadLength)
        {
            if (dataReadLength == 0)
            {
                // If the given size is 0, CreateViewStream creates a stream with the data full length.
                // We don't want that, so we return an empty memorystream instead.
                return new MemoryStream(new byte[0]);
            }
            return megafileMap.CreateViewStream(dataReadOffset, dataReadLength, MemoryMappedFileAccess.Read);
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("MegaFile");
            }
            foreach (var file in stringTable)
            {
                yield return file;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    megafileMap.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
