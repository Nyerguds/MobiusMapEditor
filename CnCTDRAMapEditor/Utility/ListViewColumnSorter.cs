// This class is based on example code from the Microsoft documentation, at http://support.microsoft.com/kb/319401
// (redirect to https://learn.microsoft.com/en-gb/troubleshoot/developer/visualstudio/csharp/language-compilers/sort-listview-by-column)
// It has been modified to optionally insert specific string modifiers per column.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {

        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer defaultObjectComparer;

        /// <summary>
        /// Class constructor. Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            SortColumn = 0;
            // Initialize the sort order to 'none'
            SortOrder = SortOrder.None;
            // Initialize the CaseInsensitiveComparer object
            defaultObjectComparer = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface. It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = x as ListViewItem;
            ListViewItem listviewY = y as ListViewItem;
            // Check if the contents are null
            bool xIsNull = listviewX == null || listviewX.SubItems.Count <= SortColumn;
            bool yIsNull = listviewY == null || listviewY.SubItems.Count <= SortColumn;
            if (xIsNull && yIsNull)
            {
                return 0;
            }
            if (xIsNull)
            {
                return -1;
            }
            if (yIsNull)
            {
                return 1;
            }
            // Retrieve the actual text to compare
            string compareX = listviewX.SubItems[SortColumn].Text;
            string compareY = listviewY.SubItems[SortColumn].Text;
            // Get the comparer to use
            IComparer comparer;
            if (SpecificComparers != null && SortColumn < SpecificComparers.Length && SpecificComparers[SortColumn] != null)
            {
                comparer = SpecificComparers[SortColumn];
            }
            else
            {
                comparer = defaultObjectComparer;
            }
            // Compare the two items
            int compareResult = comparer.Compare(compareX, compareY);
            // Calculate correct return value based on object comparison
            if (SortOrder == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (SortOrder == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return -compareResult;
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
        public int SortColumn { get; set; }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// Default comparer. Exposed so it could be called as fallback in the comparers set in <see cref="SpecificComparers"/>
        /// </summary>
        public CaseInsensitiveComparer DefaultObjectComparer => defaultObjectComparer;

        /// <summary>
        /// Gets or sets specific comparers for each column. Any that are undefined will result in
        /// case-insensitive string compare.
        /// </summary>
        public Comparer<string>[] SpecificComparers { get; set; }
    }
}
