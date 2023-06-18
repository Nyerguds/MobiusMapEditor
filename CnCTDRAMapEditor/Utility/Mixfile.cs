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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

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
        {
            FileInfo mixFile = new FileInfo(mixPath);
            this.fileStart = 0;
            this.fileLength = mixFile.Length;
            this.MixFileName = mixPath;
            this.mixFileMap = MemoryMappedFile.CreateFromFile(
                new FileStream(mixPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            this.ReadMixHeader(this.mixFileMap, this.fileStart, this.fileLength);
        }

        public Mixfile(Mixfile container, string name)
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
            this.ReadMixHeader(this.mixFileMap, offset, this.fileLength);
        }

        private void ReadMixHeader(MemoryMappedFile mixMap, long mixStart, long mixLength)
        {
            this.mixFileContents.Clear();
            uint readOffset = 0;
            ushort fileCount = 0;
            uint dataSize = 0;
            bool hasFlags = false;
            bool encrypted = false;
            bool checksum = false;
            byte[] buffer;
            using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
            {
                buffer = headerReader.ReadBytes(2);
                ushort start = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                if (start == 0)
                {
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
                using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
                {
                    buffer = headerReader.ReadBytes(2);
                    ushort flags = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                    checksum = (flags & 1) != 0;
                    encrypted = (flags & 2) != 0;
                    readOffset += 2;
                }
                if (!encrypted)
                {
                    using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
                    {
                        buffer = headerReader.ReadBytes(2);
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
                    throw new ArgumentOutOfRangeException("Not a valid mix file: minimum encrypted header length exceeds file length.");
                }
                header = DecodeHeader(mixMap, mixStart, mixLength, ref readOffset);
            }
            else
            {
                headerSize = 6 + (uint)(fileCount * 12);
                if (readOffset + headerSize > mixLength)
                {
                    throw new ArgumentOutOfRangeException("Not a valid mix file: header length exceeds file length.");
                }
                using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, headerSize)))
                {
                    header = headerReader.ReadBytes((Int32)headerSize);
                    // End of header reading; no longer needed.
                    //readOffset += headerSize;
                }
                readOffset += headerSize;
            }
            // Store so files can correctly be offset to the end of the header
            this.dataStart = readOffset;
            using (BinaryReader headerReader = new BinaryReader(new MemoryStream(header)))
            {
                fileCount = headerReader.ReadUInt16();
                dataSize = headerReader.ReadUInt32();
                for (int i = 0; i < fileCount; ++i)
                {
                    uint fileId = headerReader.ReadUInt32();
                    uint fileOffset = headerReader.ReadUInt32();
                    uint fileLength = headerReader.ReadUInt32();
                    if (fileOffset + fileLength > mixLength)
                    {
                        throw new ArgumentOutOfRangeException(String.Format("Not a valid mix file: file #{0} with id {1:X08} exceeds archive length.", i, fileId));
                    }
                    this.mixFileContents.Add(fileId, (fileOffset, fileLength));
                }
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Byte[] DecodeHeader(MemoryMappedFile mixMap, long mixStart, long mixLength, ref uint readOffset)
        {
            // Based on specs written up by OmniBlade on the Shikadi Modding Wiki.
            // https://moddingwiki.shikadi.net/wiki/MIX_Format_(Westwood)
            // A very special thanks to Morton on the C&C Mod Haven Discord for helping me out with this.

            // DER identifies the block as "02 28": an integer of length 40. So just cut off the first 2 bytes and get the key.
            Byte[] derKeyBytes = Convert.FromBase64String(PublicKey);
            byte[] modulusBytes = new byte[40];
            Array.Copy(derKeyBytes, 2, modulusBytes, 0, modulusBytes.Length);
            // Read blocks
            byte[] readBlock;
            using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 40)))
            {
                readBlock = headerReader.ReadAllBytes();
                readOffset += 40;
            }
            // Read data is little endian. BigInteger uses big endian.
            Array.Reverse(readBlock);
            BigInteger value1 = new BigInteger(readBlock);
            using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 40)))
            {
                readBlock = headerReader.ReadAllBytes();
                readOffset += 40;
            }
            Array.Reverse(readBlock);
            BigInteger value2 = new BigInteger(readBlock);

            // RSA: decryption is x = y^e % n, encryption is y = x^d % n
            // x is plaintext, y is encrypted, n is modulus, e is public exponent, d is private exponent
            BigInteger modulus = new BigInteger(modulusBytes);
            BigInteger exponent = new BigInteger(new Byte[] { 01, 00, 01 });
            BigInteger value1Decr = value1.ModPow(exponent, modulus);
            BigInteger value2Decr = value2.ModPow(exponent, modulus);
            byte[] value1DecrB = value1Decr.ToByteArray();
            byte[] value2DecrB = value2Decr.ToByteArray();
            Array.Reverse(value1DecrB);
            Array.Reverse(value2DecrB);
            // Find offset of any trailing zeroes. Cast to nullable int to see a difference between 0 and default returned from finding nothing.
            int? value1lenN = Enumerable.Range(0, value1DecrB.Length).Reverse().Select(i => (int?)i).FirstOrDefault(i => value1DecrB[i.Value] != 0);
            int? value2lenN = Enumerable.Range(0, value2DecrB.Length).Reverse().Select(i => (int?)i).FirstOrDefault(i => value2DecrB[i.Value] != 0);
            int value1len = value1lenN.HasValue ? value1lenN.Value + 1 : 0;
            int value2len = value2lenN.HasValue ? value2lenN.Value + 1 : 0;
            byte[] blowFishKey = new Byte[value1len + value2len];
            Array.Copy(value1DecrB, 0, blowFishKey, 0, value1len);
            Array.Copy(value2DecrB, 0, blowFishKey, value1len, value2len);
            if (blowFishKey.Length != 56)
            {
                byte[] blowFishKey2 = new byte[56];
                Array.Copy(blowFishKey, 0, blowFishKey2, 0, Math.Min(blowFishKey2.Length, blowFishKey.Length));
                blowFishKey = blowFishKey2;
            }
            IBufferedCipher blowfish = CipherUtilities.GetCipher("Blowfish/ECB/NoPadding");
            blowfish.Init(false, new KeyParameter(blowFishKey));
            Byte[] blowBuffer = new byte[8];
            using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 8)))
            {
                readBlock = headerReader.ReadAllBytes();
                blowfish.ProcessBytes(readBlock, 0, 8, blowBuffer, 0);
            }
            ushort fileCount = ArrayUtils.ReadUInt16FromByteArrayLe(blowBuffer, 0);
            uint headerSize = 6 + (uint)(fileCount * 12);
            uint blocksToRead = (headerSize + 7) / 8;
            uint realHeaderSize = blocksToRead * 8;
            if (readOffset + realHeaderSize > mixLength)
            {
                throw new ArgumentOutOfRangeException("Not a valid mix file: encrypted header length exceeds file length.");
            }
            // Adjust read offset to end of first block
            readOffset += 8;
            blocksToRead--;
            // Don't bother trimming this. It'll read it using the amount of files value anyway.
            byte[] decryptedHeader = new byte[realHeaderSize];
            // Add already-read block.
            Array.Copy(blowBuffer, 0, decryptedHeader, 0, blowBuffer.Length);
            int bfOffsetIn = 0;
            int bfOffsetOut = 8;
            using (BinaryReader headerReader = new BinaryReader(this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, realHeaderSize - 8)))
            {
                readBlock = headerReader.ReadAllBytes();
                for (int i = 0; i < blocksToRead; i++)
                {
                    blowfish.ProcessBytes(readBlock, bfOffsetIn, 8, decryptedHeader, bfOffsetOut);
                    bfOffsetIn += 8;
                    bfOffsetOut += 8;
                    readOffset += 8;
                }
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
