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
    public class MixParseException : Exception
    {
        protected readonly string AffectedEntryIdTag = "MixParseExceptionAffectedEntryId";

        public MixParseException()
        {
            this.AffectedEntryId = 0;
        }

        public MixParseException(string message)
            :base(message)
        {
            this.AffectedEntryId = 0;
        }

        public MixParseException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.AffectedEntryId = 0;
        }

        public MixParseException(uint id)
        {
            this.AffectedEntryId = id;
        }

        public MixParseException(string message, uint id)
            : base(message)
        {
            this.AffectedEntryId = id;
        }

        public MixParseException(string message, Exception innerException, uint id)
            : base(message, innerException)
        {
            this.AffectedEntryId = id;
        }

        public uint AffectedEntryId
        {
            get { return (uint)this.Data[this.AffectedEntryIdTag]; }
            set { this.Data[this.AffectedEntryIdTag] = value; }
        }

    }

    public class MixFile : IDisposable
    {
        private static readonly string PublicKey = "AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V";
        private static readonly string PrivateKey = "AigKVje8mROcR8QixnxUEF5b29Curkq01DNDWCdOG99XBqH79OaCiTCB";
        private static readonly int Exponent = 65537;

        private Dictionary<uint, MixEntry[]> mixFileContents = new Dictionary<uint, MixEntry[]>();
        private List<uint> headerIds = new List<uint>();
        private HashMethod hasher = HashMethod.GetRegisteredMethods().FirstOrDefault();

        /// <summary>Path the file was loaded from. For embedded mix files, this will be the original path with the deeper opened mix file(s) indicated behind " → ".</summary>
        public string FilePath { get; private set; }
        /// <summary>Filename to display. Will be null if it is loaded by id from inside another mix archive and its name is not known.</summary>
        public string FileName { get; private set; }
        /// <summary>File ID in case <see href="FileName"/> is not available.</summary>
        public uint FileId { get; private set; }
        public int FileCount { get; private set; }
        public bool IsNewFormat { get; private set; }
        public bool IsEmbedded => parent != null;
        public bool HasEncryption { get; private set; }
        public bool HasChecksum { get; private set; }
        public IEnumerable<uint> FileIds => this.mixFileContents.Keys.OrderBy(k => k);
        public List<uint> HeaderIds => headerIds.ToList();

        /// <summary>Hasher to use.</summary>
        public HashMethod Hasher
        {
            get { return hasher; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.hasher = value;
            }
        }

        private long fileStart;
        private long fileLength;
        private MemoryMappedFile mixFileMap;
        private MixFile parent;

        /// <summary>
        /// Does the same parsing as a normal mix file constructor, but without throwing exceptions.
        /// Mostly added because try-catch on the normal constructor when debugging slows Visual Studio to
        /// a crawl because it insists on showing all exceptions in the debug feed, even if they're caught.
        /// </summary>
        public static bool CheckValidMix(string mixPath, bool handleAdvanced)
        {
            try
            {
                if (!File.Exists(mixPath))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            MixFile dummy = new MixFile();
            FileInfo mixFile = new FileInfo(mixPath);
            dummy.fileStart = 0;
            dummy.fileLength = mixFile.Length;
            dummy.FilePath = mixPath;
            dummy.FileName = Path.GetFileName(mixPath);
            dummy.FileId = 0;
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(
                    new FileStream(mixPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                    null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false))
                {
                    dummy.mixFileMap = mmf;
                    return dummy.ReadMixHeader(mmf, dummy.fileStart, dummy.fileLength, handleAdvanced, false);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Does the same parsing as a normal mix file constructor, but without throwing exceptions.
        /// Mostly added because try-catch on the normal constructor when debugging slows Visual Studio to
        /// a crawl because it insists on showing all exceptions in the debug feed, even if they're caught.
        /// </summary>
        public static bool CheckValidMix(MixFile container, MixEntry entry, bool handleAdvanced)
        {
            MixFile dummy = new MixFile();
            dummy.parent = container;
            string name = entry.Name ?? entry.IdString;
            MixEntry actualEntry = container.VerifyInternal(entry);
            if (actualEntry == null)
            {
                return false;
            }
            dummy.FilePath = container.FilePath + " → " + name;
            dummy.FileName = entry.Name;
            dummy.FileId = entry.Id;
            dummy.fileStart = actualEntry.Offset;
            dummy.fileLength = actualEntry.Length;
            // Copy reference to parent map. The "CreateViewStream" function takes care of reading the right parts from it.
            dummy.mixFileMap = container.mixFileMap;
            try
            {
                return dummy.ReadMixHeader(dummy.mixFileMap, actualEntry.Offset, dummy.fileLength, handleAdvanced, false);
            }
            catch
            {
                return false;
            }
        }

        private MixFile()
        { }

        public MixFile(string mixPath)
            : this(mixPath, true)
        {
        }

        public MixFile(string mixPath, bool handleAdvanced)
        {
            FileInfo mixFile = new FileInfo(mixPath);
            this.fileStart = 0;
            this.fileLength = mixFile.Length;
            this.FilePath = mixPath;
            this.FileName = Path.GetFileName(mixPath);
            // Technically relies on the parent mix file, not the mix file's own hasher, so ignore it here.
            this.FileId = 0;
            this.mixFileMap = MemoryMappedFile.CreateFromFile(
                new FileStream(mixPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            this.ReadMixHeader(this.mixFileMap, this.fileStart, this.fileLength, handleAdvanced, true);
        }

        public MixFile(MixFile container, string name)
            : this(container, new MixEntry(name), true)
        {
        }

        public MixFile(MixFile container, string name, bool handleAdvanced)
            : this(container, new MixEntry(name), handleAdvanced)
        {
        }

        public MixFile(MixFile container, uint nameId)
            : this(container, new MixEntry(nameId, 0, 0, 0), true)
        {
        }

        public MixFile(MixFile container, uint nameId, bool handleAdvanced)
            : this(container, new MixEntry(nameId, 0, 0, 0), handleAdvanced)
        {
        }

        public MixFile(MixFile container, MixEntry entry)
            : this(container, entry, true)
        {
        }

        public MixFile(MixFile container, MixEntry entry, bool handleAdvanced)
        {
            this.parent = container;
            string name = entry.Name ?? entry.IdString;
            MixEntry actualEntry = container.VerifyInternal(entry);
            if (actualEntry == null)
            {
                throw new FileNotFoundException(name + " was not found inside this mix archive.");
            }
            this.FilePath = container.FilePath + " → " + name;
            this.FileName = entry.Name;
            this.FileId = entry.Id;
            this.fileStart = actualEntry.Offset;
            this.fileLength = actualEntry.Length;
            // Copy reference to parent map. The "CreateViewStream" function takes care of reading the right parts from it.
            this.mixFileMap = container.mixFileMap;
            this.ReadMixHeader(this.mixFileMap, actualEntry.Offset, this.fileLength, handleAdvanced, true);
        }

        public uint GetFileId(string filename)
        {
            return hasher.GetNameId(filename);
        }

        private bool GetFileInfo(string filename, out uint offset, out uint length)
        {
            uint fileId = GetFileId(filename);
            return GetFileInfo(fileId, out offset, out length);
        }

        private bool GetFileInfo(uint fileId, out uint offset, out uint length)
        {
            offset = 0;
            length = 0;
            MixEntry[] fileInfo;
            if (!this.mixFileContents.TryGetValue(fileId, out fileInfo) || fileInfo.Length == 0)
            {
                return false;
            }
            offset = fileInfo[0].Offset;
            length = fileInfo[0].Length;
            return true;
        }

        public MixEntry[] GetFullFileInfo(uint fileId)
        {
            MixEntry[] fileInfo;
            if (!this.mixFileContents.TryGetValue(fileId, out fileInfo) || fileInfo.Length == 0)
            {
                return null;
            }
            MixEntry[] returnInfo = new MixEntry[fileInfo.Length];
            for (int i = 0; i < fileInfo.Length; ++i)
            {
                MixEntry entry = fileInfo[i];
                returnInfo[i] = new MixEntry(entry);
            }
            return returnInfo;
        }

        public MixEntry[] GetFullFileInfo(string filename)
        {
            return GetFullFileInfo(GetFileId(filename));
        }

        public byte[] ReadFile(string filename)
        {
            using (Stream file = OpenFile(filename))
            {
                return file?.ReadAllBytes();
            }
        }

        public byte[] ReadFile(uint fileId)
        {
            using (Stream file = OpenFile(fileId))
            {
                return file?.ReadAllBytes();
            }
        }

        public byte[] ReadFile(MixEntry fileInfo)
        {
            using (Stream file = OpenFile(fileInfo))
            {
                return file?.ReadAllBytes();
            }
        }

        public Stream OpenFile(string filename)
        {
            if (!this.GetFileInfo(filename, out uint offset, out uint length))
            {
                return null;
            }
            return this.CreateViewStream(this.mixFileMap, this.fileStart, this.fileLength, offset, length, true);
        }

        public Stream OpenFile(uint fileId)
        {
            if (!this.GetFileInfo(fileId, out uint offset, out uint length))
            {
                return null;
            }
            return this.CreateViewStream(this.mixFileMap, this.fileStart, this.fileLength, offset, length, true);
        }

        public Stream OpenFile(MixEntry fileInfo)
        {
            MixEntry requestedInfo = VerifyInternal(fileInfo);
            if (requestedInfo == null)
            {
                return null;
            }
            return this.CreateViewStream(this.mixFileMap, this.fileStart, this.fileLength, requestedInfo.Offset, requestedInfo.Length, true);
        }

        public int Identify(IEnumerable<MixEntry> info, bool deep, out int total)
        {
            int identified = 0;
            total = 0;
            HashSet<uint> availableNames = info.Where(i => !String.IsNullOrEmpty(i.Name)).Select(i => i.Id).ToHashSet();
            foreach (uint id in this.FileIds)
            {
                total++;
                if (availableNames.Contains(id))
                {
                    identified++;
                }
                if (deep)
                {
                    // Loops over duplicate file entries too.
                    foreach (MixEntry entry in this.mixFileContents[id])
                    {
                        if (!MixFile.CheckValidMix(this, entry, true))
                        {
                            continue;
                        }
                        try
                        {
                            using (MixFile mf = new MixFile(this, entry))
                            {
                                foreach (uint intId in mf.FileIds)
                                {
                                    // Not going more than one layer deep, so no recursion.
                                    total++;
                                    if (availableNames.Contains(id))
                                    {
                                        identified++;
                                    }
                                }
                            }
                        }
                        catch (MixParseException) { /* ignore; is not a mix file */ }
                    }
                }
            }
            return identified;
        }

        public void InsertInfo(Dictionary<uint, MixEntry> info, HashMethod hasher, bool overwrite)
        {
            // This method assumes the file was successfully identified as a specific game type,
            // so we need to set the hasher to the one from the identified game.
            this.Hasher = hasher;
            foreach (KeyValuePair<uint, MixEntry[]> contentItem in mixFileContents)
            {
                if (!info.TryGetValue(contentItem.Key, out MixEntry infoEntry))
                {
                    continue;
                }
                foreach (MixEntry entry in contentItem.Value)
                {
                    // In some cases (like the RAMIX info file) there is no name, only an id.
                    bool nameMatches = String.IsNullOrEmpty(infoEntry.Name) || entry.Id == hasher.GetNameId(infoEntry.Name);
                    // Only overwrite if input is filled in, and name id for the new name indeed matches on the given hasher.
                    if (nameMatches && (overwrite || !String.IsNullOrEmpty(entry.Name)))
                    {
                        entry.Name = infoEntry.Name;
                    }
                    // Description is linked to name; don't allow setting it if the name did not match.
                    if (nameMatches && !String.IsNullOrEmpty(infoEntry.Description) && (overwrite || !String.IsNullOrEmpty(entry.Description)))
                    {
                        entry.Description = infoEntry.Description;
                    }
                    // Info is analysis info of the actual file content. Set this even if the name didn't match.
                    if (!String.IsNullOrEmpty(infoEntry.Info) && (overwrite || !String.IsNullOrEmpty(entry.Info)))
                    {
                        entry.Info = infoEntry.Info;
                    }
                }
            }
        }

        /// <summary>
        /// Matches a given MixEntry to find the corresponding internal entry, first by id, then by starting offset.
        /// This ensures the data is valid. If the starting offset is 0, the first found entry for that id is used.
        /// </summary>
        /// <param name="entry">The entry to retrieve the internal equivalent of.</param>
        /// <returns>The found internal entry.</returns>
        private MixEntry VerifyInternal(MixEntry entry)
        {
            MixEntry[] fileInfos = this.GetFullFileInfo(entry.Id);
            if (fileInfos == null || fileInfos.Length == 0)
            {
                return null;
            }
            if (entry.Offset == 0)
            {
                //Dummy entry. Return first.
                return fileInfos[0];
            }
            MixEntry requestedInfo = fileInfos.FirstOrDefault(fi => fi.Offset == entry.Offset);
            if (requestedInfo == null)
            {
                return fileInfos[0];
            }
            return requestedInfo;
        }

        /// <summary>
        /// Read the mix file header and read the file index from it.
        /// </summary>
        /// <param name="mixMap">The <see cref="MemoryMappedFile"/> to read from.</param>
        /// <param name="mixStart">Start of the current mix file's data inside the <paramref name="mixMap"/></param>
        /// <param name="mixLength">Length of the mix file.</param>
        /// <param name="handleAdvanced"><see langword="true"/> to handle the advanced mix file type capable of handling encryption and checksums.</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>true if the mix file header was parsed successfully.</returns>
        /// <exception cref="MixParseException"></exception>
        private bool ReadMixHeader(MemoryMappedFile mixMap, long mixStart, long mixLength, bool handleAdvanced, bool throwWhenParsing)
        {
            this.mixFileContents.Clear();
            uint readOffset = 0;
            this.FileCount = 0;
            // uint dataSize = 0;
            this.IsNewFormat = false;
            this.HasEncryption = false;
            this.HasChecksum = false;
            byte[] buffer;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2, throwWhenParsing))
            {
                if (headerStream == null && !throwWhenParsing)
                {
                    return false;
                }
                buffer = headerStream.ReadAllBytes();
                ushort start = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                if (start == 0)
                {
                    if (!handleAdvanced)
                    {
                        if (!throwWhenParsing)
                        {
                            return false;
                        }
                        throw new MixParseException("This mix file can't be of the type with extended header format.");
                    }
                    IsNewFormat = true;
                    readOffset += 2;
                }
                else
                {
                    FileCount = start;
                    // Don't increase read offset when reading file count, to keep it synchronised for the encrypted stuff.
                }
            }
            if (IsNewFormat)
            {
                using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2, throwWhenParsing))
                {
                    if (headerStream == null && !throwWhenParsing)
                    {
                        return false;
                    }
                    buffer = headerStream.ReadAllBytes();
                    ushort flags = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                    this.HasChecksum = (flags & 1) != 0;
                    this.HasEncryption = (flags & 2) != 0;
                    readOffset += 2;
                }
                if (!this.HasEncryption)
                {
                    using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 2, throwWhenParsing))
                    {
                        if (headerStream == null && !throwWhenParsing)
                        {
                            return false;
                        }
                        buffer = headerStream.ReadAllBytes();
                        FileCount = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                        // Don't increase read offset when reading file count, to keep it synchronised for the encrypted stuff.
                    }
                }
            }
            uint headerSize;
            byte[] header = null;
            if (this.HasEncryption)
            {
                if (readOffset + 88 > mixLength)
                {
                    if (!throwWhenParsing)
                    {
                        return false;
                    }
                    throw new MixParseException("Not a valid mix file: minimum encrypted header length exceeds file length.");
                }
                header = DecodeHeader(mixMap, mixStart, mixLength, ref readOffset, throwWhenParsing);
                if (!throwWhenParsing && header == null)
                {
                    return false;
                }
                FileCount = ArrayUtils.ReadUInt16FromByteArrayLe(header, 0);
            }
            else
            {
                headerSize = 6 + (uint)(FileCount * 12);
                if (readOffset + headerSize > mixLength)
                {
                    if (!throwWhenParsing)
                    {
                        return false;
                    }
                    throw new MixParseException("Not a valid mix file: header length exceeds file length.");
                }
                using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, headerSize, throwWhenParsing))
                {
                    if (headerStream == null && !throwWhenParsing)
                    {
                        return false;
                    }
                    header = headerStream.ReadAllBytes();
                }
                readOffset += headerSize;
            }
            // To adjust the relative offsets to the end of the header.
            uint dataStart = readOffset;
            // Skip to file table
            int hdrPtr = 6;
            for (int i = 0; i < FileCount; ++i)
            {
                uint fileId = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr);
                uint fileOffset = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr + 4) + dataStart;
                uint fileLength = ArrayUtils.ReadUInt32FromByteArrayLe(header, hdrPtr + 8) & 0x7FFFFFFF;
                hdrPtr += 12;
                if (fileOffset + fileLength > mixLength)
                {
                    if (!throwWhenParsing)
                    {
                        return false;
                    }
                    throw new MixParseException(String.Format("Not a valid mix file: file #{0} with id {1:X08} exceeds archive length.", i, fileId), fileId);
                }
                MixEntry entry = new MixEntry(fileId, fileOffset, fileLength, i);
                MixEntry[] existing;
                headerIds.Add(fileId);
                if (!this.mixFileContents.TryGetValue(fileId, out existing))
                {
                    this.mixFileContents.Add(fileId, new[] { entry });
                }
                else
                {
                    entry.Duplicate = existing.Length;
                    MixEntry[] newForId = new MixEntry[existing.Length + 1];
                    Array.Copy(existing, newForId, existing.Length);
                    newForId[existing.Length] = entry;
                    this.mixFileContents[fileId] = newForId;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a view stream on the current mix file, or on the current embedded mix file.
        /// </summary>
        /// <param name="mixMap">The MemoryMappedFile of the mix file or parent mix file</param>
        /// <param name="mixFileStart">Start of the current mix file inside the mixMap</param>
        /// <param name="mixFileLength">End of the current mix file inside the mixMap</param>
        /// <param name="dataReadOffset">Read position inside the current mix file.</param>
        /// <param name="dataReadLength">Length of the data to read.</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return null instead of throwing an exception.</param>
        /// <returns>A Stream containing the contents of the requested section of the MemoryMappedFile, or an empty MemoryStream if <paramref name="dataReadLength"/> is 0.</returns>
        /// <exception cref="IndexOutOfRangeException">The data is not in the bounds of this mix file.</exception>
        private Stream CreateViewStream(MemoryMappedFile mixMap, long mixFileStart, long mixFileLength, long dataReadOffset, uint dataReadLength, bool throwWhenParsing)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Mixfile");
            }
            if (dataReadOffset + dataReadLength > mixFileLength)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                // Can normally never happen; it is checked when the header is read.
                throw new MixParseException("Data exceeds mix file bounds.");
            }
            if (dataReadLength == 0)
            {
                // If the given size is 0, CreateViewStream creates a stream with the data full length.
                // We don't want that, so we return an empty memorystream instead.
                return new MemoryStream(new byte[0]);
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
        /// <param name="throwWhenParsing">If false, parse errors will simply return null instead of throwing an exception.</param>
        /// <returns>
        ///     the entire decrypted header, including the 6 bytes with the amount of files and the data length at the start.
        ///     It might contain padding at the end as result of the decryption.
        /// </returns>
        private byte[] DecodeHeader(MemoryMappedFile mixMap, long mixStart, long mixLength, ref uint readOffset, bool throwWhenParsing)
        {
            // Based on specs written up by OmniBlade on the Shikadi Modding Wiki.
            // https://moddingwiki.shikadi.net/wiki/MIX_Format_(Westwood)
            // A very special thanks to Morton on the C&C Mod Haven Discord for helping me out with this.

            // DER should identify the block as "02 28": an integer of length 40.
            byte[] derKeyBytes = Convert.FromBase64String(PublicKey);
            if (derKeyBytes.Length < 42 || derKeyBytes[0] != 2 || derKeyBytes[1] != 40)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new MixParseException("Program error: internal header key info is incorrect.");
            }
            // Get the 40-byte key.
            byte[] modulusBytes = new byte[40];
            Array.Copy(derKeyBytes, 2, modulusBytes, 0, modulusBytes.Length);
            Array.Reverse(modulusBytes);
            BigInteger publicModulus = new BigInteger(modulusBytes);
            BigInteger publicExponent = new BigInteger(Exponent);
            // Read blocks
            byte[] readBlock;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, 80, throwWhenParsing))
            {
                if (headerStream == null && !throwWhenParsing)
                {
                    return null;
                }
                readBlock = headerStream.ReadAllBytes();
                readOffset += 80;
            }
            byte[] blowFishKey = MixFileCrypto.DecryptBlowfishKey(readBlock, publicModulus, publicExponent);
            byte[] blowBuffer = new byte[BlowfishStream.SIZE_OF_BLOCK];
            long remaining = mixLength - readOffset;
            byte[] decryptedHeader;
            using (Stream headerStream = this.CreateViewStream(mixMap, mixStart, mixLength, readOffset, (uint)remaining, throwWhenParsing))
            {
                if (headerStream == null && !throwWhenParsing)
                {
                    return null;
                }
                using (BlowfishStream bfStream = new BlowfishStream(headerStream, blowFishKey))
                {
                    bfStream.Read(blowBuffer, 0, BlowfishStream.SIZE_OF_BLOCK);

                    ushort fileCount = ArrayUtils.ReadUInt16FromByteArrayLe(blowBuffer, 0);
                    uint headerSize = 6 + (uint)(fileCount * 12);
                    uint blocksToRead = (headerSize + BlowfishStream.SIZE_OF_BLOCK - 1) / BlowfishStream.SIZE_OF_BLOCK;
                    uint realHeaderSize = blocksToRead * BlowfishStream.SIZE_OF_BLOCK;
                    if (readOffset + realHeaderSize > mixLength)
                    {
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new MixParseException("Not a valid mix file: encrypted header length exceeds file length.");
                    }
                    // Don't bother trimming this. It'll read it using the amount of files value anyway.
                    decryptedHeader = new byte[realHeaderSize];
                    // Add already-read block.
                    Array.Copy(blowBuffer, 0, decryptedHeader, 0, BlowfishStream.SIZE_OF_BLOCK);
                    readOffset += realHeaderSize;
                    bfStream.Read(decryptedHeader, BlowfishStream.SIZE_OF_BLOCK, (int)(realHeaderSize - BlowfishStream.SIZE_OF_BLOCK));
                }
            }
            return decryptedHeader;
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// Returns whether the MixFile object is disposed. Note that it will also be considered
        /// disposed if it is loaded from inside a parent mix file, and the parent is disposed.
        /// </summary>
        public bool Disposed => disposedValue || (IsEmbedded && parent.Disposed);

        protected virtual void Dispose(bool disposing)
        {
            // Only dispose if not an embedded mix file.
            // If embedded, the mixFileMap is contained in the parent.
            if (disposing)
            {
                if (!this.IsEmbedded && !disposedValue)
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

    public class MixEntry
    {
        public int Index;
        public uint Id;
        public string Name;
        public int Duplicate;
        public uint Offset;
        public uint Length;
        public MixContentType Type = MixContentType.Unknown;
        public string Info;
        public string Description;

        public string DisplayName => (Name ?? IdString) + (Duplicate == 0 ? string.Empty : " (" + Duplicate.ToString() + ")");
        public string SortName => Name ?? ("zzzzzzzzzzzz " + IdString);
        public string IdString => '[' + IdStringBare + ']';
        public string IdStringBare => Id.ToString("X4");

        public MixEntry()
        { }

        public MixEntry(MixEntry orig)
        {
            Index = orig.Index;
            Id = orig.Id;
            Name = orig.Name;
            Duplicate = orig.Duplicate;
            Offset = orig.Offset;
            Length = orig.Length;
            Type = orig.Type;
            Info = orig.Info;
            Description = orig.Description;
        }

        public MixEntry(uint id, string name, string description)
        {
            Name = name;
            Id = id;
            Description= description;
        }

        public MixEntry(string filename)
        {
            HashRol1 hashRol = new HashRol1();
            Id = hashRol.GetNameId(filename);
            Name = filename;
            Offset = 0;
            Length = 0;
        }

        public MixEntry(uint id, uint offset, uint length, int index)
        {
            Id = id;
            Offset = offset;
            Length = length;
            Index = index;
        }

        public override string ToString()
        {
            return DisplayName + " (" + Type.ToString() + ")";
        }
    }
}
