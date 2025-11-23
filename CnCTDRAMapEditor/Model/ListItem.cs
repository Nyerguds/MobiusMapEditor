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
    /// Static tool class to handle <see cref="ListItem{T}"/>
    /// </summary>
    public static class ListItem
    {
        public static ListItem<T> Create<T>(T value)
        {
            return new ListItem<T>(value);
        }

        public static ListItem<T> Create<T>(T value, string Label)
        {
            return new ListItem<T>(value, Label);
        }

        public static T GetValueFromComboBox<T>(ComboBox comboBox)
        {
            return GetValueFromComboBox(comboBox, default(T));
        }

        public static T GetValueFromComboBox<T>(ComboBox comboBox, T defaultValue)
        {
            if (comboBox.SelectedItem is ListItem<T> li)
            {
                return li.Value;
            }
            return defaultValue;
        }

        public static ListItem<T> GetItemFromComboBox<T>(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ListItem<T> li)
            {
                return li;
            }
            return null;
        }

        public static ListItem<T> GetItemFromComboBoxAtIndex<T>(ComboBox comboBox, int i)
        {
            return comboBox.Items[i] as ListItem<T>;
        }

        public static T GetValueFromListBox<T>(ListBox listBox)
        {
            return GetValueFromListBox(listBox, default(T));
        }

        public static T GetValueFromListBox<T>(ListBox listBox, T defaultValue)
        {
            if (listBox.SelectedItem is ListItem<T> li)
            {
                return li.Value;
            }
            return defaultValue;
        }

        public static ListItem<T> GetItemFromListBox<T>(ListBox listBox)
        {
            if (listBox.SelectedItem is ListItem<T> li)
            {
                return li;
            }
            return null;
        }

        public static ListItem<T> GetItemFromListBoxAtIndex<T>(ListBox listBox, int i)
        {
            return listBox.Items[i] as ListItem<T>;
        }

        public static int GetIndexInList<T>(T value, ListItem<T>[] items)
        {
            return GetIndexInList<T>(value, items, -1);
        }

        public static int GetIndexInList<T>(T value, ListItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (int i = 0; i < len; ++i)
            {
                ListItem<T> item = items[i];
                if (item != null && item.Value.Equals(value))
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
                ListItem<T> item = comboBox.Items[i] as ListItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInListByLabel<T>(string label, ListItem<T>[] items)
        {
            return GetIndexInListByLabel(label, items, -1);
        }

        public static int GetIndexInListByLabel<T>(string label, ListItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (int i = 0; i < len; ++i)
            {
                ListItem<T> item = items[i];
                if (item != null && item.Label.Equals(label))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInComboBoxByLabel<T>(string label, ComboBox comboBox)
        {
            return GetIndexInComboBoxByLabel<T>(label, comboBox, -1);
        }

        public static int GetIndexInComboBoxByLabel<T>(string label, ComboBox comboBox, int defaultIndex)
        {
            for (int i = 0; i < comboBox.Items.Count; ++i)
            {
                ListItem<T> item = comboBox.Items[i] as ListItem<T>;
                if (item != null && item.Label.Equals(label))
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
        public static T CheckInList<T>(T value, IEnumerable<ListItem<T>> items)
        {
            foreach (ListItem<T> item in items)
            {
                if (item != null && item.Value.Equals(value))
                {
                    return value;
                }
            }
            ListItem<T> firstItem = items.FirstOrDefault();
            T first = firstItem == null ? default : firstItem.Value;
            return first;
        }

        /// <summary>
        /// If the value exists in the given list, return the value itself. Otherwise, return the given <paramref name="defaultValue"/>
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="items">List to check.</param>
        /// <param name="defaultValue">Value to return as default on failure.</param>
        /// <returns></returns>
        public static T CheckInList<T>(T value, IEnumerable<ListItem<T>> items, T defaultValue)
        {
            foreach (ListItem<T> item in items)
            {
                if (item != null && item.Value.Equals(value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        public static ListItem<T> FindInComboBox<T>(T value, ComboBox comboBox)
        {
            for (int i = 0; i < comboBox.Items.Count; ++i)
            {
                ListItem<T> item = comboBox.Items[i] as ListItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return item;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Class to use as items in <see cref="ComboBox"/> controls, containing
    /// both a value and an explicit string to show.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class ListItem<T> : IEquatable<ListItem<T>>, IComparable<ListItem<T>>
    {
        public string Label { get; set; }
        public T Value { get; private set; }

        public ListItem(T obj)
        {
            Value = obj;
            Label = obj.ToString();
        }

        public ListItem(T value, string label)
        {
            Value = value;
            Label = label;
        }

        public bool Equals(ListItem<T> other)
        {
            return other != null
                && ((this.Value == null && other.Value == null) || (this.Value != null && this.Value.Equals(other.Value)))
                && String.Equals(this.Label, other.Label, StringComparison.InvariantCultureIgnoreCase);
        }

        public int CompareTo(ListItem<T> other)
        {
            if (this.Equals(other))
                return 0;
            return String.Compare(this.Label ?? String.Empty, other.Label ?? String.Empty);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
