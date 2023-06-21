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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public static class ListItem
    {
        public static ListItem<T> MakeListItem<T>(T value, string Label)
        {
            return new ListItem<T>(value, Label);
        }

        public static T GetValueFromDropdown<T>(ComboBox dropdown)
        {
            return GetValueFromDropdown(dropdown, default(T));
        }

        public static T GetValueFromDropdown<T>(ComboBox dropdown, T defaultValue)
        {
            if (dropdown.SelectedItem is ListItem<T> li)
            {
                return li.Value;
            }
            return defaultValue;
        }

        public static int GetIndexInList<T>(T value, ListItem<T>[] items)
        {
            return GetIndexInList<T>(value, items, -1);
        }

        public static int GetIndexInList<T>(T value, ListItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (Int32 i = 0; i < len; ++i)
            {
                ListItem<T> item = items[i];
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInDropdown<T>(T value, ComboBox dropdown)
        {
            return GetIndexInDropdown(value, dropdown, -1);
        }

        public static int GetIndexInDropdown<T>(T value, ComboBox dropdown, int defaultIndex)
        {
            for (Int32 i = 0; i < dropdown.Items.Count; ++i)
            {
                ListItem<T> item = dropdown.Items[i] as ListItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInListByLabel<T>(String label, ListItem<T>[] items)
        {
            return GetIndexInListByLabel<T>(label, items, -1);
        }

        public static int GetIndexInListByLabel<T>(String label, ListItem<T>[] items, int defaultIndex)
        {
            int len = items.Length;
            for (Int32 i = 0; i < len; ++i)
            {
                ListItem<T> item = items[i];
                if (item != null && item.Label.Equals(label))
                {
                    return i;
                }
            }
            return defaultIndex;
        }

        public static int GetIndexInDropdownByLabel<T>(String label, ComboBox dropdown)
        {
            return GetIndexInDropdownByLabel<T>(label, dropdown, -1);
        }

        public static int GetIndexInDropdownByLabel<T>(String label, ComboBox dropdown, int defaultIndex)
        {
            for (Int32 i = 0; i < dropdown.Items.Count; ++i)
            {
                ListItem<T> item = dropdown.Items[i] as ListItem<T>;
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
            T first = firstItem == null ? default(T) : firstItem.Value;
            return first;
        }

        /// <summary>
        /// If the value exists in the given list, this returns the value itself. Otherwise it returns the given <see cref="errValue"/>
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="items">List to check.</param>
        /// <param name="errValue">Value to return as default on failure.</param>
        /// <returns></returns>
        public static T CheckInList<T>(T value, IEnumerable<ListItem<T>> items, T errValue)
        {
            foreach (ListItem<T> item in items)
            {
                if (item != null && item.Value.Equals(value))
                {
                    return value;
                }
            }
            return errValue;
        }
    }

    public class ListItem<T>
    {
        public T Value { get; private set; }
        public String Label { get; private set; }

        public ListItem(T value, String label)
        {
            this.Value = value;
            this.Label = label;
        }

        public override String ToString()
        {
            return this.Label;
        }
    }
}
