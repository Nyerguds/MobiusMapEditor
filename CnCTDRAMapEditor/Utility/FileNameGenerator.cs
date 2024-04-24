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
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public class FileNameGenerator
    {
        private const string parseError = "Error parsing ini: section {0} not found.";
        private const string gamesHeader = "Games";
        private const string typesHeader = "FileTypes";

        private static readonly Dictionary<string, HashMethod> hashMethods = HashMethod.GetRegisteredMethods().ToDictionary(m => m.GetSimpleName(), StringComparer.OrdinalIgnoreCase);
        private static readonly HashMethod defaultHashMethod = HashMethod.GetRegisteredMethods().FirstOrDefault();

        private List<string> games;
        private Dictionary<string, string[]> gameFiles;
        private Dictionary<string, string[][]> gameTheaterInfo;
        private Dictionary<string, string[][]> modTheaterInfo;
        private Dictionary<string, FileNameGeneratorEntry[]> typeDefinitions;

        public List<string> Games => games?.ToList();

        public FileNameGenerator(string iniPath)
            :this(null, iniPath)
        {
        }

        public FileNameGenerator(INI iniFile)
            : this(iniFile, Path.Combine(Path.GetDirectoryName("."), "dummy.ini"))
        {
        }

        public FileNameGenerator(INI iniFile, string readPath)
        {
            games = new List<string>();
            gameFiles = new Dictionary<string, string[]>();
            gameTheaterInfo = new Dictionary<string, string[][]>();
            modTheaterInfo = new Dictionary<string, string[][]>();
            typeDefinitions = new Dictionary<string, FileNameGeneratorEntry[]>();

            bool validFile = File.Exists(readPath);
            if (iniFile == null && validFile)
            {
                iniFile = new INI();
                using (TextReader reader = new StreamReader(readPath, Encoding.GetEncoding(437)))
                {
                    iniFile.Parse(reader);
                }
            }
            INISection gamesSection = iniFile.Sections[gamesHeader];
            INISection typesSection = iniFile.Sections[typesHeader];
            if (gamesSection == null)
            {
                throw new ArgumentException(String.Format(parseError, gamesHeader), "iniFile");
            }
            if (typesSection == null)
            {
                throw new ArgumentException(String.Format(parseError, typesHeader), "iniFile");
            }
            int index = 0;
            string indexVal;
            while (!String.IsNullOrEmpty(indexVal = typesSection.TryGetValue(index.ToString())))
            {
                index++;
                INISection typeSection = iniFile.Sections[indexVal];
                if (typeSection == null)
                {
                    continue;
                }
                int nameIndex = 0;
                string nameVal;
                List<FileNameGeneratorEntry> generators = new List<FileNameGeneratorEntry>();
                while (!string.IsNullOrEmpty(nameVal = typeSection.TryGetValue(nameIndex.ToString())))
                {
                    nameIndex++;
                    generators.Add(new FileNameGeneratorEntry(nameVal));
                }
                if (generators.Count > 0)
                {
                    typeDefinitions.Add(indexVal, generators.ToArray());
                }
            }
            index = 0;
            while (!String.IsNullOrEmpty(indexVal = gamesSection.TryGetValue(index.ToString())))
            {
                index++;
                INISection gameSection = iniFile.Sections[indexVal];
                if (gameSection == null)
                {
                    continue;
                }
                string[][] theaterInfos = GetTheaterInfo(gameSection, "Theaters", true);
                string[][] modTheaterInfos = GetTheaterInfo(gameSection, "ModTheaters", false);
                string externalFile = gameSection.TryGetValue("FilesListIni");
                string filesList = gameSection.TryGetValue("FilesSection");
                if (String.IsNullOrEmpty(filesList))
                {
                    continue;
                }
                INISection gameFilesSection = null;
                if (!String.IsNullOrEmpty(externalFile))
                {
                    if (validFile)
                    {
                        INI fileListFile = new INI();
                        try
                        {
                            using (TextReader reader = new StreamReader(readPath, Encoding.GetEncoding(437)))
                            {
                                fileListFile.Parse(reader);
                            }
                        }
                        catch { /* ignore */ }
                        gameFilesSection = fileListFile.Sections[filesList];
                    }
                }
                else
                {
                    gameFilesSection = iniFile.Sections[filesList];
                }
                if (gameFilesSection == null || gameFilesSection.Count == 0)
                {
                    continue;
                }
                gameTheaterInfo.Add(indexVal, theaterInfos);
                if (modTheaterInfos != null && modTheaterInfos.Length > 0)
                {
                    modTheaterInfo.Add(indexVal, modTheaterInfos);
                }
                gameFiles.Add(indexVal, gameFilesSection.Keys.Select(kvp => kvp.Key).ToArray());
                games.Add(indexVal);
            }
        }

        private string[][] GetTheaterInfo(INISection gameSection, string keyName, bool generateDummy)
        {
            string theaters = gameSection.TryGetValue(keyName);
            if (string.IsNullOrEmpty(theaters))
            {
                return !generateDummy ? null : new string[][] { new[] { string.Empty } };
            }
            string[] theatersList = theaters.Split(',');
            string[][] theaterInfos = new string[theatersList.Length][];
            for (int i = 0; i < theatersList.Length; ++i)
            {
                theaterInfos[i] = theatersList[i].Split(':');
            }
            return theaterInfos;
        }

        public IEnumerable<KeyValuePair<uint, string>> GetAllNameIds()
        {
            foreach (string game in games)
            {
                foreach (KeyValuePair<uint, string> nameInfo in this.GetNameIds(game))
                {
                    yield return nameInfo;
                }
            }
        }

        public IEnumerable<KeyValuePair<uint, string>> GetNameIds(string game)
        {
            if (!games.Contains(game))
            {
                yield break;
            }
            string[][] theaterInfo;
            if (!gameTheaterInfo.TryGetValue(game, out theaterInfo))
            {
                theaterInfo = new string[][] { new[] { string.Empty } };
            }
            string[][] theaterInfomod;
            modTheaterInfo.TryGetValue(game, out theaterInfomod);
            string[] filenames;
            if (!gameFiles.TryGetValue(game, out filenames))
            {
                yield break;
            }
            foreach (KeyValuePair<uint, string> fileInfo in GetHashInfo(filenames, theaterInfo, false))
            {
                yield return fileInfo;
            }
            if (theaterInfomod != null && theaterInfomod.Length > 0)
            {
                foreach (KeyValuePair<uint, string> fileInfo in GetHashInfo(filenames, theaterInfomod, true))
                {
                    yield return fileInfo;
                }
            }
        }

        private IEnumerable<KeyValuePair<uint, string>> GetHashInfo(string[] filenames, string[][] theaterInfo, bool ignoreNonTheaterFiles)
        {
            foreach (string filename in filenames)
            {
                string[] fnParts = filename.Split(',');
                string name = fnParts[0].Trim();
                string hashMethod = fnParts.Length < 2 ? null : fnParts[1].Trim();
                string type = fnParts.Length < 3 ? null : fnParts[2].Trim();
                HashMethod method;
                if (!hashMethods.TryGetValue(hashMethod, out method))
                {
                    throw new Exception("Error in filename data: hash method \"" + type + "\" is unknown.");
                }
                if (String.IsNullOrEmpty(type))
                {
                    if (ignoreNonTheaterFiles)
                    {
                        continue;
                    }
                    yield return new KeyValuePair<uint, string>(method.GetNameId(name), name);
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
                        foreach (string generatedName in generator.GetNames(name, theaterInfo))
                        {
                            yield return new KeyValuePair<uint, string>(method.GetNameId(generatedName), generatedName);
                        }
                    }
                }
            }
        }

        public class FileNameGeneratorEntry
        {
            private static readonly Regex FormatRegex = new Regex("{(\\d+)(?::\\d+(?:-\\d+)?)?}", RegexOptions.Compiled);
            private static readonly Regex IterateRegex = new Regex("\\[((?:[^\\[\\]\\(\\)])|(?:\\([^\\[\\]\\(\\))]+\\)))+\\]", RegexOptions.Compiled);
            public bool IsIterator { get; private set; }
            public bool IsTheaterDependent { get; private set; }
            public int HighestArg { get; private set; }
            private string[][] iterations;

            public FileNameGeneratorEntry(string format)
            {
                Match formatMatch = FormatRegex.Match(format);
                int highestArg = -1;
                while (formatMatch.Success)
                {
                    int argNumber = Int32.Parse(formatMatch.Groups[1].Value);
                    highestArg = Math.Max(highestArg, argNumber);
                    // if any groups beyond {0} are in there, it needs to be iterated over theaters.
                    if (argNumber > 0)
                    {
                        IsTheaterDependent = true;
                    }
                    formatMatch = formatMatch.NextMatch();
                }
                HighestArg = highestArg;
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

            public IEnumerable<string> GetNames(string baseName, string[][] theaterInfo)
            {
                //foreach (string name in IterateName(baseName, new int[iterations.Length], iterations[0].Length, theaterInfo))
                //{
                //    yield return name;
                //}
                foreach (string name in CreateNames(baseName, theaterInfo, 0, new int[iterations.Length], iterations.Length, iterations.Length - 1))
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
            private IEnumerable<string> CreateNames(string baseName, string[][] theaterInfo, int currentChunkPosition, int[] chunkEntries, Int32 chunkLength, Int32 indexOfLastChunk)
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
                        foreach (string name in this.CreateNames(baseName, theaterInfo, nextCharPosition, chunkEntries, chunkLength, indexOfLastChunk))
                        {
                            yield return name;
                        }
                        continue;
                    }
                    foreach (string generatedName in BuildString(baseName, theaterInfo, chunkEntries, chunkLength))
                    {
                        yield return generatedName;
                    }
                }
            }

            private IEnumerable<string> BuildString(string baseName, string[][] theaterInfo, int[] keyEntries, int keyLength)
            {
                string[] chunks = new string[keyLength];
                for (int i = 0; i < keyLength; i++)
                {
                    chunks[i] = iterations[i][keyEntries[i]];
                }
                string format = String.Join(String.Empty, chunks);
                if (!IsTheaterDependent)
                {
                    yield return String.Format(format, (EnhFormatString)baseName);
                }
                else
                {
                    for (int i = 0; i < theaterInfo.Length; i++)
                    {
                        string[] thInfo = theaterInfo[i];
                        int thInfoLen = thInfo.Length + 1;
                        int arrLen = HighestArg + 1;
                        object[] strings = new object[arrLen];
                        strings[0] = (EnhFormatString)baseName;
                        for (int j = 1; j < arrLen; j++)
                        {
                            strings[j] = new EnhFormatString(j >= thInfoLen ? String.Empty : thInfo[j - 1]);
                        }
                        yield return String.Format(format, strings);

                    }
                }

            }
        }
    }
}
