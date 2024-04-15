//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MobiusEditor.Utility
{
    static class INIHelpers
    {
        public static readonly Regex SectionRegex = new Regex(@"^\s*\[([^\]]*)\]", RegexOptions.Compiled);
        public static readonly Regex KeyValueRegex = new Regex(@"^\s*(.+?)\s*=([^;]*?)(\s*;.*)?$", RegexOptions.Compiled);
        public static readonly Regex CommentRegex = new Regex(@"^\s*(#|;)", RegexOptions.Compiled);

        public static readonly Func<INIDiffType, string> DiffPrefix = t =>
        {
            switch (t)
            {
                case INIDiffType.Added:
                    return "+";
                case INIDiffType.Removed:
                    return "-";
                case INIDiffType.Updated:
                    return "@";
            }
            return string.Empty;
        };
    }

    public class INIKeyValueCollection : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private readonly OrderedDictionary KeyValues;
        private readonly Dictionary<String, String> Comments;

        public string this[string key]
        {
            get
            {
                if (!KeyValues.Contains(key))
                {
                    throw new KeyNotFoundException(key);
                }
                return KeyValues[key] as string;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                KeyValues[key] = value;
            }
        }

        public string TryGetValue(string key)
        {
            if (!KeyValues.Contains(key))
            {
                return null;
            }
            return KeyValues[key] as string;
        }

        public INIKeyValueCollection()
        {
            KeyValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            Comments = new Dictionary<String, String>();
        }

        public int Count => KeyValues.Count;

        public bool Contains(string key) => KeyValues.Contains(key);

        public T Get<T>(string key) where T : struct
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(this[key]);
        }

        public void Set<T>(string key, T value) where T : struct
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            this[key] = converter.ConvertToString(value);
        }

        public T Get<T>(string key, TypeConverter converter) where T : struct
        {
            return (T)converter.ConvertFromString(this[key]);
        }

        public void Set<T>(string key, T value, TypeConverter converter) where T : struct
        {
            this[key] = converter.ConvertToString(value);
        }

        public string GetComment(string key)
        {
            return this.Comments.TryGetValue(key, out string value) ? value : null;
        }

        public void SetComment(string key, string value)
        {
            if (!KeyValues.Contains(key) || String.IsNullOrEmpty(value))
            {
                Comments.Remove(key);
                return;
            }
            // Comments must always start with a semicolon, which can only be preceded by whitespace.
            if (!value.TrimStart().StartsWith(";"))
            {
                value = ";" + value;
            }
            this.Comments[key] = value;
        }

        public bool Remove(string key)
        {
            if (!KeyValues.Contains(key))
            {
                return false;
            }
            KeyValues.Remove(key);
            Comments.Remove(key);
            return true;
        }

        public void Clear()
        {
            KeyValues.Clear();
            Comments.Clear();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in KeyValues)
            {
                yield return new KeyValuePair<string, string>(entry.Key as string, entry.Value as string);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RemoveWhere(Func<string, bool> keySelector)
        {
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string,string> kvp in this)
            {
                if (keySelector(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (string key in toRemove)
            {
                KeyValues.Remove(key);
                Comments.Remove(key);
            }
        }
    }

    public class INISection : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        public readonly INIKeyValueCollection Keys;

        public string Name { get; private set; }

        public int Count { get { return Keys.Count; } }

        public string this[string key] { get => Keys[key]; set => Keys[key] = value; }

        public string TryGetValue(string key)
        {
            return Keys.TryGetValue(key);
        }

        public string GetComment(string key)
        {
            return Keys.GetComment(key);
        }

        public void SetComment(string key, string value)
        {
            Keys.SetComment(key, value);
        }

        public bool Contains(string key)
        {
            return Keys.Contains(key);
        }

        public bool Empty => Keys.Count == 0;

        public INISection(string name)
        {
            Keys = new INIKeyValueCollection();
            Name = name;
        }

        public void Parse(TextReader reader)
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                Match m = INIHelpers.KeyValueRegex.Match(line);
                if (m.Success)
                {
                    string key = m.Groups[1].Value;
                    Keys[key] = m.Groups[2].Value.Trim();
                    // Comment preserves leading whitespace, so value + comment is exactly the original value.
                    Keys.SetComment(key, m.Groups[3].Value);
                }
            }
        }

        public void Parse(string iniText)
        {
            using (StringReader reader = new StringReader(iniText))
            {
                Parse(reader);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public INISection Clone()
        {
            return CloneAs(this.Name);
        }

        public INISection CloneAs(string newName)
        {
            INISection clone = new INISection(newName);
            foreach (KeyValuePair<string, string> item in Keys)
            {
                clone[item.Key] = item.Value;
            }
            return clone;
        }

        public void Clear()
        {
            Keys.Clear();
        }

        public bool Remove(string key)
        {
            return Keys.Remove(key);
        }

        public void RemoveWhere(Func<string, bool> keySelector)
        {
            Keys.RemoveWhere(keySelector);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool withComment)
        {
            List<string> lines = new List<string>(Keys.Count);
            foreach (KeyValuePair<string, string> item in Keys)
            {
                string comment = withComment ? Keys.GetComment(item.Key) : null;
                lines.Add(string.Format("{0}={1}{2}", item.Key, item.Value, comment ?? String.Empty));
            }
            return string.Join(Environment.NewLine, lines);
        }
    }

    public class INISectionCollection : IEnumerable<INISection>, IEnumerable
    {
        private readonly OrderedDictionary Sections;

        public INISection this[string name] => Sections.Contains(name) ? (Sections[name] as INISection) : null;

        public INISectionCollection()
        {
            Sections = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
        }

        public int Count => Sections.Count;

        public bool Contains(string section) => Sections.Contains(section);

        public INISection Add(string name)
        {
            if (!Sections.Contains(name))
            {
                INISection section = new INISection(name);
                Sections[name] = section;
            }
            return this[name];
        }

        public bool Replace(INISection section)
        {
            if (section == null || !Sections.Contains(section.Name))
            {
                return false;
            }
            Sections[section.Name] = section;
            return true;
        }

        public bool Rename(string sectionName, string newSectionName)
        {
            INISection section = this[sectionName];
            if (section == null || Sections.Contains(newSectionName))
            {
                return false;
            }
            Sections.Remove(sectionName);
            section = section.CloneAs(newSectionName);
            Sections[section.Name] = section;
            return true;
        }

        public bool Add(INISection section)
        {
            if ((section == null) || Sections.Contains(section.Name))
            {
                return false;
            }
            Sections[section.Name] = section;
            return true;
        }

        public void AddRange(IEnumerable<INISection> sections)
        {
            foreach (INISection section in sections)
            {
                Add(section);
            }
        }

        public bool Remove(string name)
        {
            if (!Sections.Contains(name))
            {
                return false;
            }
            Sections.Remove(name);
            return true;
        }

        public INISection Extract(string name)
        {
            if (!Sections.Contains(name))
            {
                return null;
            }
            INISection section = this[name];
            Sections.Remove(name);
            return section;
        }

        public INISectionCollection Clone()
        {
            INISectionCollection clone = new INISectionCollection();
            foreach (DictionaryEntry entry in Sections)
            {
                if (entry.Value is INISection section)
                {
                    clone.Add(section.Clone());
                }
            }
            return clone;
        }

        public IEnumerator<INISection> GetEnumerator()
        {
            foreach (DictionaryEntry entry in Sections)
            {
                yield return entry.Value as INISection;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public static bool AnyIniSectionContains(string section, params INISectionCollection[] collections)
        {
            return AnyIniSectionContains(section, (IEnumerable<INISectionCollection>)collections);
        }

        public static bool AnyIniSectionContains(string section, IEnumerable<INISectionCollection> collections)
        {
            foreach (INISectionCollection collection in collections)
            {
                if (collection != null && collection.Contains(section))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public partial class INI : IEnumerable<INISection>, IEnumerable
    {
        public readonly INISectionCollection Sections;

        public INISection this[string name] { get => Sections[name]; }

        public INI()
        {
            Sections = new INISectionCollection();
        }

        public void Parse(TextReader reader)
        {
            INISection currentSection = null;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                Match m = INIHelpers.SectionRegex.Match(line);
                if (m.Success)
                {
                    currentSection = Sections.Add(m.Groups[1].Value);
                }
                if (currentSection != null)
                {
                    if (INIHelpers.CommentRegex.Match(line).Success)
                    {
                        continue;
                    }
                    currentSection.Parse(line);
                }
            }
        }

        public void Parse(string iniText)
        {
            using (StringReader reader = new StringReader(iniText))
            {
                Parse(reader);
            }
        }

        public IEnumerator<INISection> GetEnumerator()
        {
            foreach (INISection section in Sections)
            {
                yield return section;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return this.ToString("\r\n");
        }

        public string ToString(char lineEnd)
        {
            return this.ToString(lineEnd.ToString());
        }

        public string ToString(string lineEnd)
        {
            List<string> sections = new List<string>(Sections.Count);
            foreach (INISection item in Sections)
            {
                List<string> lines = new List<string>
                {
                    string.Format("[{0}]", item.Name)
                };
                if (!item.Empty)
                {
                    lines.Add(item.ToString());
                }
                sections.Add(string.Join(lineEnd, lines));
            }
            return string.Join(lineEnd + lineEnd, sections) + lineEnd;
        }
    }

    [Flags]
    public enum INIDiffType
    {
        None = 0,
        Added = 1,
        Removed = 2,
        Updated = 4,
        AddedOrUpdated = 5
    }

    public class INISectionDiff : IEnumerable<string>, IEnumerable
    {
        public readonly INIDiffType Type;

        private readonly Dictionary<string, INIDiffType> keyDiff;

        public INIDiffType this[string key]
        {
            get
            {
                INIDiffType diffType;
                if (!keyDiff.TryGetValue(key, out diffType))
                {
                    return INIDiffType.None;
                }
                return diffType;
            }
        }

        private INISectionDiff()
        {
            keyDiff = new Dictionary<string, INIDiffType>();
            Type = INIDiffType.None;
        }

        internal INISectionDiff(INIDiffType type, INISection section)
            : this()
        {
            foreach (KeyValuePair<string, string> keyValue in section.Keys)
            {
                keyDiff[keyValue.Key] = type;
            }
            Type = type;
        }

        internal INISectionDiff(INISection leftSection, INISection rightSection)
            : this(INIDiffType.Removed, leftSection)
        {
            foreach (KeyValuePair<string, string> keyValue in rightSection.Keys)
            {
                string key = keyValue.Key;
                if (keyDiff.ContainsKey(key))
                {
                    if (leftSection[key] == rightSection[key])
                    {
                        keyDiff.Remove(key);
                    }
                    else
                    {
                        keyDiff[key] = INIDiffType.Updated;
                        Type = INIDiffType.Updated;
                    }
                }
                else
                {
                    keyDiff[key] = INIDiffType.Added;
                    Type = INIDiffType.Updated;
                }
            }

            Type = (keyDiff.Count > 0) ? INIDiffType.Updated : INIDiffType.None;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return keyDiff.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, INIDiffType> item in keyDiff)
            {
                sb.AppendLine(string.Format("{0} {1}", INIHelpers.DiffPrefix(item.Value), item.Key));
            }
            return sb.ToString();
        }
    }

    public class INIDiff : IEnumerable<string>, IEnumerable
    {
        private readonly Dictionary<string, INISectionDiff> sectionDiffs;

        public INISectionDiff this[string key]
        {
            get
            {
                if (!sectionDiffs.TryGetValue(key, out INISectionDiff sectionDiff))
                {
                    return null;
                }
                return sectionDiff;
            }
        }

        private INIDiff()
        {
            sectionDiffs = new Dictionary<string, INISectionDiff>(StringComparer.OrdinalIgnoreCase);
        }

        public INIDiff(INI leftIni, INI rightIni)
            : this()
        {
            foreach (INISection leftSection in leftIni)
            {
                sectionDiffs[leftSection.Name] = rightIni.Sections.Contains(leftSection.Name) ?
                    new INISectionDiff(leftSection, rightIni[leftSection.Name]) :
                    new INISectionDiff(INIDiffType.Removed, leftSection);
            }

            foreach (INISection rightSection in rightIni)
            {
                if (!leftIni.Sections.Contains(rightSection.Name))
                {
                    sectionDiffs[rightSection.Name] = new INISectionDiff(INIDiffType.Added, rightSection);
                }
            }

            sectionDiffs = sectionDiffs.Where(x => x.Value.Type != INIDiffType.None).ToDictionary(x => x.Key, x => x.Value);
        }

        public bool Contains(string key) => sectionDiffs.ContainsKey(key);

        public IEnumerator<string> GetEnumerator()
        {
            return sectionDiffs.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, INISectionDiff> item in sectionDiffs)
            {
                sb.AppendLine(string.Format("{0} {1}", INIHelpers.DiffPrefix(item.Value.Type), item.Key));
                using (StringReader reader = new StringReader(item.Value.ToString()))
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        sb.AppendLine(string.Format("\t{0}", line));
                    }
                }
            }
            return sb.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NonSerializedINIKeyAttribute : Attribute
    {
    }

    public partial class INI
    {
        public static void ParseSection(ITypeDescriptorContext context, INISection section, object data)
        {
            ParseSection(context, section, data, false);
        }

        public static List<(string, string)> ParseSection(ITypeDescriptorContext context, INISection section, object data, bool returnErrorsList)
        {
            List<(string, string)> errors = returnErrorsList ? new List<(string, string)>() : null;
            if (data == null)
            {
                return errors;
            }
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(data);
            IEnumerable<PropertyInfo> properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetSetMethod() != null);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<NonSerializedINIKeyAttribute>() != null)
                {
                    continue;
                }
                string iniKey = property.Name;
                if (section.Keys.Contains(iniKey))
                {
                    try
                    {
                        TypeConverter converter = propertyDescriptors.Find(iniKey, false)?.Converter ?? TypeDescriptor.GetConverter(property.PropertyType);
                        if (converter.CanConvertFrom(context, typeof(string)))
                        {
                            property.SetValue(data, converter.ConvertFromString(context, section[iniKey]));
                        }
                    }
                    catch (Exception e)
                    {
                        if (errors == null)
                            throw;
                        errors.Add((iniKey, e.Message));
                    }
                }
            }
            return errors;
        }

        public static void RemoveHandledKeys(INISection section, object data)
        {
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(data);
            IEnumerable<PropertyInfo> properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetSetMethod() != null);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<NonSerializedINIKeyAttribute>() != null)
                {
                    continue;
                }
                section.Keys.Remove(property.Name);
            }
        }

        public static void WriteSection(ITypeDescriptorContext context, INISection section, object data)
        {
            if (data == null)
            {
                return;
            }
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(data);
            IEnumerable<PropertyInfo> properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetGetMethod() != null);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<NonSerializedINIKeyAttribute>() != null)
                {
                    continue;
                }
                Object value = property.GetValue(data);
                if (property.PropertyType.IsValueType || (value != null))
                {
                    TypeConverter converter = propertyDescriptors.Find(property.Name, false)?.Converter ?? TypeDescriptor.GetConverter(property.PropertyType);
                    if (converter.CanConvertTo(context, typeof(string)))
                    {
                        section[property.Name] = converter.ConvertToString(context, value);
                    }
                }
            }
        }

        public static void ParseSection(INISection section, object data) => ParseSection(null, section, data);

        public static void WriteSection(INISection section, object data) => WriteSection(null, section, data);
    }
}
