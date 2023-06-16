using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColor
    {
        /// <summary>Name of the color, used as identifier.</summary>
        string Name { get; }

        /// <summary>
        /// Apply this color to a given image.
        /// </summary>
        /// <param name="image">The image to process.</param>
        void ApplyToImage(Bitmap image);

        /// <summary>
        /// Apply this color to a given image, and output the actually-used bounds of the graphics.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="opaqueBounds">The actually-used nontransparent area of the graphics that need to be painted.</param>
        void ApplyToImage(Bitmap image, out Rectangle opaqueBounds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes">Image contents</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="bytesPerPixel">Bytes per pixel</param>
        /// <param name="stride">Stride of the image</param>
        /// <param name="opaqueBounds">Opaque bounds. If given, this might optimize the operation.</param>
        void ApplyToImage(byte[] bytes, int width, int height, int bytesPerPixel, int stride, Rectangle? opaqueBounds);
    }
}