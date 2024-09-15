// This class is based on example code from the Microsoft documentation, at
// https://learn.microsoft.com/en-gb/troubleshoot/developer/visualstudio/csharp/language-compilers/sort-listview-by-column
// The only modification is the ability to treat columns as numeric.
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private static readonly Regex isNumber = new Regex("^-?\\d+$");
        /// <summary>
        /// Specifies the columns to sort as numbers.
        /// </summary>
        private int[] numericColumms;

        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;

        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;

        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor. Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            this.NumberColumms = new int[0];            
            // Initialize the column to '0'
            ColumnToSort = 0;
            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;
            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface. It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult = 0;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            string compareX = listviewX.SubItems[ColumnToSort].Text;
            string compareY = listviewY.SubItems[ColumnToSort].Text;
            bool sortedNumeric = false;
            if (numericColumms.Contains(ColumnToSort))
            {
                string compareXNum = String.IsNullOrEmpty(compareX) ? "0" : compareX;
                string compareYNum = String.IsNullOrEmpty(compareY) ? "0" : compareY;
                if (isNumber.IsMatch(compareXNum) && isNumber.IsMatch(compareYNum))
                {
                    int intX = Int32.Parse(compareXNum);
                    int intY = Int32.Parse(compareYNum);
                    compareResult = intX.CompareTo(intY);
                    sortedNumeric = true;
                }
            }
            if (!sortedNumeric)
            {
                compareResult = ObjectCompare.Compare(compareX, compareY);
            }

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int[] NumberColumms
        {
            get { return numericColumms.ToArray(); }
            set { numericColumms = value == null ? new int[0] : value.ToArray(); }
        }


        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            get { return ColumnToSort; }
            set { ColumnToSort = value; }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            get { return OrderOfSort; }
            set { OrderOfSort = value; }
        }

    }
}
