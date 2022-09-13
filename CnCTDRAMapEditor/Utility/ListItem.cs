using System;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
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

        public static int GetIndexInList(T value, ListItem<T>[] items)
        {
            for (Int32 i = 0; i < items.Count(); i++)
            {
                ListItem<T> item = items[i];
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetIndexInDropdown(T value, ComboBox dropdown)
        {
            for (Int32 i = 0; i < dropdown.Items.Count; i++)
            {
                ListItem<T> item = dropdown.Items[i] as ListItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
