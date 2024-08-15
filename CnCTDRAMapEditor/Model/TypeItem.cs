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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Model
{
    /// <summary>
    /// Static tool class to handle <see cref="TypeItem{T}"/>
    /// </summary>
    public static class TypeItem
    {
        public static TypeItem<T> MakeListItem<T>(T value, string Label)
        {
            return new TypeItem<T>(Label, value);
        }

        public static T GetValueFromComboBox<T>(ComboBox dropdown)
        {
            return GetValueFromComboBox(dropdown, default(T));
        }

        public static T GetValueFromComboBox<T>(ComboBox dropdown, T defaultValue)
        {
            if (dropdown.SelectedItem is TypeItem<T> li)
            {
                return li.Type;
            }
            return defaultValue;
        }

        public static T GetValueFromListBox<T>(ListBox listBox)
        {
            return GetValueFromListBox(listBox, default(T));
        }

        public static T GetValueFromListBox<T>(ListBox listBox, T defaultValue)
        {
            if (listBox.SelectedItem is TypeItem<T> li)
            {
                return li.Type;
            }
            return defaultValue;
        }

        public static int GetIndexInList<T>(T value, TypeItem<T>[] items)
        {
            return GetIndexInList<T>(value, items, -1);
        }

        public static int GetIndexInList<T>(T value, TypeItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (int i = 0; i < len; ++i)
            {
                TypeItem<T> item = items[i];
                if (item != null && item.Type.Equals(value))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInComboBox<T>(T value, ComboBox comboBox)
        {
            return GetIndexInComboBox(value, comboBox, -1);
        }

        public static int GetIndexInComboBox<T>(T value, ComboBox comboBox, int defaultIndex)
        {
            for (int i = 0; i < comboBox.Items.Count; ++i)
            {
                TypeItem<T> item = comboBox.Items[i] as TypeItem<T>;
                if (item != null && item.Type.Equals(value))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInListByLabel<T>(string label, TypeItem<T>[] items)
        {
            return GetIndexInListByLabel(label, items, -1);
        }

        public static int GetIndexInListByLabel<T>(string label, TypeItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (int i = 0; i < len; ++i)
            {
                TypeItem<T> item = items[i];
                if (item != null && item.Name.Equals(label))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInDropdownByLabel<T>(string label, ComboBox dropdown)
        {
            return GetIndexInDropdownByLabel<T>(label, dropdown, -1);
        }

        public static int GetIndexInDropdownByLabel<T>(string label, ComboBox dropdown, int defaultIndex)
        {
            for (int i = 0; i < dropdown.Items.Count; ++i)
            {
                TypeItem<T> item = dropdown.Items[i] as TypeItem<T>;
                if (item != null && item.Name.Equals(label))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        /// <summary>
        /// If the value exists in the given list, this returns the value itself. Otherwise it returns the first value in the list.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="items">List to check.</param>
        /// <returns></returns>
        public static T CheckInList<T>(T value, IEnumerable<TypeItem<T>> items)
        {
            foreach (TypeItem<T> item in items)
            {
                if (item != null && item.Type.Equals(value))
                {
                    return value;
                }
            }
            TypeItem<T> firstItem = items.FirstOrDefault();
            T first = firstItem == null ? default : firstItem.Type;
            return first;
        }

        /// <summary>
        /// If the value exists in the given list, return the value itself. Otherwise, return the given <paramref name="defaultValue"/>
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="items">List to check.</param>
        /// <param name="defaultValue">Value to return as default on failure.</param>
        /// <returns></returns>
        public static T CheckInList<T>(T value, IEnumerable<TypeItem<T>> items, T defaultValue)
        {
            foreach (TypeItem<T> item in items)
            {
                if (item != null && item.Type.Equals(value))
                {
                    return value;
                }
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Class to use as items in <see cref="ComboBox"/> controls, containing
    /// both a value and an explicit string to show.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class TypeItem<T> : IEquatable<TypeItem<T>>, IComparable<TypeItem<T>>
    {
        public string Name { get; private set; }
        public T Type { get; private set; }

        public TypeItem(string name, T type)
        {
            Name = name;
            Type = type;
        }

        public bool Equals(TypeItem<T> other)
        {
            return other != null
                && ((this.Type == null && other.Type == null) || this.Type.Equals(other.Type))
                && String.Equals(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int CompareTo(TypeItem<T> other)
        {
            if (this.Equals(other))
                return 0;
            return String.Compare(this.Name ?? String.Empty, other.Name ?? String.Empty);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
