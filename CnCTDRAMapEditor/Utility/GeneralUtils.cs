using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MobiusEditor.Utility
{

    public class CustomComponentResourceManager : ComponentResourceManager
    {
        public CustomComponentResourceManager(Type type, string resourceName)
           : base(type)
        {
            this.BaseNameField = resourceName;
        }
    }

    public class ExplorerComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

    public static class GeneralUtils
    {

        public static bool CheckForIniInfo(String path, FileType fileType, string section, string key, string value)
        {
            try
            {
                Encoding enc = new UTF8Encoding(false, true);
                String iniContents = null;
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        String iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
                        Byte[] bytes = File.ReadAllBytes(path);
                        iniContents = enc.GetString(bytes);
                        break;
                    case FileType.MEG:
                    case FileType.PGM:
                        using (var megafile = new Megafile(path))
                        {
                            var testIniFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".ini").FirstOrDefault();
                            if (testIniFile != null)
                            {
                                using (var iniReader = new StreamReader(megafile.Open(testIniFile), enc))
                                {
                                    iniContents = iniReader.ReadToEnd();
                                }
                            }
                        }
                        break;
                }
                if (iniContents == null)
                {
                    return false;
                }
                INI checkIni = new INI();
                checkIni.Parse(iniContents);
                INISection iniSection = checkIni.Sections.Extract(section);
                if (key == null || value == null)
                {
                    return iniSection != null;
                }
                return iniSection != null && iniSection.Keys.Contains(key) && iniSection[key].Trim() == value;
            }
            catch
            {
                return false;
            }
        }

        public static String MakeNew4CharName(IEnumerable<string> currentList, string fallback, params string[] reservedNames)
        {
            string name = string.Empty;
            // generate names in a way that will never run out before some maximum is reached.
            for (int i = 'a'; i <= 'z'; ++i)
            {
                for (int j = 'a'; j <= 'z'; ++j)
                {
                    for (int k = 'a'; k <= 'z'; ++k)
                    {
                        for (int l = 'a'; l <= 'z'; ++l)
                        {
                            name = String.Concat((char)i, (char)j, (char)k, (char)l);
                            if (!currentList.Contains(name, StringComparer.InvariantCultureIgnoreCase) && !reservedNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                            {
                                return name;
                            }
                        }
                    }
                }
            }
            return fallback;
        }

        public static string TrimRemarks(string value, bool trimResult, params char[] cutFrom)
        {
            if (String.IsNullOrEmpty(value))
                return value;
            int index = value.IndexOfAny(cutFrom);
            if (index == -1)
                return value;
            value = value.Substring(0, index);
            if (trimResult)
                value = value.TrimEnd();
            return value;
        }

        public static string AddRemarks(string value, string defaultVal, Boolean trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd)
        {
            if (String.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            foreach (string val in valuesToDetect)
            {
                if ((val ?? String.Empty).Trim().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return valTrimmed + remarkToAdd;
                }
            }
            return value;
        }

        public static string FilterToExisting(string value, string defaultVal, Boolean trimSource, IEnumerable<string> existing)
        {
            if (String.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            foreach (string val in existing)
            {
                if ((val ?? String.Empty).Trim().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }
            return defaultVal;
        }

        public static StringBuilder TrimEnd(this StringBuilder sb, params char[] toTrim)
        {
            if (sb == null || sb.Length == 0) return sb;
            int i = sb.Length - 1;
            if (toTrim.Length > 0)
            {
                for (; i >= 0; --i)
                    if (!toTrim.Contains(sb[i]))
                        break;
            }
            else
            {
                for (; i >= 0; --i)
                    if (!char.IsWhiteSpace(sb[i]))
                        break;
            }
            if (i < sb.Length - 1)
                sb.Length = i + 1;
            return sb;
        }

        public static Rectangle GetBoundingBoxCenter(int imgWidth, int imgHeight, int maxWidth, int maxHeight)
        {
            double previewScaleW = (double)maxWidth / imgWidth;
            double previewScaleH = (double)maxHeight / imgHeight;
            bool maxIsW = previewScaleW < previewScaleH;
            double previewScale = maxIsW ? previewScaleW : previewScaleH;
            int width = (int)Math.Floor(imgWidth * previewScale);
            int height = (int)Math.Floor(imgHeight * previewScale);
            int offsetX = maxIsW ? 0 : (maxWidth - width) / 2;
            int offsetY = maxIsW ? (maxHeight - height) / 2 : 0;
            return new Rectangle(offsetX, offsetY, width, height);
        }
    }
}
