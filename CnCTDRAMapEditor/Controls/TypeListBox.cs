using MobiusEditor.Interface;
using MobiusEditor.Model;
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
        [Category("Behavior")]
        public Image MissingThumbnail { get; set; } = SystemIcons.Error.ToBitmap();

        public IEnumerable<IBrowsableType> Types
        {
            get => Items.Cast<TypeItem<IBrowsableType>>().Select(t => t.Type);
            set
            {
                DataSource = value.Select(t => new TypeItem<IBrowsableType>(t.DisplayName, t)).ToArray();
                ItemHeight = Math.Max(ItemHeight, value.Max(t => (t.Thumbnail?.Height ?? MissingThumbnail.Height)));
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

            var typeItem = Items[e.Index] as TypeItem<IBrowsableType>;
            if (typeItem?.Type != null)
            {
                e.ItemHeight = (int)((typeItem.Type.Thumbnail?.Height ?? MissingThumbnail.Height) *
                    Properties.Settings.Default.ObjectToolItemSizeMultiplier);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            e.DrawBackground();

            if ((e.Index >= 0) && (e.Index < Items.Count))
            {
                var typeItem = Items[e.Index] as TypeItem<IBrowsableType>;
                if (typeItem?.Type != null)
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center
                    };

                    var textColor = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
                    var textSize = e.Graphics.MeasureString(typeItem.Name, Font, e.Bounds.Width, stringFormat);
                    e.Graphics.DrawString(typeItem.Name, Font, textColor, e.Bounds, stringFormat);

                    if ((e.State & DrawItemState.ComboBoxEdit) == DrawItemState.None)
                    {
                        var thumbnail = typeItem.Type.Thumbnail ?? MissingThumbnail;
                        var thumbnailWidth = (int)Math.Min((e.Bounds.Width - textSize.Width),
                            thumbnail.Width);
                        int thumbnailHeight = (int)Math.Min(e.Bounds.Height, thumbnail.Height);

                        double widthRatio = (e.Bounds.Width - textSize.Width) / (double)thumbnail.Width;
                        double heightRatio = e.Bounds.Height / (double)thumbnail.Height;
                        if (heightRatio < widthRatio)
                        {
                            thumbnailWidth = (int)(thumbnail.Width * heightRatio);
                        }
                        else
                        {
                            thumbnailHeight = (int)(thumbnail.Height * widthRatio);
                        }

                        var thumbnailSize = new Size(thumbnailWidth, thumbnailHeight);
                        var thumbnailBounds = new Rectangle(new Point(e.Bounds.Right - thumbnailSize.Width, e.Bounds.Top), thumbnailSize);
                        e.Graphics.DrawImage(thumbnail, thumbnailBounds);
                    }
                }
            }

            e.DrawFocusRectangle();
        }
    }
}
