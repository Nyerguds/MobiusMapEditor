using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class DropDownItem<T>
    {
        public T Value { get; private set; }
        public String Label { get; private set; }

        public DropDownItem(T value, String label)
        {
            this.Value = value;
            this.Label = label;
        }

        public override String ToString()
        {
            return this.Label;
        }

        public static int GetIndexInList(T value, DropDownItem<T>[] items)
        {
            for (Int32 i = 0; i < items.Count(); i++)
            {
                DropDownItem<T> item = items[i];
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
                DropDownItem<T> item = dropdown.Items[i] as DropDownItem<T>;
                if (item != null && item.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
