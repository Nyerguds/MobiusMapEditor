//
// Copyright 2020 Rami Pasanen
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public class TypeListBox : ListBox
    {
        private const int MaxListBoxItemHeight = 255;

        [Category("Behavior")]
        public Image MissingThumbnail { get; set; } = SystemIcons.Error.ToBitmap();

        public IEnumerable<IBrowsableType> Types
        {
            get => Items.Cast<ListItem<IBrowsableType>>().Select(t => t.Value);
            set
            {
                DataSource = value.Select(t => new ListItem<IBrowsableType>(t, t.DisplayName)).ToArray();
                ItemHeight = Math.Min(255,Math.Max(ItemHeight, value.Max(t => (t.Thumbnail?.Height ?? MissingThumbnail.Height))));
                Invalidate();
            }
        }

        public IBrowsableType SelectedType => SelectedValue as IBrowsableType;

        public TypeListBox()
        {
            DisplayMember = "Name";
            ValueMember = "Type";
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            if (e.Index >= 0 && e.Index < Items.Count && Items[e.Index] is ListItem<IBrowsableType> listItem && listItem.Value != null)
            {
                StringFormat stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                };
                var textSize = e.Graphics.MeasureString(listItem.Label, Font, e.ItemWidth, stringFormat);
                e.ItemHeight = Math.Max((int)textSize.Height, Math.Min((int)Math.Round((listItem.Value.Thumbnail?.Height ?? MissingThumbnail.Height) *
                    Properties.Settings.Default.ObjectToolItemSizeMultiplier), MaxListBoxItemHeight));
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            e.DrawBackground();

            if (e.Index >= 0 && e.Index < Items.Count && Items[e.Index] is ListItem<IBrowsableType> listItem && listItem.Value != null)
            {
                StringFormat stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                };
                Brush textColor = e.State.HasFlag(DrawItemState.Selected) ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
                SizeF textSize = e.Graphics.MeasureString(listItem.Label, Font, e.Bounds.Width, stringFormat);
                // To int, with leniency for rounding errors.
                int textSizeInt = (int)textSize.Width + (textSize.Width % 1 < 0.01 ? 0 : 1);
                e.Graphics.DrawString(listItem.Label, Font, textColor, e.Bounds, stringFormat);
                if (!e.State.HasFlag(DrawItemState.ComboBoxEdit))
                {
                    Image thumbnail = listItem.Value.Thumbnail ?? MissingThumbnail;
                    int thumbnailWidth = Math.Min(e.Bounds.Width - textSizeInt, thumbnail.Width);
                    int thumbnailHeight = Math.Min(e.Bounds.Height, thumbnail.Height);
                    double widthRatio = (e.Bounds.Width - textSizeInt) / (double)thumbnail.Width;
                    double heightRatio = e.Bounds.Height / (double)thumbnail.Height;
                    if (heightRatio < widthRatio)
                    {
                        thumbnailWidth = (int)Math.Round(thumbnail.Width * heightRatio);
                    }
                    else
                    {
                        thumbnailHeight = (int)Math.Round(thumbnail.Height * widthRatio);
                    }
                    var thumbnailSize = new Size(thumbnailWidth, thumbnailHeight);
                    var thumbnailBounds = new Rectangle(new Point(e.Bounds.Right - thumbnailSize.Width, e.Bounds.Top), thumbnailSize);
                    e.Graphics.DrawImage(thumbnail, thumbnailBounds);
                }
            }
            e.DrawFocusRectangle();
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (MissingThumbnail != null)
                {
                    try
                    {
                        MissingThumbnail.Dispose();
                    }
                    catch
                    {
                        // Ignore.
                    }
                    MissingThumbnail = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
