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
using MobiusEditor.Utility.Hashing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Numerics;

namespace MobiusEditor.Utility
{
    public class Mixfile: IDisposable
    {
        private static readonly string PublicKey = "AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V";
        private static readonly string PrivateKey = "AigKVje8mROcR8QixnxUEF5b29Curkq01DNDWCdOG99XBqH79OaCiTCB";

        private Dictionary<uint, (uint Offset, uint Length)> mixFileContents = new Dictionary<uint, (uint, uint)>();
        private HashRol1 hashRol = new HashRol1();

        public string MixFileName { get; private set; }
        private MemoryMappedFile mixFileMap;
        private bool isEmbedded = false;
        private long fileStart;
        private long fileLength;
        private uint dataStart;

        public Mixfile(string mixPath)
            :this(mixPath, true)
        {
        }

        public Mixfile(string mixPath, bool handleAdvanced)
        {
            FileInfo mixFile = new FileInfo(mixPath);
            this.fileStart = 0;
            this.fileLength = mixFile.Length;
            this.MixFileName = mixPath;
            this.mixFileMap = MemoryMappedFile.CreateFromFile(
                new FileStream(mixPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            this.ReadMixHeader(this.mixFileMap, this.fileStart, this.fileLength, handleAdvanced);
        }

        public Mixfile(Mixfile container, string name)
            : this(container, name, true)
        {
        }

        public Mixfile(Mixfile container, string name, bool handleAdvanced)
        {
            this.isEmbedded = true;
            this.MixFileName = container.MixFileName + " -> " + name;
            if (!container.GetFileInfo(name, out uint offset, out uint length))
            {
                throw new FileNotFoundException(name + " was not found inside this mix archive.");
            }
            this.fileStart = offset;
            this.fileLength = length;
            // Copy reference to parent map. The "CreateViewStream" function takes care of reading the right parts from it.
            this.mixFileMap = container.mixFileMap;
            this.ReadMixHeader(this.mixFileMap, offset, this.fileLength, handleAdvanced);
        }

        private void ReadMixHeader(MemoryMappedFile mixMap, long mixStart, long mixLength, bool handleAdvanced)
        {
            this.mixFileContents.Clear();
            uint readOffset = 0;
            ushort fileCount = 0;
            // uint dataSize = 0;
            bool hasFlags = false;
            bool encrypted = false;
            //bool checksum = false;
            byte[] buffer;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2))
            {
                buffer = headerStream.ReadAllBytes();
                ushort start = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                if (start == 0)
                {
                    if (!handleAdvanced)
                    {
                        throw new ArgumentException("mixMap", "This mix file can't be one with extended header format.");

                    }
                    hasFlags = true;
                    readOffset += 2;
                }
                else
                {
                    fileCount = start;
                    // Don't increase read offset when reading file count, to keep it synchronised for the encrypted stuff.
                }
            }
            if (hasFlags)
            {
                using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2))
                {
                    buffer = headerStream.ReadAllBytes();
                    ushort flags = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                    //checksum = (flags & 1) != 0;
                    encrypted = (flags & 2) != 0;
                    readOffset += 2;
                }
                if (!encrypted)
                {
                    using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2))
                    {
                        buffer = headerStream.ReadAllBytes();
                        fileCount = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                        // Don't increase read offset when reading file count, to keep it synchronised for the encrypted stuff.
                    }
                }
            }
            uint headerSize;
            Byte[] header = null;
            if (encrypted)
            {
                if (readOffset + 88 > mixLength)
                {
                    throw new ArgumentException("mixMap", "Not a valid mix file: minimum encrypted header length exceeds file length.");
                }
                header = DecodeHeader(mixMap, mixStart, mixLength, ref readOffset);
            }
            else
            {
                headerSize = 6 + (uint)(fileCount * 12);
                if (readOffset + headerSize > mixLength)
                {
                    throw new ArgumentException("mixMap", "Not a valid mix file: header length exceeds file length.");
                }
                using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, headerSize))
                {
                    header = headerStream.ReadAllBytes();
                    // End of header reading; no longer needed.
                    //readOffset += headerSize;
                }
                readOffset += headerSize;
            }
            // Store so files can correctly be offset to the end of the header
            this.dataStart = readOffset;

            int hdrPtr = 0;
            fileCount = ArrayUtils.ReadUInt16FromByteArrayLe(header, hdrPtr);
            //dataSize = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr + 2);
            // Skip to file table
            hdrPtr += 6;
            for (int i = 0; i < fileCount; ++i)
            {
                uint fileId = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr);
                uint fileOffset = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr + 4);
                uint fileLength = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr + 8);
                hdrPtr += 12;
                if (fileOffset + fileLength > mixLength)
                {
                    throw new ArgumentException("mixMap", String.Format("Not a valid mix file: file #{0} with id {1:X08} exceeds archive length.", i, fileId));
                }
                this.mixFileContents.Add(fileId, (fileOffset, fileLength));
            }
        }

        public bool GetFileInfo(string filename, out uint offset, out uint length)
        {
            offset = 0;
            length = 0;
            uint fileId = this.hashRol.GetNameId(filename);
            (uint Offset, uint Length) fileLoc;
            if (!this.mixFileContents.TryGetValue(fileId, out fileLoc))
            {
                return false;
            }
            offset = fileLoc.Offset + this.dataStart;
            length = fileLoc.Length;
            return true;
        }

        public Stream OpenFile(string path)
        {
            if (!this.GetFileInfo(path, out uint offset, out uint length))
            {
                return null;
            }
            return this.CreateViewStream(this.mixFileMap, this.fileStart, this.fileLength, offset, length);
        }

        /// <summary>
        /// Creates a view stream on the current mix file, or on the current embedded mix file.
        /// </summary>
        /// <param name="mixMap">The MemoryMappedFile of the mix file or parent mix file</param>
        /// <param name="mixFileStart">Start of the current mix file inside the mixMap</param>
        /// <param name="mixFileLength">End of the current mix file inside the mixMap</param>
        /// <param name="dataReadOffset">Read position inside the current mix file.</param>
        /// <param name="dataReadLength">Length of the data to read.</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">The data is not in the bounds of this mix file.</exception>
        private Stream CreateViewStream(MemoryMappedFile mixMap, long mixFileStart, long mixFileLength, long dataReadOffset, uint dataReadLength)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException("Mixfile");
            }
            if (dataReadOffset + dataReadLength > mixFileLength)
            {
                // Can normally never happen; it is checked when the header is read.
                throw new IndexOutOfRangeException("Data exceeds mix file bounds.");
            }
            return mixMap.CreateViewStream(mixFileStart + dataReadOffset, dataReadLength, MemoryMappedFileAccess.Read);
        }

        /// <summary>
        /// Decrypts the header.
        /// </summary>
        /// <param name="mixMap">Memory map of the mix file</param>
        /// <param name="mixStart">Start of the mix file data</param>
        /// <param name="mixLength">Length of the mix file data</param>
        /// <param name="readOffset">Contains the start of the encrypted block inside the mix data. At the end of the process,
        /// it contains the end of the whole encrypted header, so it can be used as origin point for the actual data addresses.</param>
        /// <returns>
        ///     the entire decrypted header, including the 6 bytes with the amount of files and the data length at the start.
        ///     It might contain padding at the end as result of the decryption.
        /// </returns>
        private Byte[] DecodeHeader(MemoryMappedFile mixMap, long mixStart, long mixLength, ref uint readOffset)
        {
            // Based on specs written up by OmniBlade on the Shikadi Modding Wiki.
            // https://moddingwiki.shikadi.net/wiki/MIX_Format_(Westwood)
            // A very special thanks to Morton on the C&C Mod Haven Discord for helping me out with this.

            // DER identifies the block as "02 28": an integer of length 40. So just cut off the first 2 bytes and get the key.
            Byte[] derKeyBytes = Convert.FromBase64String(PublicKey);
            byte[] modulusBytes = new byte[40];
            Array.Copy(derKeyBytes, 2, modulusBytes, 0, modulusBytes.Length);
            Array.Reverse(modulusBytes);
            BigInteger publicModulus = new BigInteger(modulusBytes);
            BigInteger publicExponent = new BigInteger(65537);
            // Read blocks
            byte[] readBlock;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 80))
            {
                readBlock = headerStream.ReadAllBytes();
                readOffset += 80;
            }
            byte[] blowFishKey = MixFileCrypto.DecryptBlowfishKey(readBlock, publicModulus, publicExponent);
            Byte[] blowBuffer = new byte[BlowfishStream.SIZE_OF_BLOCK];
            long remaining = mixLength - readOffset;
            byte[] decryptedHeader;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, (uint)remaining))
            using (BlowfishStream bfStream = new BlowfishStream(headerStream, blowFishKey))
            {
                bfStream.Read(blowBuffer, 0, BlowfishStream.SIZE_OF_BLOCK);

                ushort fileCount = ArrayUtils.ReadUInt16FromByteArrayLe(blowBuffer, 0);
                uint headerSize = 6 + (uint)(fileCount * 12);
                uint blocksToRead = (headerSize + BlowfishStream.SIZE_OF_BLOCK - 1) / BlowfishStream.SIZE_OF_BLOCK;
                uint realHeaderSize = blocksToRead * BlowfishStream.SIZE_OF_BLOCK;
                if (readOffset + realHeaderSize > mixLength)
                {
                    throw new ArgumentException("mixMap", "Not a valid mix file: encrypted header length exceeds file length.");
                }
                // Don't bother trimming this. It'll read it using the amount of files value anyway.
                decryptedHeader = new byte[realHeaderSize];
                // Add already-read block.
                Array.Copy(blowBuffer, 0, decryptedHeader, 0, BlowfishStream.SIZE_OF_BLOCK);
                readOffset += realHeaderSize;
                bfStream.Read(decryptedHeader, BlowfishStream.SIZE_OF_BLOCK, (int)(realHeaderSize - BlowfishStream.SIZE_OF_BLOCK));
            }
            return decryptedHeader;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                // Only dispose if not an embedded mix file.
                // If embedded, the mixFileMap is contained in the parent.
                if (disposing && !this.isEmbedded)
                {
                    this.mixFileMap.Dispose();
                }
                this.mixFileMap = null;
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion
    }
}
