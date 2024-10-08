﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
using System.Globalization;
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
        /// <summary>Used to let the main constructor know which overload it was called from, to determine what to throw errors about.</summary>
        [Flags]
        private enum ConstrArgs
        {
            None     /**/ = 0,
            IniObj   /**/ = 1 << 0,
            IniPath  /**/ = 1 << 1,
            SideInis /**/ = 1 << 2,
        }

        private const string ParseError = "Error parsing ini: section {0} not found.";
        // Main games header. This is the only hardcoded header in the system.
        private const string GamesHeader = "Games";
        // ini keys per game definition.
        private const string GameKeyContentInis = "ContentInis";
        private const string GameKeyFileTypes = "FileTypes";
        private const string GameKeyFilesSections = "FilesSections";
        private const string GameKeyTheaters = "Theaters";
        private const string GameKeyModTheaters = "ModTheaters";
        private const string GameKeyHasher = "Hasher";
        private const string GameKeyNewMixFormat = "NewMixFormat";
        private const string GameKeyHasMixNesting = "HasMixNesting";
        private const string InfoSuffix = "Info";

        private static readonly Dictionary<string, HashMethod> hashMethods = HashMethod.GetRegisteredMethods().ToDictionary(m => m.SimpleName, StringComparer.OrdinalIgnoreCase);
        private static readonly HashMethod defaultHashMethod = HashMethod.GetRegisteredMethods().FirstOrDefault();

        /// <summary>input-order version of the keys in <see cref="gameInfo"/>.</summary>
        private List<string> games = new List<string>();
        private Dictionary<string, GameDefinition> gameInfo = new Dictionary<string, GameDefinition>();

        /// <summary>
        /// List of games that were read from the hashing information.
        /// </summary>
        public List<string> Games => games.ToList();

        private List<string> errors = new List<string>();
        public List<string> ProcessErrors => errors.ToList();

        /// <summary>
        /// Creates a new MixFileNameGenerator from a given ini file path.
        /// </summary>
        /// <param name="iniPath">Ini file to open. If additional inis need to be read, they will be looked up relative to the folder this ini is located in.</param>
        public MixFileNameGenerator(string iniPath)
            : this(null, iniPath, null, ConstrArgs.IniPath)
        {
        }

        /// <summary>
        /// Creates a new MixFileNameGenerator from a given ini object.
        /// </summary>
        /// <param name="iniFile">Ini file to open. If additional inis need to be read, they will be looked up in the current working directory.</param>
        public MixFileNameGenerator(INI iniFile)
            : this(iniFile, Path.Combine(Path.GetDirectoryName("."), "dummy.ini"), null, ConstrArgs.IniObj)
        {
        }

        /// <summary>
        /// Creates a new MixFileNameGenerator from a given ini object.
        /// </summary>
        /// <param name="iniFile">Main ini file to open.</param>
        /// <param name="iniPath">Source path of <paramref name="iniFile"/>, Is needed if side inis need to be read.</param>
        public MixFileNameGenerator(INI iniFile, string iniPath)
            : this(iniFile, iniPath, null, ConstrArgs.IniObj | ConstrArgs.IniPath)
        {
        }

        /// <summary>
        /// Creates a new MixFileNameGenerator from a given ini object, with possible additional ini
        /// objects given to read the information for specific games. This overload can be used to load
        /// the data purely from embedded resources in the project.
        /// </summary>
        /// <param name="iniFile">Main ini file to open.</param>
        /// <param name="additionalInis">Dictionary of additional ini files that can be used to read the file lists of specific games.</param>
        public MixFileNameGenerator(INI iniFile, Dictionary<string, INI> additionalInis)
            : this(iniFile, null, additionalInis, ConstrArgs.IniObj | ConstrArgs.SideInis)
        {
        }

        /// <summary>
        /// Creates a new MixFileNameGenerator from given ini objects.
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
            INISection gamesSection = iniFile.Sections[GamesHeader];
            if (gamesSection == null)
            {
                throw new ArgumentException(String.Format(ParseError, GamesHeader), "iniFile");
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
                    errors.Add("No ini section found for game \"" + gameString + "\"; skipping game.");
                    continue;
                }
                // Read game info
                string[] externalFiles = (gameSection.TryGetValue(GameKeyContentInis) ?? String.Empty).Split(',', true);
                string[] typesSections = (gameSection.TryGetValue(GameKeyFileTypes) ?? String.Empty).Split(',', true);
                string[] filesSections = (gameSection.TryGetValue(GameKeyFilesSections) ?? String.Empty).Split(',', true);
                string[][] theaterInfos = GetTheaterInfo(gameSection, GameKeyTheaters, true);
                string[][] modTheaterInfos = GetTheaterInfo(gameSection, GameKeyModTheaters, false);
                string hasher = gameSection.TryGetValue(GameKeyHasher);
                if (hasher == null)
                {
                    errors.Add("No hash method defined for game definition \"" + gameString + "\"; skipping game.");
                    continue;
                }
                bool newMixFormat = YesNoBooleanTypeConverter.Parse(gameSection.TryGetValue(GameKeyNewMixFormat));
                bool hasMixNesting = YesNoBooleanTypeConverter.Parse(gameSection.TryGetValue(GameKeyHasMixNesting));

                if (!hashMethods.TryGetValue(hasher, out HashMethod hashMethod))
                {
                    errors.Add("Coukd not found a hash method for identifier \"" + hasher + "\" for game definition \"" + gameString + "\"; skipping game.");
                    continue;
                }
                if (filesSections.All(fs => String.IsNullOrEmpty(fs)))
                {
                    errors.Add("No files sections defined for game definition \"" + gameString + "\"; skipping game.");
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
                        try
                        {
                            string filesListPath = Path.Combine(Path.GetDirectoryName(iniPath), ini);
                            if (File.Exists(filesListPath))
                            {
                                extraIni = new INI();
                                using (TextReader reader = new StreamReader(filesListPath, Encoding.GetEncoding(437)))
                                {
                                    extraIni.Parse(reader);
                                }
                                // If anything fails in this, the ini is not added.
                                gameIniFiles.Add(extraIni);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add("Error reading \"" + ini + "\": " + ex.Message);
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
                    bool found = false;
                    foreach (INI ini in gameIniFiles)
                    {
                        INISection gameFilesSection = ini.Sections[filesList];
                        if (gameFilesSection == null)
                        {
                            continue;
                        }
                        found = true;
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
                    if (!found)
                    {
                        errors.Add("Files section \"" + filesList + "\" for game definition \"" + gameString + "\" not found.");
                    }
                }
                // don't add games with zero files.
                if (gameFiles.Count == 0)
                {
                    errors.Add("No filenames found for game definition \"" + gameString + "\"; skipping.");
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
                gd.GenerateNameIds(errors);
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
                        string info = typeSection.TryGetValue(nameIndex.ToString() + InfoSuffix);
                        generators.Add(new FileNameGeneratorEntry(nameVal, info, nameIndex, typeString, errors));
                        nameIndex++;
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
                return generateDummy ? new string[][] { new[] { String.Empty } } : null;
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
        /// Identify the files inside a mix file.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        /// <returns>The detected game type.</returns>
        public string IdentifyMixFile(MixFile mixFile)
        {
            HashMethod forcedHasher = null;
            return IdentifyMixFile(mixFile, null, ref forcedHasher);
        }

        /// <summary>
        /// Identify the files inside a mix file, using a specific game.
        /// Generally used for embedded mix files, using the parent's game type.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        /// <param name="forcedGameType">Game type identified on the parent; is always taken as primary type.</param>
        /// <returns>The detected game type.</returns>
        public string IdentifyMixFile(MixFile mixFile, string forcedGameType)
        {
            HashMethod forcedHasher = null;
            return IdentifyMixFile(mixFile, forcedGameType, ref forcedHasher);
        }

        /// <summary>
        /// Identify the files inside a mix file, using a specific hasher.
        /// Generally used for embedded mix files, using the parent's detected hasher.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        /// <param name="forcedHasher">
        ///     If no specific game type was identified, but a hasher was, it can be force through this.
        ///     At the end of this function, this object will always contain the used hasher.
        /// </param>
        /// <returns>The detected game type.</returns>
        public string IdentifyMixFile(MixFile mixFile, ref HashMethod forcedHasher)
        {
            return IdentifyMixFile(mixFile, null, ref forcedHasher);
        }

        /// <summary>
        /// Identify the files inside a mix file, and return the identifier for the detected game.
        /// </summary>
        /// <param name="mixFile">mix file to insert the name info into.</param>
        /// <param name="forcedGameType">Game type identified on the parent; is always taken as primary type.</param>
        /// <param name="forcedHasher">
        ///     If no specific game type was identified, but a hasher was, it can be force through this.
        ///     At the end of this function, this object will always contain the used hasher.
        /// </param>
        /// <returns>The game type identified for this mix file.</returns>
        public string IdentifyMixFile(MixFile mixFile, string forcedGameType, ref HashMethod forcedHasher)
        {
            double maxRatio = 0;
            GameDefinition maxGame = null;
            GameDefinition forced = null;
            if (forcedGameType != null)
            {
                gameInfo.TryGetValue(forcedGameType, out forced);
                if (forced != null)
                {
                    forcedHasher = forced.Hasher;
                }
            }
            HashMethod dbFileHasher = null;
            Dictionary<uint, MixEntry> dbFileInfo = GetRaMixDatabaseInfo(mixFile, out dbFileHasher);
            if (dbFileInfo == null || (forcedHasher != null && dbFileHasher != forcedHasher))
            {
                dbFileInfo = GetXccDatabaseInfo(mixFile, out dbFileHasher);
                if (forcedHasher != null && dbFileHasher != null && dbFileHasher != forcedHasher)
                {
                    dbFileInfo = null;
                    dbFileHasher = null;
                }
            }
            // A successful db file detection that conflicts with the forced game type will override it.
            if (forcedHasher != null && dbFileHasher != null && forced.Hasher.GetType() != dbFileHasher.GetType())
            {
                forced = null;
                forcedHasher = null;
            }
            if (forcedHasher == null)
            {
                forcedHasher = dbFileHasher;
            }
            Dictionary<string, double> identifiedAmounts = new Dictionary<string, double>();
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
                    Type hasherType = gd.Hasher.GetType();
                    // Follow the lead of xcc detection or forced type.
                    if (forcedHasher != null && hasherType != forcedHasher.GetType())
                    {
                        continue;
                    }
                    // If forced is not null, only detect the games with that one's hasher.
                    if (forced != null && hasherType != forced.Hasher.GetType())
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
            if (maxGame == null && (dbFileInfo == null || forcedHasher == null))
            {
                return null;
            }
            // Select all game definition, in order of identification, that have the same hasher algorithm as the top identified one.
            HashMethod mh = maxGame != null ? maxGame.Hasher : forcedHasher;
            Type mhType = mh.GetType();
            List<GameDefinition> viableGames = identifiedAmounts.Keys
                .OrderByDescending(gt => identifiedAmounts[gt])
                .Select(gn => gameInfo[gn])
                .Where(gd => mhType == gd.Hasher.GetType())
                .ToList();
            if (replaceGame)
            {
                // Move forced type to first place.
                viableGames.Remove(forced);
                viableGames.Insert(0, forced);
            }
            Dictionary<uint, MixEntry> info = dbFileInfo != null ? new Dictionary<uint, MixEntry>(dbFileInfo) : new Dictionary<uint, MixEntry>();
            foreach (GameDefinition gd in viableGames)
            {
                foreach (MixEntry entry in gd.FileInfo)
                {
                    MixEntry existing;
                    if (!info.TryGetValue(entry.Id, out existing))
                    {
                        info.Add(entry.Id, entry);
                    }
                    else if (existing.Type == MixContentType.DbTmp && String.Equals(existing.Name, entry.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Name matches; fill in description if db info didn't give it.
                        if (String.IsNullOrEmpty(existing.Description))
                        {
                            existing.Description = entry.Description;
                        }
                        existing.Type = MixContentType.Unknown;
                    }
                }
            }
            // Clear remaining temp mixcontent identifiers.
            if (dbFileInfo != null)
            {
                foreach (uint dbKey in dbFileInfo.Keys)
                {
                    if (info.TryGetValue(dbKey, out MixEntry dbEntry) && dbEntry.Type == MixContentType.DbTmp)
                    {
                        dbEntry.Type = MixContentType.Unknown;
                    }
                }

            }
            mixFile.InsertInfo(info, mh, true);
            if (forcedHasher == null && maxGame != null)
            {
                forcedHasher = maxGame.Hasher;
            }
            return maxGame?.Name;
        }

        /// <summary>
        /// Identify a file inside the whole range of known filenames.
        /// </summary>
        /// <param name="id">id to identify.</param>
        /// <returns>All known files matching the given ID.</returns>
        public List<MixEntry> IdentifySingleFile(uint id)
        {
            List<MixEntry> matches = new List<MixEntry>();
            foreach (string game in games)
            {
                GameDefinition gameDef = gameInfo[game];
                matches.AddRange(gameDef.FileInfo.Where(fi => fi.Id == id));
            }
            return matches;
        }

        private Dictionary<uint, MixEntry> GetXccDatabaseInfo(MixFile mixFile, out HashMethod hm)
        {
            const string xccFileName = "local mix database.dat";
            const int xccHeaderLength = 0x34;
            const uint maxProcessed = 0x500000;
            List<uint> filesList = mixFile.FileIds.ToList();
            // Could make this more robust / overengineered by returning all hashers for which a valid one was found?
            foreach (HashMethod hasher in HashMethod.GetRegisteredMethods())
            {
                uint xccId = hasher.GetNameId(xccFileName);
                // Check if there's an xcc filenames database.
                foreach (uint fileId in filesList)
                {
                    MixEntry[] entries = mixFile.GetFullFileInfo(fileId);
                    if (entries == null)
                    {
                        continue;
                    }
                    MixEntry dbEntry = entries[0];
                    if (fileId != xccId || dbEntry.Length >= maxProcessed || dbEntry.Length <= xccHeaderLength)
                    {
                        continue;
                    }
                    try
                    {
                        bool isXccHeader;
                        using (Stream fileStream = mixFile.OpenFile(dbEntry))
                        {
                            isXccHeader = MixContentAnalysis.IdentifyXccNames(fileStream, dbEntry);
                        }
                        if (!isXccHeader)
                        {
                            continue;
                        }
                        byte[] fileContents = mixFile.ReadFile(dbEntry);
                        MixEntry xccEntry = new MixEntry(xccId, xccFileName, "XCC filenames database");
                        xccEntry.Type = MixContentType.XccNames;
                        xccEntry.Info = dbEntry.Info;
                        // Confirmed to be an xcc names file. Now read it.
                        Dictionary<uint, MixEntry> xccInfoFilenames = new Dictionary<uint, MixEntry>();
                        xccInfoFilenames.Add(xccEntry.Id, xccEntry);
                        int readOffs = 0x34;
                        int fileSize = (int)dbEntry.Length;
                        while (readOffs < fileSize)
                        {
                            int endOffs;
                            for (endOffs = readOffs; endOffs < fileSize && fileContents[endOffs] != 0; ++endOffs) ;
                            string filename = Encoding.ASCII.GetString(fileContents, readOffs, endOffs - readOffs);
                            readOffs = endOffs + 1;
                            uint hashForCurrent = hasher.GetNameId(filename);
                            if (!xccInfoFilenames.ContainsKey(hashForCurrent))
                            {
                                MixEntry identified = new MixEntry(hashForCurrent, filename, null);
                                identified.Type = MixContentType.DbTmp;
                                xccInfoFilenames.Add(hashForCurrent, identified);
                            }
                        }
                        hm = hasher;
                        return xccInfoFilenames;
                    }
                    catch (Exception)
                    {
                        /* ignore */
                    }
                }
            }
            hm = null;
            return null;
        }

        private Dictionary<uint, MixEntry> GetRaMixDatabaseInfo(MixFile mixFile, out HashMethod hm)
        {
            const uint maxProcessed = 0x500000;
            const int entrySize = 0x44;
            const int fullHeaderSize = 0x108;
            const int minSize = fullHeaderSize + entrySize;
            List<uint> filesList = mixFile.FileIds.ToList();
            uint raMixId = 0x7FFFFFFF;
            // Could make this more robust / overengineered by returning all hashers for which a valid one was found?
            bool wrongHasher;
            foreach (HashMethod hasher in HashMethod.GetRegisteredMethods())
            {
                wrongHasher = false;
                // Check if there's an xcc filenames database.
                foreach (uint fileId in filesList)
                {
                    MixEntry[] entries = mixFile.GetFullFileInfo(fileId);
                    if (entries == null)
                    {
                        continue;
                    }
                    MixEntry dbEntry = entries[0];
                    if (fileId != raMixId || dbEntry.Length >= maxProcessed || dbEntry.Length < minSize)
                    {
                        continue;
                    }
                    try
                    {
                        bool isRaMixHeader;
                        using (Stream fileStream = mixFile.OpenFile(dbEntry))
                        {
                            isRaMixHeader = MixContentAnalysis.IdentifyRaMixNames(fileStream, dbEntry);
                        }
                        if (!isRaMixHeader)
                        {
                            continue;
                        }
                        byte[] fileContents = mixFile.ReadFile(dbEntry);
                        MixEntry raMixEntry = new MixEntry(raMixId, null, "RAMIX filenames database");
                        raMixEntry.Type = MixContentType.RaMixNames;
                        raMixEntry.Info = dbEntry.Info;
                        // Confirmed to be an RAMIX names file. Now read it.
                        Dictionary<uint, MixEntry> raMixInfoFilenames = new Dictionary<uint, MixEntry>();
                        raMixInfoFilenames.Add(raMixEntry.Id, raMixEntry);
                        int offs = 0x102;
                        int unkn1 = fileContents[offs + 0x00] | (fileContents[offs + 0x01] << 8);
                        offs += 2;
                        int unkn2 = fileContents[offs + 0x00] | (fileContents[offs + 0x01] << 8) | (fileContents[offs + 0x02] << 16) | (fileContents[offs + 0x03] << 24);
                        offs += 4;
                        int files = fileContents[2] | (fileContents[3] << 8);
                        offs = fullHeaderSize;
                        for (int i = 0; i < files; ++i)
                        {
                            uint id = (uint)(fileContents[offs + 0x00] | (fileContents[offs + 0x01] << 8) | (fileContents[offs + 0x02] << 16) | (fileContents[offs + 0x03] << 24));
                            int descLen = Math.Min(50, (int)fileContents[offs + 0x04]);
                            string description = Encoding.ASCII.GetString(fileContents, offs + 0x05, descLen).TrimEnd('\0');
                            int nameLen = Math.Min(12, (int)fileContents[offs + 0x37]);
                            string filename = Encoding.ASCII.GetString(fileContents, offs + 0x38, nameLen).TrimEnd('\0').ToLowerInvariant();
                            uint hashForCurrent = hasher.GetNameId(filename);
                            if (id != hashForCurrent)
                            {
                                wrongHasher = true;
                                break;
                            }
                            if (!raMixInfoFilenames.ContainsKey(hashForCurrent) && id == hashForCurrent)
                            {
                                MixEntry identified = new MixEntry(hashForCurrent, filename, description);
                                identified.Type = MixContentType.DbTmp;
                                raMixInfoFilenames.Add(hashForCurrent, identified);
                            }
                            offs += entrySize;
                        }
                        if (wrongHasher)
                        {
                            break;
                        }
                        hm = hasher;
                        return raMixInfoFilenames;
                    }
                    catch (Exception)
                    {
                        /* ignore */
                    }
                }
            }
            hm = null;
            return null;
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
            private static readonly Regex idOnlyEntry = new Regex("^\\*([0-9A-F]+)\\*$", RegexOptions.Compiled);

            public GameDefinition(string name)
            {
                this.Name = name;
            }

            /// <summary>
            /// Generates the full list of name IDs for this game definition.
            /// </summary>
            /// <param name="errors">If given, errors are added in this list.</param>
            public void GenerateNameIds(List<string> errors)
            {
                HashSet<uint> added = new HashSet<uint>();
                List<MixEntry> finalEntries = new List<MixEntry>();
                // Skip any duplicates when storing them.
                foreach (MixEntry entry in GetNameIds(errors))
                {
                    uint id = entry.Id;
                    if (added.Contains(id))
                    {
                        continue;
                    }
                    added.Add(id);
                    finalEntries.Add(entry);
#if DEBUG
                    // For testing
                    //System.Diagnostics.Debug.WriteLine(String.Format("{0:X8} : {1} - {2}", entry.Id, entry.Name, entry.Description ?? String.Empty));
#endif
                }
                this.FileInfo = finalEntries;
            }

            private IEnumerable<MixEntry> GetNameIds(List<string> errors)
            {
                string[][] theaterInfo = this.TheaterInfo ?? new string[][] { new[] { string.Empty } };
                string[][] theaterInfomod = this.ModTheaterInfo;
                if (this.FileInfoRaw == null || this.FileInfoRaw.Count == 0)
                {
                    yield break;
                }
                foreach (MixEntry fileInfo in GetHashInfo(this.FileInfoRaw, this.FileDescriptions, this.TypeDefinitions, theaterInfo, this.Hasher, false, errors))
                {
                    yield return fileInfo;
                }
                if (theaterInfomod != null && theaterInfomod.Length > 0)
                {
                    foreach (MixEntry fileInfo in GetHashInfo(this.FileInfoRaw, this.FileDescriptions, this.TypeDefinitions, theaterInfomod, this.Hasher, true, errors))
                    {
                        yield return fileInfo;
                    }
                }
            }

            private IEnumerable<MixEntry> GetHashInfo(IEnumerable<string> filenames, Dictionary<string, string> filenameInfo, Dictionary<string, FileNameGeneratorEntry[]> typeDefinitions,
                string[][] theaterInfo, HashMethod hashMethod, bool ignoreNonTheaterFiles, List<string> errors)
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
                        Match idMatch = idOnlyEntry.Match(name);
                        if (idMatch.Success)
                        {
                            yield return new MixEntry(UInt32.Parse(idMatch.Groups[1].Value, NumberStyles.HexNumber), null , info);
                        }
                        else
                        {
                            yield return new MixEntry(hashMethod.GetNameId(name), name, info);
                        }
                    }
                    else
                    {
                        // Fetch all filename generators for this type definition
                        if (!typeDefinitions.TryGetValue(type, out FileNameGeneratorEntry[] generators))
                        {
                            if (errors != null)
                            {
                                errors.Add("Error in filename data: no definition found for type \"" + type + "\"");
                            }
                            continue;
                        }
                        // Generate all normal filenames.
                        foreach (FileNameGeneratorEntry generator in generators)
                        {
                            // if only running for addon-theaters, skip files that don't have theater info in them.
                            if (ignoreNonTheaterFiles && !generator.IsTheaterDependent)
                            {
                                continue;
                            }
                            string fileInfo;
                            if (String.IsNullOrEmpty(generator.ExtraInfo))
                            {
                                fileInfo = info;
                            }
                            else
                            {
                                string extraInfo = generator.ExtraInfo;
                                bool hasArg0 = EnhFormatString.HasArg(extraInfo, 0);
                                if (hasArg0)
                                {
                                    int argsLength = EnhFormatString.GetHighestArg(extraInfo) + 1;
                                    object[] args = new object[argsLength];
                                    args[0] = new EnhFormatString(String.IsNullOrEmpty(info) ? string.Empty : info);
                                    // Preserve all args after {0}.
                                    for (int i = 1; i < argsLength; ++i)
                                    {
                                        args[i] = new FormatPreserveArg(i);
                                    }
                                    fileInfo = String.Format(extraInfo, args);
                                }
                                else
                                {
                                    fileInfo = (String.IsNullOrEmpty(info) ? string.Empty : (info + " ")) + generator.ExtraInfo;
                                }
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
            // Regex: \[(((\d+)-(\d+))|(((?:\([^\[\]\(\))]+\))|(?:[^\[\]]))+))\]
            // Groups:  123     4      56~
            // Group 1: full capture (not actually used; just for testing)
            // Group 2: numeric block (needs to be first to have priority over the non-numeric one since it's also valid for that format)
            // Group 3: first number in numeric part
            // Group 4: second number in numeric part
            // Group 5: non-numeric block: group containing all chunks to iterate over (not actually used; just for testing)
            // Group 6: Repeating group of the chunks to iterate over; each one is either a block surrounded with ( ), or a single character. Empty () blocks are allowed.
            //                                                         123      4       56~
            private static readonly Regex iterateRegex = new Regex("\\[(((\\d+)-(\\d+))|(((?:\\([^\\[\\]\\(\\))]*\\))|(?:[^\\[\\]]))+))\\]", RegexOptions.Compiled);
            public bool IsTheaterDependent { get; private set; }
            public int HighestArg { get; private set; }
            public string ExtraInfo { get; set; }
            private readonly string[][] iterations;

            /// <summary>
            /// Parses a definition string to generate filenames from.
            /// </summary>
            /// <param name="format">Format string</param>
            /// <param name="extraInfo">Info string</param>
            /// <param name="defName">Definition name</param>
            /// <param name="defIndex">Index of the current entry in this definition</param>
            /// <param name="parseErrors">Errors to fill</param>
            public FileNameGeneratorEntry(string format, string extraInfo, int defIndex, string defName, List<string> parseErrors)
            {
                ExtraInfo = extraInfo;
                int highestArgFormat = EnhFormatString.GetHighestArg(format);
                int highestArgInfo = EnhFormatString.GetHighestArg(extraInfo);
                // This ignores highest arg in info.
                IsTheaterDependent = highestArgFormat > 0;
                HighestArg = Math.Max(highestArgFormat, highestArgInfo);
                Match iteratorMatch = iterateRegex.Match(format);
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
                    bool isNumRange = !string.IsNullOrEmpty(iteratorMatch.Groups[2].Value);
                    if (isNumRange)
                    {
                        // Generate numeric range from $3 to $4
                        string first = iteratorMatch.Groups[3].Value;
                        string second = iteratorMatch.Groups[4].Value;
                        // If you loop from 01-999, it should do 01, 02 ... 98, 99, 100, 101 ... 998, 999.
                        int len = Math.Min(first.Length, second.Length);
                        int firstNum = Int32.Parse(first);
                        int secondNum = Int32.Parse(second);
                        if (firstNum > secondNum)
                        {
                            // Swap values
                            int tmp = firstNum;
                            firstNum = secondNum;
                            secondNum = tmp;
                        }
                        string iterateFormat = "D" + len;
                        foreach (int val in Enumerable.Range(firstNum, secondNum - firstNum + 1))
                        {
                            iterationChunks.Add(val.ToString(iterateFormat));
                        }
                    }
                    else
                    {
                        HashSet<string> iterationSet = new HashSet<string>();
                        foreach (Capture capture in iteratorMatch.Groups[6].Captures)
                        {
                            string val = capture.Value;
                            if (val.Length > 1)
                            {
                                // chop off the surrounding brackets
                                val = val.Substring(1, val.Length - 2);
                            }
                            if (!iterationSet.Contains(val))
                            {
                                iterationSet.Add(val);
                                iterationChunks.Add(val);
                            }
                            else if (parseErrors != null)
                            {
                                parseErrors.Add(String.Format("Warning: Duplicate iteration chunk \"{0}\" in entry #{1} of definition \"{2}\"", capture.Value, defIndex, defName));
                            }
                        }
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

            /// <summary>
            /// Gets all names that can be generated from this entry, along with the accompanying info string for each one.
            /// </summary>
            /// <param name="baseName">Basic name format to apply the generator on.</param>
            /// <param name="extraInfo">Extra info</param>
            /// <param name="theaterInfo">The theater names, in all configured long and short variants.</param>
            /// <returns>All different file names that can be generated from this entry, along with the accompanying description.</returns>
            public IEnumerable<(string, string)> GetNames(string baseName, string extraInfo, string[][] theaterInfo)
            {
                foreach ((string, string) name in CreateNames(baseName, extraInfo, theaterInfo, 0, new int[iterations.Length]))
                {
                    yield return name;
                }
            }

            /// <summary>
            /// This is the main workhorse, it creates new strings and formats them to output the final composed names.
            /// </summary>
            /// <param name="baseName">base name to format into the string as {0}</param>
            /// <param name="theaterInfo">Array of theater names in all their configured forms. Used to iterate over the names in case groups beyond {0} are used.</param>
            /// <param name="currentChunkPosition">The position of the entry which is replaced by new items currently.</param>
            /// <param name="chunkEntries">The current key represented as int array, to be filled ith the array of items to iterate. Its length should always match the amount in <see cref="iterations"/>.</param>
            /// <returns>The list of all names with their corresponding descriptions.</returns>
            private IEnumerable<(string, string)> CreateNames(string baseName, string extraInfo, string[][] theaterInfo, int currentChunkPosition, int[] chunkEntries)
            {
                int indexOfLastChunk = chunkEntries.Length - 1;
                int nextChunkPosition = currentChunkPosition + 1;
                int entriesLength = iterations[currentChunkPosition].Length;
                // We are looping through the full length of our array of entries to generate.
                for (int i = 0; i < entriesLength; i++)
                {
                    // The string index at the currentChunkPosition in chunkEntries will be replaced by the next one,
                    // and from these indices, a new string combination will be created using the "iterations" data.
                    chunkEntries[currentChunkPosition] = i;
                    // The method calls itself recursively until all positions of the key char array have been replaced.
                    if (currentChunkPosition < indexOfLastChunk)
                    {
                        foreach ((string, string) nameInfo in this.CreateNames(baseName, extraInfo, theaterInfo, nextChunkPosition, chunkEntries))
                        {
                            yield return nameInfo;
                        }
                        continue;
                    }
                    // This only outputs multiple entries if the item uses the theater info.
                    foreach ((string, string) generatedName in BuildString(baseName, extraInfo, theaterInfo, chunkEntries))
                    {
                        yield return generatedName;
                    }
                }
            }

            /// <summary>
            /// Builds a string out of a filled key entries list combined with the iterations info. If theater info is used, this returns one entry per theater.
            /// </summary>
            /// <param name="baseName">Base name to insert into the formatting.</param>
            /// <param name="extraInfo">Extra info for this entry; if applicable, this will be adapted to the theater.</param>
            /// <param name="theaterInfo">Array of theater names in all their configured forms. Used to iterate over the names in case groups beyond {0} are used.</param>
            /// <param name="keyEntries">Array of indices indicating which iteration to use for each chunk.</param>
            /// <returns></returns>
            private IEnumerable<(string, string)> BuildString(string baseName, string extraInfo, string[][] theaterInfo, int[] keyEntries)
            {
                int keyLength = iterations.Length;
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
