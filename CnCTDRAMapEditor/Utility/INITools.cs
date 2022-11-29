using MobiusEditor.Model;
using System;
using System.Linq;

namespace MobiusEditor.Utility
{
    public static class INITools
    {

        /// <summary>
        /// Returns whether certain ini information was found in the given ini data.
        /// </summary>
        /// <param name="ini">ini data.</param>
        /// <param name="section">Section to find.</param>
        /// <returns>True if the ini section was found.</returns>
        public static bool CheckForIniInfo(INI ini, string section)
        {
            return CheckForIniInfo(ini, section, null, null);
        }

        /// <summary>
        /// Returns whether certain ini information was found in the given ini data.
        /// </summary>
        /// <param name="ini">ini data.</param>
        /// <param name="section">Section to find.</param>
        /// <param name="key">Optional key to find. If no complete key/value pair is given, only the existence of the section will be checked.</param>
        /// <param name="value">Optional value to find. If no complete key/value pair is given, only the existence of the section will be checked.</param>
        /// <returns>True if the ini information was found.</returns>
        public static bool CheckForIniInfo(INI ini, string section, string key, string value)
        {
            INISection iniSection = ini[section];
            if (key == null || value == null)
            {
                return iniSection != null;
            }
            return iniSection != null && iniSection.Keys.Contains(key) && iniSection[key].Trim() == value;
        }

        /// <summary>
        /// Checks if the given string is a valid ini key in an ASCII context.
        /// </summary>
        /// <param name="iniKey">The key to check.</param>
        /// <param name="reservedNames">Optional array of reserved names. IF given, any entry in this list will also return false.</param>
        /// <returns>True if the given string is a valid ini key in an ASCII context.</returns>
        public static bool IsValidKey(String iniKey, params string[] reservedNames)
        {
            if (reservedNames != null)
            {
                foreach (string name in reservedNames)
                {
                    if (name.Equals(iniKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return iniKey.All(c => c > ' ' && c <= '~' && c != '=' && c != '[' && c != ']');
        }

        /// <summary>
        /// Will find a section in the ini information, parse its data into the given data object, remove all
        /// keys managed by the data object from the ini section, and, if empty, remove the section from the ini.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to read data.</param>
        /// <returns>Null if the section was not found, otherwise the trimmed section.</returns>
        public static INISection ParseAndLeaveRemainder<T>(INI ini, string name, T data, MapContext context)
        {
            var dataSection = ini.Sections[name];
            if (dataSection == null)
                return null;
            INI.ParseSection(context, dataSection, data);
            INI.RemoveHandledKeys(dataSection, data);
            if (dataSection.Keys.Count() == 0)
                ini.Sections.Remove(name);
            return dataSection;
        }

        /// <summary>
        /// Will extract a section from the ini information, add the current data to it, and re-add it
        /// at the end of the ini object. If the <see cref="shouldAdd" /> argument is false, and no section
        /// with this name is found in the current ini object, the object is not added. Otherwise it
        /// will be added, with the data object info added into it.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to write data.</param>
        /// <param name="shouldAdd">False if the object is not supposed to be added. This will be ignored if a section with that name is found that contains keys not managed by the data object.</param>
        /// <returns>Null if the section was not found, otherwise the final re-added section.</returns>
        public static INISection FillAndReAdd<T>(INI ini, string name, T data, MapContext context, bool shouldAdd)
        {
            INISection dataSection = ini.Sections.Extract(name);
            if (dataSection != null)
            {
                INI.RemoveHandledKeys(dataSection, data);
                if (dataSection.Keys.Count > 0)
                {
                    // Contains extra keys.
                    shouldAdd = true;
                }
            }
            if (!shouldAdd)
            {
                return null;
            }
            if (dataSection != null)
            {
                ini.Sections.Add(dataSection);
            }
            else
            {
                dataSection = ini.Sections.Add(name);
            }
            INI.WriteSection(context, dataSection, data);
            return dataSection;
        }

        /// <summary>
        /// Will seek a section in the ini, remove any information in it that is handled by the data object,
        /// and remove the section from the ini if no keys remain in it.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="data">Data object.</param>
        /// <param name="name">Name of the section.</param>
        public static void ClearDataFrom<T>(INI ini, string name, T data)
        {
            var basicSection = ini.Sections[name];
            if (basicSection != null)
            {
                INI.RemoveHandledKeys(basicSection, data);
                if (basicSection.Keys.Count() == 0)
                    ini.Sections.Remove(name);
            }

        }
    }
}
