using MobiusEditor.Utility.Hashing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace MobiusEditor.Utility
{
    public class Mixfile: IDisposable
    {
        private Dictionary<uint, (uint Offset, uint Length)> mixFileContents = new Dictionary<uint, (uint, uint)>();
        private HashRol1 hashRol = new HashRol1();

        public string MixFileName { get; private set; }
        private MemoryMappedFile mixFileMap;
        private bool isEmbedded = false;
        private long fileStart;
        private long fileLength;
        private long dataStart;

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
            this.ReadMixHeader(mixFileMap, offset, fileLength);
        }

        private void ReadMixHeader(MemoryMappedFile mixMap, long mixStart, long mixLength)
        {
            mixFileContents.Clear();
            uint readOffset = 0;
            ushort nrOfFiles = 0;
            bool hasFlags = false;
            bool encrypted = false;
            bool checksum = false;
            using (BinaryReader headerReader = new BinaryReader(CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
            {
                ushort start = headerReader.ReadUInt16();
                if (start == 0)
                    hasFlags = true;
                else
                    nrOfFiles = start;
                readOffset += 2;
            }
            if (hasFlags)
            {
                using (BinaryReader headerReader = new BinaryReader(CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
                {
                    var flags = headerReader.ReadUInt16();
                    checksum = (flags & 1) != 0;
                    encrypted = (flags & 2) != 0;
                    readOffset += 2;
                }
                // Not encrypted; read nr of files.
                if (!encrypted)
                {
                    using (BinaryReader headerReader = new BinaryReader(CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2)))
                    {
                        nrOfFiles = headerReader.ReadUInt16();
                        readOffset += 2;
                    }
                }
            }
            uint headerSize;
            Byte[] header = null;
            if (encrypted)
            {
                using (BinaryReader headerReader = new BinaryReader(CreateViewStream(mixMap, mixStart, mixLength, readOffset, 80)))
                {
                    byte[] blowfishKey = headerReader.ReadAllBytes();
                    readOffset += 80;
                }
                // The rest of the blowfish decryption
                throw new NotSupportedException("Encrypred mix archives are currently not supported.");
            }
            else
            {
                // Ignore data size in header; it's only used for caching.
                readOffset += 4;
                headerSize = (UInt32)(nrOfFiles * 12);
                if (readOffset + headerSize > mixLength)
                {
                    throw new ArgumentOutOfRangeException("Not a valid mix file: header length exceeds file length.");
                }
                using (BinaryReader headerReader = new BinaryReader(CreateViewStream(mixMap, mixStart, mixLength, readOffset, headerSize)))
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
                for (int i = 0; i < nrOfFiles; ++i)
                {
                    uint fileId = headerReader.ReadUInt32();
                    uint fileOffset = headerReader.ReadUInt32();
                    uint fileLength = headerReader.ReadUInt32();
                    if (fileOffset + fileLength > mixLength)
                    {
                        throw new ArgumentOutOfRangeException(String.Format("Not a valid mix file: file #{0} with id {1:X08} exceeds archive length.", i, fileId));
                    }
                    mixFileContents.Add(fileId, (fileOffset, fileLength));
                }
            }
        }

        public bool GetFileInfo(string filename, out uint offset, out uint length)
        {
            offset = 0;
            length = 0;
            uint fileId = hashRol.GetNameId(filename);
            (uint Offset, uint Length) fileLoc;
            if (!mixFileContents.TryGetValue(fileId, out fileLoc))
            {
                return false;
            }
            offset = fileLoc.Offset;
            length = fileLoc.Length;
            return true;
        }

        public Stream OpenFile(string path)
        {
            uint fileId = hashRol.GetNameId(path);
            (uint Offset, uint Length) fileLoc;
            if (!mixFileContents.TryGetValue(fileId, out fileLoc))
            {
                return null;
            }
            return CreateViewStream(mixFileMap, fileStart, fileLength, this.dataStart + fileLoc.Offset, fileLoc.Length);
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
            if (disposedValue)
            {
                throw new ObjectDisposedException("Mixfile");
            }
            if (dataReadOffset + dataReadLength > mixFileLength)
            {
                throw new IndexOutOfRangeException("Data exceeds mix file bounds.");
            }
            return mixMap.CreateViewStream(mixFileStart + dataReadOffset, dataReadLength, MemoryMappedFileAccess.Read);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Only dispose if not an embedded mix file.
                // If embedded, the mixFileMap is contained in the parent.
                if (disposing && !isEmbedded)
                {
                    mixFileMap.Dispose();
                }
                mixFileMap = null;
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
