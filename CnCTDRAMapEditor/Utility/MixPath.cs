using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MobiusEditor.Utility
{
    internal class MixPath
    {
        /// <summary>
        /// Mattern to identify a filename as file ID. This can be used to analyse the data returned by <see cref="GetComponents"/>.
        /// </summary>
        public static readonly Regex FilePathIdPattern = new Regex("^\\*([0-9A-F]{8})\\*$");

        private static string GetMixFileName(MixFile mixFile)
        {
            if (mixFile == null)
                return String.Empty;
            if (!mixFile.IsEmbedded)
                return mixFile.FilePath;
            return mixFile.FileName ?? ('*' + mixFile.FileId.ToString("X4") + '*');
        }

        private static string GetMixEntryName(MixEntry file)
        {
            return file == null ? String.Empty : file.Name ?? ('*' + file.Id.ToString("X4") + '*');
        }

        /// <summary>
        /// Builds the mix file chain and mix content entries into the mix file path format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin".
        /// Where names are unavailable, ids will be substituted in the format "*FFFFFFFF*".
        /// </summary>
        /// <param name="mixFiles">Chain of opened mix files, starting with the physical file on disk, and continuing with deeper mix files opened inside.</param>
        /// <param name="files">Files found in the deepest mix file in the chain.</param>
        /// <returns></returns>
        public static string BuildMixPath(IEnumerable<MixFile> mixFiles, params MixEntry[] files)
        {
            int mixCount = mixFiles.Count();
            string[] mixArr = new string[mixCount];
            int ind = 0;
            foreach (MixFile mixFile in mixFiles)
            {
                mixArr[ind++] = GetMixFileName(mixFile);
            }
            string[] filesArr = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                MixEntry file = files[i];
                filesArr[i] = GetMixEntryName(file);
            }
            return BuildMixPath(mixArr, filesArr);
        }

        /// <summary>
        /// Builds the mix file chain and mix content entries into the mix file path format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin".
        /// </summary>
        /// <param name="mixArr">Array of the mix hierarchy, starting with the physical file on disk, and continuing with deeper mix files opened inside to get to the actual files.</param>
        /// <param name="filesArr">Array with the names of the actual files found in the deepest mix file in the chain.</param>
        /// <returns></returns>
        public static string BuildMixPath(string[] mixArr, string[] filesArr)
        {
            return String.Join(";", mixArr) + "?" + String.Join(";", filesArr);
        }

        /// <summary>
        /// Gets the path components inside the given mix path. Any ids inside the components will be left as they are,
        /// in hexadecimal format, and surrounded by asterisks, to be easily identifiable with the <see cref="FilePathIdPattern"/>.
        /// </summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <param name="mixParts">The mix files to open to get to the files, starting with the physical file on disk, and continuing with deeper mix files opened inside.</param>
        /// <param name="filenameParts">The files to open that can be found in the deepest mix file in the chain. Normally a .ini on the first index and a .bin on the second.</param>
        public static void GetComponents(string path, out string[] mixParts, out string[] filenameParts)
        {
            if (path == null)
            {
                path = string.Empty;
            }
            int index = path.IndexOf('?');
            string mixString = index == -1 ? String.Empty : path.Substring(0, index);
            mixParts = mixString.Split(';');
            string filenameString = index == -1 ? String.Empty : path.Substring(index + 1);
            filenameParts = filenameString.Split(';');
        }

        /// <summary>
        /// Gets the path components inside the given mix path. Any ids inside the components will be returned as UI-viewable strings,
        /// with the IDs in hexadecimal format, and enclosed in square brackets.
        /// </summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <param name="mixParts">The mix files to open to get to the files, starting with the physical file on disk, and continuing with deeper mix files opened inside.</param>
        /// <param name="filenameParts">The files to open that can be found in the deepest mix file in the chain. Normally a .ini on the first index and a .bin on the second.</param>
        /// <returns></returns>
        public static string GetComponentsViewable(string path, out string[] mixParts, out string[] filenameParts)
        {
            GetComponents(path, out mixParts, out filenameParts);
            // Only check on IDs starting from the second entry; first should be an absolute path.
            for (int i = 1; i < mixParts.Length; ++i)
            {
                string mixPart = mixParts[i];
                Match mixId = FilePathIdPattern.Match(mixPart);
                if (mixId.Success)
                {
                    mixParts[i] = "[" + mixId.Groups[1].Value + "]";
                }
            }
            for (int i = 0; i < filenameParts.Length; ++i)
            {
                string filenamePart = filenameParts[i];
                Match filenameId = FilePathIdPattern.Match(filenamePart);
                if (filenameId.Success)
                {
                    filenameParts[i] = "[" + filenameId.Groups[1].Value + "]";
                }
            }
            return String.Join(";", mixParts) + "?" + String.Join(";", filenameParts);
        }

        /// <summary>
        /// Returns the mix path as a UI-viewable string, with " -&gt; " arrows indicating the internal hierarchy, and any IDs
        /// enclosed in square brackets. As final component, it returns the first found filename in the filename parts of the mix path block.
        /// </summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <param name="shortPath">true if the path of the original mix file should not be included.</param>
        /// <param name="nameIsId">returns whether the returned filename is a file ID or a real identified filename.</param>
        /// <returns>The mix path as a UI-viewable string</returns>
        public static string GetFileNameReadable(string path, bool shortPath, out bool nameIsId)
        {
            nameIsId = false;
            GetComponents(path, out _, out string[] filenamePartsRaw);
            GetComponentsViewable(path, out string[] mixParts, out string[] filenameParts);
            FileInfo fileInfo = new FileInfo(mixParts[0]);
            string mixString = String.Join(String.Empty, mixParts.Skip(1).Select(mp => " -> " + mp).ToArray());
            string mixname = shortPath ? fileInfo.Name : fileInfo.FullName;
            string fullName = mixname + mixString;
            string loadedName = filenameParts[0];
            string loadedNameRaw = filenamePartsRaw[0];
            if (String.IsNullOrEmpty(loadedName) && filenameParts.Length > 1 && !String.IsNullOrEmpty(filenameParts[1]))
            {
                // Use the .bin file.
                loadedName = filenameParts[1];
                loadedNameRaw = filenamePartsRaw[1];
            }
            if (!String.IsNullOrEmpty(loadedName))
            {
                nameIsId = MixPath.FilePathIdPattern.IsMatch(loadedNameRaw);
                fullName += " -> " + loadedName;
            }
            return fullName;
        }

        public static bool IsMixPath(string path)
        {
            if (String.IsNullOrEmpty(path) || !path.Contains('?')) {
                return false;
            }
            GetComponents(path, out string[] mixParts, out string[] nameParts);
            // Not checking on actual file existence; just validity of the format.
            return mixParts.Length > 0 && mixParts[0].Length > 0 && nameParts.Length > 0 && nameParts[0].Length > 0;
        }

        /// <summary>Opens a mix path and checks if the files involved are all present. If the files to open are a pair, this checks them both.</summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <returns>True if the mix file exists, and the internal file could be found inside.</returns>
        public static bool PathIsValid(string path)
        {
            GetComponents(path, out string[] mixParts, out string[] filenameParts);
            List<FileType> toCheck = new List<FileType>();
            if (filenameParts.Length > 0 && !String.IsNullOrEmpty(filenameParts[0]))
            {
                toCheck.Add(FileType.INI);
            }
            if (filenameParts.Length > 1 && !String.IsNullOrEmpty(filenameParts[1]))
            {
                toCheck.Add(FileType.BIN);
            }
            int checks = toCheck.Count;
            if (checks == 0)
            {
                return false;
            }
            bool[] checkOk = new bool[checks];
            for (int i = 0; i < checks; ++i)
            {
                using (MixFile mainMix = OpenMixPath(path, toCheck[i], out MixFile contentMix, out MixEntry fileEntry))
                {
                    if (mainMix != null && contentMix != null && fileEntry != null)
                    {
                        // Don't even need to really open it; the fact fileEntry was found is enough.
                        checkOk[i] = true;
                    }
                }
            }
            for (int i = 0; i < checks; ++i)
            {
                if (!checkOk[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Opens a mix path and reads the requested file to a byte array.</summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <param name="fileType">File type to open; INI or BIN, to refer to the first or second internal file (after the question mark in the path). Specify "None" to use the first one that is filled in.</param></param>
        /// <param name="fileEntry"></param>
        /// <returns>A byte array containing the file contents of the target file, or null if some component in the chain could not be found.</returns>
        public static byte[] ReadFile(string path, FileType fileType, out MixEntry fileEntry)
        {
            using (MixFile mainMix = OpenMixPath(path, fileType, out MixFile contentMix, out fileEntry))
            {
                if (mainMix != null && contentMix != null && fileEntry != null)
                {
                    return contentMix.ReadFile(fileEntry);
                }
            }
            return null;
        }

        /// <summary>
        /// Parses and opens a mix path. Returns the main mix file that should be disposed after the operation, and has output parameters
        /// for the actual content mix to request the file on, and the mix content info to use to request the file.
        /// </summary>
        /// <param name="path">Mix file path block, in the format "x:\path\mixfile.mix;submix1.mix;submix2.mix?file.ini;file.bin"</param>
        /// <param name="fileType">File type to open; INI or BIN, to refer to the first or second internal file (after the question mark in the path). Specify "None" to use the first one that is filled in.</param>
        /// <param name="contentMix">Output parameter for the actual mix file to request the <paramref name="fileEntry"/> on.</param>
        /// <param name="fileEntry">Output parameter for the mix entry file info to use to request access to read the file from <paramref name="contentMix"/>. If available as name and not as id, the filename from the parsed path is filled in on this.</param>
        /// <returns>The main mix file that should be disposed after the file contents have been read from the <paramref name="contentMix"/> file, or null if some component in the chain could not be found.</returns>
        public static MixFile OpenMixPath(string path, FileType fileType, out MixFile contentMix, out MixEntry fileEntry)
        {
            contentMix = null;
            fileEntry = null;
            GetComponents(path, out string[] mixParts, out string[] filenameParts);
            if (mixParts == null || mixParts.Length == 0 || mixParts[0] == null || mixParts[0].Length == 0
                || filenameParts == null || filenameParts.Length == 0
                || (filenameParts[0] == null || mixParts[0].Length == 0) && (filenameParts[1] == null || mixParts[1].Length == 0))
            {
                return null;
            }
            string baseMixFile = mixParts[0];
            string filename = null;
            switch (fileType)
            {
                case FileType.INI:
                    filename = filenameParts[0];
                    break;
                case FileType.BIN:
                    filename = filenameParts.Length < 2 ? String.Empty : filenameParts[1];
                    break;
                case FileType.None:
                    filename = filenameParts[0];
                    if (String.IsNullOrEmpty(filename) && filenameParts.Length > 1)
                    {
                        filename = filenameParts[1];
                    }
                    break;

            }
            if (String.IsNullOrEmpty(filename))
            {
                return null;
            }
            if (!File.Exists(baseMixFile))
            {
                return null;
            }
            MixFile baseMix;
            try
            {
                baseMix = new MixFile(baseMixFile);
            }
            catch
            {
                return null;
            }
            // Set to base mix at first, then the remaining mix file names are looped to find any deeper ones to open.
            contentMix = baseMix;
            // If anything goes wrong in the next part, the base mix will be disposed before exiting, so everything is cleaned up.
            int len = mixParts.Length;
            for (int i = 1; i < len; ++i)
            {
                string subMix = mixParts[i];
                if (subMix.Length == 0)
                {
                    try { baseMix.Dispose(); }
                    catch { /* ignore */ }
                    contentMix = null;
                    return null;
                }
                Match mixIdMatch = FilePathIdPattern.Match(subMix);
                uint mixId = 0;
                bool mixIsId = mixIdMatch.Success;
                if (mixIsId)
                {
                    mixId = UInt32.Parse(mixIdMatch.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                MixEntry[] entries = mixIsId ? contentMix.GetFullFileInfo(mixId) : contentMix.GetFullFileInfo(subMix);
                if (entries == null || entries.Length == 0)
                {
                    try { baseMix.Dispose(); }
                    catch { /* ignore */ }
                    contentMix = null;
                    return null;
                }
                MixEntry newmixfile = entries[0];
                // no need to keep track of those; they don't need to get disposed anyway.
                try
                {
                    contentMix = new MixFile(contentMix, newmixfile);
                }
                catch
                {
                    try { baseMix.Dispose(); }
                    catch { /* ignore */ }
                    contentMix = null;
                    return null;
                }
            }
            Match idMatch = FilePathIdPattern.Match(filename);
            uint id = 0;
            bool isId = idMatch.Success;
            if (isId)
            {
                id = UInt32.Parse(idMatch.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            MixEntry[] fileEntries = isId ? contentMix.GetFullFileInfo(id) : contentMix.GetFullFileInfo(filename);
            if (fileEntries == null || fileEntries.Length == 0)
            {
                try { baseMix.Dispose(); }
                catch { /* ignore */ }
                contentMix = null;
                return null;
            }
            fileEntry = fileEntries[0];
            if (!isId)
            {
                fileEntry.Name = filename;
            }
            return baseMix;
        }
    }
}
