using System;
using System.Drawing;

namespace MobiusEditor.Utility
{
    public class Tile : IDisposable
    {
        public Bitmap Image { get; private set; }

        public Rectangle OpaqueBounds { get; private set; }

        public Tile(Bitmap image, Rectangle opaqueBounds)
        {
            Image = image;
            OpaqueBounds = opaqueBounds;
        }

        public Tile(Bitmap image)
            : this(image, new Rectangle(0, 0, image.Width, image.Height))
        {
        }

        public void Dispose()
        {
            Bitmap image = this.Image;
            this.Image = null;
            try
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
            catch
            {
                // ignore.
            }

        }
    }
}
