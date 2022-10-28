using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public static class ListItem
    {

        public static int GetIndexInList<T>(T value, ListItem<T>[] items)
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
            return -1;
        }

        public static int GetIndexInDropdown<T>(T value, ComboBox dropdown)
        {
            for (Int32 i = 0; i < dropdown.Items.Count; ++i)
            {
                ListItem<T> item = dropdown.Items[i] as ListItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
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
