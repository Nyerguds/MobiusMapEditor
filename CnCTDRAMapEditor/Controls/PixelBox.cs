using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    /// <summary>
    /// PictureBox with editable interpolation mode.
    /// </summary>
    public class PixelBox : PictureBox
    {
        public PixelBox()
        {
            this.InterpolationMode = InterpolationMode.NearestNeighbor;
        }

        /// <summary>
        /// Gets or sets the interpolation mode.
        /// </summary>
        /// <value>The interpolation mode.</value>
        [Category("Behavior")]
        [DefaultValue(InterpolationMode.NearestNeighbor)]
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = this.InterpolationMode;
            // docs on this are wrong; putting it to Half PREVENTS it from shifting the whole thing up and to the left by half a (zoomed) pixel.
            // Not sure if this also applies for other modes than NearestNeighbor though.
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            base.OnPaint(pe);
        }

    }
}