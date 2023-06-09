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
    }
}