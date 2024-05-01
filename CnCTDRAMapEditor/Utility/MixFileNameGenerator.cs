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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// ROMFIS - The Ridiculously Overengineered Mix Filename Identification System!
    /// </summary>
    public class MixFileNameGenerator
    {
        [Flags]
        private enum ConstrArgs
        {
            None     /**/ = 0,
            IniObj   /**/ = 1 << 0,
            IniPath  /**/ = 1 << 1,
            SideInis /**/ = 1 << 2,
        }

        private const string parseError = "Error parsing ini: section {0} not found.";
        private const string gamesHeader = "Games";

        private static readonly Dictionary<string, HashMethod> hashMethods = HashMethod.GetRegisteredMethods().ToDictionary(m => m.SimpleName, StringComparer.OrdinalIgnoreCase);
        private static readonly HashMethod defaultHashMethod = HashMethod.GetRegisteredMethods().FirstOrDefault();


        /// <summary>input-order version of the keys in <see cref="gameInfo"/>.</summary>
        private List<string> games = new List<string>();
        private Dictionary<string, GameDefinition> gameInfo = new Dictionary<string, GameDefinition>();

        public List<string> Games => games.ToList();

        public MixFileNameGenerator(string iniPath)
            : this(null, iniPath, null, ConstrArgs.IniPath)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iniFile">Ini file to open. If additional inis need to be read, they will be looked up in the current working directory.</param>
        public MixFileNameGenerator(INI iniFile)
            : this(iniFile, Path.Combine(Path.GetDirectoryName("."), "dummy.ini"), null, ConstrArgs.IniObj)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iniFile">Main ini file to open.</param>
        /// <param name="iniPath">Source path of <paramref name="iniFile"/>, Is needed if side inis need to be read.</param>
        public MixFileNameGenerator(INI iniFile, string iniPath)
            : this(iniFile, iniPath, null, ConstrArgs.IniObj | ConstrArgs.IniPath)
        {
        }

        /// <summary>
        /// Make filename generator from ini objects, with possible additional ini objects given
        /// to read the file lists of specific games. This overload can be used to load the strings
        /// from embedded resources in the project.
        /// </summary>
        /// <param name="iniFile">Main ini file to open.</param>
        /// <param name="additionalInis">Dictionary of additional ini files that can be used to read the file lists of specific games.</param>
        public MixFileNameGenerator(INI iniFile, Dictionary<string, INI> additionalInis)
            : this(iniFile, null, additionalInis, ConstrArgs.IniObj | ConstrArgs.SideInis)
        {
        }

        /// <summary>
        /// Full constructor; not public because all specific cases are handled in the overloads.
        /// </summary>
        /// <param name="iniFile">Main ini file to open. Can be null if <paramref name="iniPath"/> is given.</param>
        /// <param name="iniPath">Source path of <paramref name="iniFile"/>. Is needed if side inis need to be read, and <paramref name="additionalInis"/> is not supplied.</param>
        /// <param name="additionalInis">Dictionary of additional ini files that can be used to read the file lists of specific games.</param>
        /// <param name="originArgs">Origin args, to know what to give exceptions on when data is missing.</param>
        /// <exception cref="ArgumentException"></exception>
        private MixFileNameGenerator(INI iniFile, string iniPath, Dictionary<string, INI> additionalInis, ConstrArgs originArgs)
        {
            bool hasIni = (originArgs & ConstrArgs.IniObj) != 0;
            bool hasPath = (originArgs & ConstrArgs.IniPath) != 0;
            bool hasSide = (originArgs & ConstrArgs.SideInis) != 0;
            bool validPath = File.Exists(iniPath);
            // If given, ini obj needs to be valid.
            if (hasIni && iniFile == null)
            {
                throw new ArgumentNullException("iniFile");
            }
            // If path is given and no ini object, path needs to exist.
            if (!hasIni && hasPath && !validPath)
            {
                throw new ArgumentNullException("readPath");
            }
            bool validFolder = Directory.Exists(Path.GetDirectoryName(iniPath));
            if (iniFile == null && validPath)
            {
                iniFile = new INI();
                using (TextReader reader = new StreamReader(iniPath, Encoding.GetEncoding(437)))
                {
                    iniFile.Parse(reader);
                }
            }
            if (iniFile == null && hasIni)
            {
                throw new ArgumentNullException("iniFile");
            }
            INISection gamesSection = iniFile.Sections[gamesHeader];
            if (gamesSection == null)
            {
                throw new ArgumentException(String.Format(parseError, gamesHeader), "iniFile");
            }
            // Iterate over games
            int gameIndex = 0;
            string gameString;
            while (!String.IsNullOrEmpty(gameString = gamesSection.TryGetValue(gameIndex.ToString())))
            {
                gameIndex++;
                INISection gameSection = iniFile.Sections[gameString];
                if (gameSection == null)
                {
                    continue;
                }
                // Read game info
                string[] externalFiles = (gameSection.TryGetValue("ContentInis") ?? String.Empty).Split(',', true);
                string[] typesSections = (gameSection.TryGetValue("FileTypes") ?? String.Empty).Split(',', true);
                string[] filesSections = (gameSection.TryGetValue("FilesSections") ?? String.Empty).Split(',', true);
                string[][] theaterInfos = GetTheaterInfo(gameSection, "Theaters", true);
                string[][] modTheaterInfos = GetTheaterInfo(gameSection, "ModTheaters", false);
                string hasher = gameSection.TryGetValue("Hasher");
                YesNoBooleanTypeConverter boolConv = new YesNoBooleanTypeConverter();
                bool newMixFormat = boolConv.ConvertFrom(gameSection.TryGetValue("NewMixFormat"));
                bool hasMixNesting = boolConv.ConvertFrom(gameSection.TryGetValue("HasMixNesting"));
                HashMethod hashMethod;
                hashMethods.TryGetValue(hasher, out hashMethod);
                // no files sections specified
                if (filesSections.All(fs => String.IsNullOrEmpty(fs)))
                {
                    continue;
                }
                // Read game inis
                List<INI> gameIniFiles = new List<INI>();
                foreach (string ini in externalFiles)
                {
                    INI extraIni;
                    if (additionalInis != null && additionalInis.TryGetValue(ini, out extraIni))
                    {
                        gameIniFiles.Add(extraIni);
                        continue;
                    }
                    if (validFolder)
                    {
                        string filesListPath = Path.Combine(Path.GetDirectoryName(iniPath), ini);
                        if (File.Exists(filesListPath))
                        {
                            extraIni = new INI();
                            try
                            {
                                using (TextReader reader = new StreamReader(filesListPath, Encoding.GetEncoding(437)))
                                {
                                    extraIni.Parse(reader);
                                }
                                // If anything fails in this, the ini is not added.
                                gameIniFiles.Add(extraIni);
                            }
                            catch { /* ignore */ }
                        }
                    }
                }
                // Add main ini as final one to read from.
                gameIniFiles.Add(iniFile);
                // Get type definitions for game
                Dictionary<string, FileNameGeneratorEntry[]> typeDefsForGame = GetTypeDefinitions(typesSections, gameIniFiles);
                List<INISection> gameFilesSections = new List<INISection>();
                Dictionary<string, string> gameDescriptions = new Dictionary<string, string>();
                List<string> gameFiles = new List<string>();
                foreach (string filesList in filesSections)
                {
                    foreach (INI ini in gameIniFiles)
                    {
                        INISection gameFilesSection = ini.Sections[filesList];
                        if (gameFilesSection == null)
                        {
                            continue;
                        }
                        foreach (KeyValuePair<string, string> iniEntry in gameFilesSection)
                        {
                            if (!gameDescriptions.ContainsKey(iniEntry.Key))
                            {
                                gameFiles.Add(iniEntry.Key);
                                gameDescriptions.Add(iniEntry.Key, iniEntry.Value);
                            }
                        }
                        break;
                    }
                }
                // don't add games with zero files.
                if (gameFiles.Count == 0)
                {
                    continue;
                }
                // Fill data into the GameDefinition object.
                GameDefinition gd = new GameDefinition(gameString);
                gd.TypeDefinitions = typeDefsForGame;
                gd.FileInfoRaw = gameFiles;
                gd.FileDescriptions = gameDescriptions;
                gd.Hasher = hashMethod ?? defaultHashMethod;
                gd.NewMixFormat = newMixFormat;
                gd.HasMixNesting = hasMixNesting;
                gd.TheaterInfo = theaterInfos;
                if (modTheaterInfos != null && modTheaterInfos.Length > 0)
                {
                    gd.ModTheaterInfo = modTheaterInfos;
                }
                // All necessary info is inserted. Tell the GameDefinition object to generate the name ids.
                gd.GenerateNameIds();
                gameInfo.Add(gameString, gd);
                games.Add(gameString);
            }
        }

        private Dictionary<string, FileNameGeneratorEntry[]> GetTypeDefinitions(String[] typesSections, List<INI> toScan)
        {
            Dictionary<string, FileNameGeneratorEntry[]> typeDefinitions = new Dictionary<string, FileNameGeneratorEntry[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string sectionName in typesSections)
            {
                INISection typesSection = null;
                foreach (INI ini in toScan)
                {
                    typesSection = ini.Sections[sectionName];
                    if (typesSection != null)
                    {
                        break;
                    }
                }
                if (typesSection == null)
                {
                    continue;
                }
                int index = 0;
                string typeString;
                while (!String.IsNullOrEmpty(typeString = typesSection.TryGetValue(index.ToString())))
                {
                    index++;
                    // Read first encountered one only.
                    if (typeDefinitions.ContainsKey(typeString))
                    {
                        continue;
                    }
                    INISection typeSection = null;
                    foreach (INI iniFile in toScan)
                    {
                        typeSection = iniFile.Sections[typeString];
                        if (typeSection != null)
                        {
                            break;
                        }
                    }
                    if (typeSection == null)
                    {
                        continue;
                    }
                    int nameIndex = 0;
                    string nameVal;
                    List<FileNameGeneratorEntry> generators = new List<FileNameGeneratorEntry>();
                    while (!string.IsNullOrEmpty(nameVal = typeSection.TryGetValue(nameIndex.ToString())))
                    {
                        string info = typeSection.TryGetValue(nameIndex.ToString() + "Info");
                        nameIndex++;
                        generators.Add(new FileNameGeneratorEntry(nameVal, info));
                    }
                    if (generators.Count > 0)
                    {
                        typeDefinitions.Add(typeString, generators.ToArray());
                    }
                }
            }
            return typeDefinitions;
        }

        private string[][] GetTheaterInfo(INISection gameSection, string keyName, bool generateDummy)
        {
            string theaters = gameSection.TryGetValue(keyName);
            if (string.IsNullOrEmpty(theaters))
            {
                return !generateDummy ? null : new string[][] { new[] { string.Empty } };
            }
            string[] theatersList = theaters.Split(',', StringSplitOptions.RemoveEmptyEntries, true);
            string[][] theaterInfos = new string[theatersList.Length][];
            for (int i = 0; i < theatersList.Length; ++i)
            {
                theaterInfos[i] = theatersList[i].Split(':', StringSplitOptions.None, true);
            }
            return theaterInfos;
        }

#if DEBUG
        /// <summary>
        /// Debug function. Don't use.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MixEntry> GetAllNameIds()
        {
            foreach (string game in games)
            {
                if (gameInfo.TryGetValue(game, out GameDefinition gd))
                {
                    foreach (MixEntry nameInfo in gd.FileInfo)
                    {
                        yield return nameInfo;
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Identify the files inside a mix file, using a specific game.
        /// Generally used for embedded mix files, using the parent's game type.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        public string IdentifyMixFile(MixFile mixFile)
        {
            return IdentifyMixFile(mixFile, null);
        }

        /// <summary>
        /// Identify the files inside a mix file, and return the identifier for the detected game.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        /// <param name="forcedGameType">Game type identified on the parent; is always taken as primary type.</param>
        /// <returns>The game type identified for this mix file.</returns>
        public string IdentifyMixFile(MixFile mixFile, string forcedGameType)
        {
            double maxRatio = 0;
            GameDefinition maxGame = null;
            GameDefinition forced = null;
            if (forcedGameType != null)
            {
                gameInfo.TryGetValue(forcedGameType, out forced);
            }
            Dictionary<string,double> identifiedAmounts = new Dictionary<string,double>();
            for (int i = 0; i < games.Count; ++i)
            {
                string gameName = games[i];
                if (gameInfo.TryGetValue(gameName, out GameDefinition gd))
                {
                    // ignore games that cam't handle this mix file
                    if (!gd.NewMixFormat && mixFile.IsNewFormat)
                    {
                        continue;
                    }
                    // get identified amount, including deeper check if needed.
                    int amount = mixFile.Identify(gd.FileInfo, gd.HasMixNesting, out int fullAmount);
                    double ratio = amount / (double)fullAmount;
                    identifiedAmounts.Add(gameName, ratio);
                    if (ratio > maxRatio)
                    {
                        maxRatio = ratio;
                        maxGame = gd;
                    }
                }
            }
            bool replaceGame = forced != null && maxGame != forced;
            if (replaceGame)
            {
                maxGame = forced;
            }
            if (maxGame == null)
            {
                return null;
            }
            // Select all game definition, in order of identification, that have the same hasher algorithm as the top identified one.
            HashMethod mh = maxGame.Hasher;
            string mhName = mh.SimpleName;
            List<GameDefinition> viableGames = identifiedAmounts.Keys
                .OrderByDescending(gt => identifiedAmounts[gt])
                .Select(gn => gameInfo[gn])
                .Where(gd => mhName.Equals(gd.Hasher.SimpleName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (replaceGame)
            {
                // Move forced type to first place.
                viableGames.Remove(forced);
                viableGames.Insert(0, forced);
            }
            Dictionary<uint, MixEntry> info = new Dictionary<uint, MixEntry>();
            foreach (GameDefinition gd in viableGames)
            {
                foreach (MixEntry entry in gd.FileInfo)
                {
                    if (!info.ContainsKey(entry.Id))
                    {
                        info.Add(entry.Id, entry);
                    }
                }
            }
            mixFile.InsertInfo(info, mh, true);
            return maxGame.Name;
        }

        private class GameDefinition
        {
            public string Name { get; private set; }
            public Dictionary<string, FileNameGeneratorEntry[]> TypeDefinitions { get; set; }
            public List<MixEntry> FileInfo { get; set; }
            public List<string> FileInfoRaw { get; set; }
            public Dictionary<string, string> FileDescriptions { get; set; }
            public HashMethod Hasher { get; set; }
            public string[][] TheaterInfo { get; set; }
            public string[][] ModTheaterInfo { get; set; }
            public bool HasMixNesting { get; set; }
            public bool NewMixFormat { get; set; }

            public GameDefinition(string name)
            {
                this.Name = name;
            }

            public void GenerateNameIds()
            {
                this.FileInfo = GetNameIds().ToList();
#if DEBUG
                // For testing
                /*/
                foreach (MixEntry entry in this.FileInfo)
                {
                    Debug.WriteLine(String.Format("{0:X8} : {1} - {2}", entry.Id, entry.Name, entry.Description ?? String.Empty));
                }
                //*/
#endif
            }

            private IEnumerable<MixEntry> GetNameIds()
            {
                string[][] theaterInfo = this.TheaterInfo ?? new string[][] { new[] { string.Empty } };
                string[][] theaterInfomod = this.ModTheaterInfo;
                if (this.FileInfoRaw == null || this.FileInfoRaw.Count == 0)
                {
                    yield break;
                }
                foreach (MixEntry fileInfo in GetHashInfo(this.FileInfoRaw, this.FileDescriptions, this.TypeDefinitions, theaterInfo, this.Hasher, false))
                {
                    yield return fileInfo;
                }
                if (theaterInfomod != null && theaterInfomod.Length > 0)
                {
                    foreach (MixEntry fileInfo in GetHashInfo(this.FileInfoRaw, this.FileDescriptions, this.TypeDefinitions, theaterInfomod, this.Hasher, true))
                    {
                        yield return fileInfo;
                    }
                }
            }

            private IEnumerable<MixEntry> GetHashInfo(IEnumerable<string> filenames, Dictionary<string, string> filenameInfo, Dictionary<string, FileNameGeneratorEntry[]> typeDefinitions,
                string[][] theaterInfo, HashMethod hashMethod, bool ignoreNonTheaterFiles)
            {
                foreach (string filename in filenames)
                {
                    string info = filenameInfo == null || !filenameInfo.ContainsKey(filename) ? null : filenameInfo[filename];
                    // Ignore 1-character dummy strings.
                    if (info.Trim().Length <= 1)
                    {
                        info = null;
                    }
                    string[] fnParts = filename.Split(',');
                    string name = fnParts[0].Trim();
                    string type = fnParts.Length < 2 ? null : fnParts[1].Trim();
                    if (String.IsNullOrEmpty(type))
                    {
                        if (ignoreNonTheaterFiles)
                        {
                            continue;
                        }
                        yield return new MixEntry(hashMethod.GetNameId(name), name, info);
                    }
                    else
                    {
                        FileNameGeneratorEntry[] generators = null;
                        if (!typeDefinitions.TryGetValue(type, out generators))
                        {
                            throw new Exception("Error in filename data: no definition found for type \"" + type + "\"");
                        }
                        // Generate all normal filenames.
                        foreach (FileNameGeneratorEntry generator in generators)
                        {
                            // if only running for addon-theaters, skip files that don't have theater info in them.
                            if (ignoreNonTheaterFiles && !generator.IsTheaterDependent)
                            {
                                continue;
                            }
                            string fileInfo = info;
                            if (!String.IsNullOrEmpty(generator.ExtraInfo))
                            {
                                fileInfo = (String.IsNullOrEmpty(info) ? string.Empty : (info + " ")) + generator.ExtraInfo;
                            }
                            foreach ((string nameStr, string infoStr) in generator.GetNames(name, fileInfo, theaterInfo))
                            {
                                yield return new MixEntry(hashMethod.GetNameId(nameStr), nameStr, infoStr);
                            }
                        }
                    }
                }
            }
        }

        public class FileNameGeneratorEntry
        {
            private static readonly Regex IterateRegex = new Regex("\\[((?:[^\\[\\]\\(\\)])|(?:\\([^\\[\\]\\(\\))]+\\)))+\\]", RegexOptions.Compiled);
            public bool IsTheaterDependent { get; private set; }
            public int HighestArg { get; private set; }
            public string ExtraInfo { get; set; }
            private string[][] iterations;

            public FileNameGeneratorEntry(string format)
                : this(format, null)
            {
            }

            public FileNameGeneratorEntry(string format, string extraInfo)
            {
                ExtraInfo = extraInfo;
                int highestArgFormat = EnhFormatString.GetHighestArg(format);
                int highestArgInfo = EnhFormatString.GetHighestArg(extraInfo);
                // This ignores highest arg in info.
                IsTheaterDependent = highestArgFormat > 0;
                HighestArg = Math.Max(highestArgFormat, highestArgInfo);
                Match iteratorMatch = IterateRegex.Match(format);
                List<string[]> iterationBlocks = new List<string[]>();
                // Chop that string up!
                int currentIndex = 0;
                while (iteratorMatch.Success)
                {
                    // capture in-between chunks as a single-item list to 'iterate over'.
                    if (iteratorMatch.Index > currentIndex)
                    {
                        iterationBlocks.Add(new[] { format.Substring(currentIndex, iteratorMatch.Index - currentIndex) });
                    }
                    List<string> iterationChunks = new List<string>();
                    foreach (Capture capture in iteratorMatch.Groups[1].Captures)
                    {
                        string val = capture.Value;
                        if (val.Length > 2)
                        {
                            // chop off the surrounding brackets
                            val = val.Substring(1, val.Length - 2);
                        }
                        iterationChunks.Add(val);
                    }
                    iterationBlocks.Add(iterationChunks.ToArray());
                    currentIndex = iteratorMatch.Index + iteratorMatch.Length;
                    iteratorMatch = iteratorMatch.NextMatch();
                }
                if (currentIndex < format.Length)
                {
                    iterationBlocks.Add(new[] { format.Substring(currentIndex, format.Length - currentIndex) });
                }
                iterations = iterationBlocks.ToArray();
            }

            public IEnumerable<(string, string)> GetNames(string baseName, string extraInfo, string[][] theaterInfo)
            {
                foreach ((string, string) name in CreateNames(baseName, extraInfo, theaterInfo, 0, new int[iterations.Length], iterations.Length, iterations.Length - 1))
                {
                    yield return name;
                }
            }

            /// <summary>
            /// This is the main workhorse, it creates new strings and formats them to output the final composed names.
            /// </summary>
            /// <param name="baseName">base name to format into the string as {0}</param>
            /// <param name="theaterInfo">Theater info, used to iterate over the names in case groups beyond {0} are used.</param>
            /// <param name="currentChunkPosition">The position of the entry which is replaced by new items currently.</param>
            /// <param name="chunkEntries">The current key represented as int array, to be filled ith the array of items to iterate.</param>
            /// <param name="chunkLength">The length of the full key, to know when to end.</param>
            /// <param name="indexOfLastChunk">The length of the full key minus one, to know when to end.</param>
            /// <returns></returns>            
            private IEnumerable<(string, string)> CreateNames(string baseName, string extraInfo, string[][] theaterInfo, int currentChunkPosition, int[] chunkEntries, Int32 chunkLength, Int32 indexOfLastChunk)
            {
                int nextCharPosition = currentChunkPosition + 1;
                int entriesLength = iterations[currentChunkPosition].Length;
                // We are looping through the full length of our entries-to-test array
                for (int i = 0; i < entriesLength; i++)
                {
                    // The character at the currentCharPosition will be replaced by a new character
                    // from the charactersToTest array => a new key combination will be created
                    chunkEntries[currentChunkPosition] = i;

                    // The method calls itself recursively until all positions of the key char array have been replaced
                    if (currentChunkPosition < indexOfLastChunk)
                    {
                        foreach ((string, string) nameInfo in this.CreateNames(baseName, extraInfo, theaterInfo, nextCharPosition, chunkEntries, chunkLength, indexOfLastChunk))
                        {
                            yield return nameInfo;
                        }
                        continue;
                    }
                    foreach ((string, string) generatedName in BuildString(baseName, extraInfo, theaterInfo, chunkEntries, chunkLength))
                    {
                        yield return generatedName;
                    }
                }
            }

            private IEnumerable<(string, string)> BuildString(string baseName, string extraInfo, string[][] theaterInfo, int[] keyEntries, int keyLength)
            {
                string[] chunks = new string[keyLength];
                for (int i = 0; i < keyLength; i++)
                {
                    chunks[i] = iterations[i][keyEntries[i]];
                }
                string format = String.Join(String.Empty, chunks);
                int arrLen = HighestArg + 1;
                object[] strings = new object[arrLen];
                if (arrLen > 0)
                {
                    strings[0] = (EnhFormatString)baseName;
                }
                if (!IsTheaterDependent)
                {
                    for (int j = 1; j < arrLen; j++)
                    {
                        // If the description uses theater args, ignore them.
                        strings[j] = String.Empty;
                    }
                    string name = String.Format(format, strings);
                    string info = extraInfo == null ? null : String.Format(extraInfo, strings);
                    yield return (name, info);
                }
                else
                {
                    for (int i = 0; i < theaterInfo.Length; i++)
                    {
                        string[] thInfo = theaterInfo[i];
                        int thInfoLen = thInfo.Length + 1;
                        for (int j = 1; j < arrLen; j++)
                        {
                            strings[j] = new EnhFormatString(j >= thInfoLen ? String.Empty : thInfo[j - 1]);
                        }
                        string name = String.Format(format, strings);
                        string info = extraInfo == null ? null : String.Format(extraInfo, strings);
                        yield return (name, info);

                    }
                }

            }
        }
    }
}
