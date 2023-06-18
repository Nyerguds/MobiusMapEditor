//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
#if DEBUG
using System.Drawing.Imaging;
#endif


namespace MobiusEditor.Utility
{
    /// <summary>
    /// Blob detection class, written by Maarten Meuris, aka Nyerguds, and released under the WTFPL.
    /// Originally written for StackOverflow question https://stackoverflow.com/q/50277978/395685 but not posted there since it's a homework question.
    /// Answer link is https://stackoverflow.com/a/50282882/395685
    /// </summary>
    public static class BlobDetection
    {
        //Example code
#if DEBUG
        /// <summary>
        /// Detects darker or brighter spots on the image by brightness threshold, and returns their center points.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <param name="detectDark">Detect dark spots. False to detect bright drops.</param>
        /// <param name="brightnessThreshold">Brightness threshold needed to see a pixel as "bright".</param>
        /// <param name="mergeThreshold">The found spots are merged based on their square bounds. This is the amount of added pixels when checking these bounds. Use -1 to disable all merging.</param>
        /// <returns>A list of points indicating the centers of all found spots.</returns>
        public static List<Point> FindPoints(Bitmap image, Boolean detectDark, Single brightnessThreshold, Int32 mergeThreshold)
        {
            List<List<Point>> blobs = FindBlobs(image, detectDark, brightnessThreshold, mergeThreshold, true);
            return blobs.Where(b => b.Count > 0).Select(GetBlobCenter).ToList();
        }

        /// <summary>
        /// Detects darker or brighter spots on the image by brightness threshold, and returns a list of points for each spot.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <param name="detectDark">Detect dark spots. False to detect bright drops.</param>
        /// <param name="brightnessThreshold">Brightness threshold.</param>
        /// <param name="mergeThreshold">The found spots are merged based on their square bounds. This is the amount of added pixels when checking these bounds. Use -1 to disable all merging.</param>
        /// <param name="getEdgesOnly">True to make the returned lists only contain the edges of the blobs. This saves a lot of memory.</param>
        /// <returns>A list of blobs.</returns>
        public static List<List<Point>> FindBlobs(Bitmap image, Boolean detectDark, Single brightnessThreshold, Int32 mergeThreshold, Boolean getEdgesOnly)
        {
            Int32 width = image.Width;
            Int32 height = image.Height;
            // Binarization: get 32-bit data
            BitmapData sourceData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Int32 stride = sourceData.Stride;
            Byte[] data = new Byte[stride * height];
            System.Runtime.InteropServices.Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
            image.UnlockBits(sourceData);
            // Binarization: get brightness
            Single[,] brightness = new Single[height, width];
            Int32 lineOffset = 0;
            for (Int32 y = 0; y < height; ++y)
            {
                // use stride to get the start offset of each line
                Int32 offset = lineOffset;
                for (Int32 x = 0; x < width; ++x)
                {
                    // get colour
                    Byte blu = data[offset + 0];
                    Byte grn = data[offset + 1];
                    Byte red = data[offset + 2];
                    Color c = Color.FromArgb(red, grn, blu);
                    brightness[y, x] = c.GetBrightness();
                    offset += 4;
                }
                lineOffset += stride;
            }
            Func<Single[,], Int32, Int32, Boolean> clearsThreshold;
            if (detectDark)
                clearsThreshold = (imgData, yVal, xVal) => imgData[yVal, xVal] <= brightnessThreshold;
            else
                clearsThreshold = (imgData, yVal, xVal) => imgData[yVal, xVal] >= brightnessThreshold;
            if (mergeThreshold < 0)
                return FindBlobs(brightness, width, height, clearsThreshold, true, getEdgesOnly);
            else
                return FindBlobs(brightness, width, height, clearsThreshold, true, mergeThreshold, getEdgesOnly);
        }

        public static Point GetBlobCenter(List<Point> blob)
        {
            if (blob.Count == 0)
                return new Point(-1, -1);
            Rectangle bounds = GetBlobBounds(blob);
            return new Point(bounds.X + (bounds.Width - 1) / 2, bounds.Y + (bounds.Height - 1) / 2);
        }
#endif

        /// <summary>
        /// Detects a list of all blobs in the image, and merges any with bounds that intersect with each other according to the 'mergeThreshold' parameter.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="mergeThreshold">The found spots are merged based on their square bounds. This is the amount of added pixels when checking these bounds. Use -1 to disable all merging.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        public static List<List<Point>> FindBlobs<T>(T data, Int32 width, Int32 height, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Int32 mergeThreshold, Boolean getEdgesOnly)
        {
            List<Boolean[,]> inBlobs;
            Boolean[,] fullBlobs;
            List<List<Point>> blobs = FindBlobs(data, width, height, clearsThreshold, allEightEdges, getEdgesOnly, out inBlobs, out fullBlobs);
            MergeBlobs(blobs, width, height, null, mergeThreshold);
            return blobs;
        }

        /// <summary>
        /// Detects a list of all blobs in the image, and merges any with bounds that intersect with each other according to the 'mergeThreshold' parameter.
        /// Returns the results as Boolean[,] arrays.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="mergeThreshold">The found spots are merged based on their square bounds. This is the amount of added pixels when checking these bounds. Use -1 to disable all merging.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        public static List<Boolean[,]> FindBlobsAsBooleans<T>(T data, Int32 width, Int32 height, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Int32 mergeThreshold, Boolean getEdgesOnly)
        {
            List<Boolean[,]> inBlobs;
            Boolean[,] fullBlobs;
            List<List<Point>> blobs = FindBlobs(data, width, height, clearsThreshold, allEightEdges, getEdgesOnly, out inBlobs, out fullBlobs);
            MergeBlobs(blobs, width, height, inBlobs, mergeThreshold);
            return inBlobs;
        }

        /// <summary>
        /// Detects a list of all blobs connected to the points in the toCheck list. Does no merging.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="toCheck">List of points to check.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        public static List<List<Point>> FindBlobs<T>(T data, Int32 width, Int32 height, Point[] toCheck, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Boolean getEdgesOnly)
        {
            List<Boolean[,]> inBlobs;
            Boolean[,] fullBlobs;
            List<List<Point>> blobs = FindBlobs(data, width, height, toCheck, clearsThreshold, allEightEdges, getEdgesOnly, out inBlobs, out fullBlobs);
            return blobs;
        }

        /// <summary>
        /// Detects a list of all blobs connected to the points in the toCheck list. Does no merging.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="toCheck">List of points to check.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        /// <param name="inBlobs">Output parameter for receiving the blobs as boolean[,] arrays.</param>
        /// <param name="inAnyBlob">Output parameter for receiving all points in all the blobs as single boolean[,] array.</param>
        /// <returns>The list of blobs, as list of list of points</returns>
        public static List<List<Point>> FindBlobs<T>(T data, Int32 width, Int32 height, Point[] toCheck, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Boolean getEdgesOnly, out List<Boolean[,]> inBlobs, out Boolean[,] inAnyBlob)
        {
            List<List<Point>> blobs = new List<List<Point>>();
            inAnyBlob = new Boolean[height, width];
            inBlobs = new List<Boolean[,]>();
            for (Int32 p = 0; p < toCheck.Length; ++p)
            {
                Point pt = toCheck[p];
                Boolean[,] inBlob;
                List<Point> newBlob = MakeBlobForPoint(pt.X, pt.Y, data, width, height, clearsThreshold, allEightEdges, getEdgesOnly, inAnyBlob, out inBlob);
                if (newBlob == null)
                    continue;
                blobs.Add(newBlob);
                inBlobs.Add(inBlob);
            }
            return blobs;
        }

        /// <summary>
        /// Detects a list of all blobs in the image. Does no merging.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        public static List<List<Point>> FindBlobs<T>(T data, Int32 width, Int32 height, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Boolean getEdgesOnly)
        {
            List<Boolean[,]> inBlobs;
            Boolean[,] fullBlobs;
            List<List<Point>> blobs = FindBlobs(data, width, height, clearsThreshold, allEightEdges, getEdgesOnly, out inBlobs, out fullBlobs);
            return blobs;
        }

        /// <summary>
        /// Detects a list of all blobs in the image, returning both the blobs and the boolean representations of the blobs. Does no merging.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in.</typeparam>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="getEdgesOnly">True to make the lists in 'blobs' only contain the edge points of the blobs. The 'inBlobs' items will still have all points marked.</param>
        /// <param name="inBlobs">Output parameter for receiving the blobs as boolean[,] arrays.</param>
        /// <param name="inAnyBlob">Output parameter for receiving all points in all the blobs as single boolean[,] array.</param>
        /// <returns>The list of blobs, as list of list of points</returns>
        public static List<List<Point>> FindBlobs<T>(T data, Int32 width, Int32 height, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Boolean getEdgesOnly, out List<Boolean[,]> inBlobs, out Boolean[,] inAnyBlob)
        {
            List<List<Point>> blobs = new List<List<Point>>();
            inAnyBlob = new Boolean[height, width];
            inBlobs = new List<Boolean[,]>();
            for (Int32 y = 0; y < height; ++y)
            {
                for (Int32 x = 0; x < width; ++x)
                {
                    Boolean[,] inBlob;
                    List<Point> newBlob = MakeBlobForPoint(x, y, data, width, height, clearsThreshold, allEightEdges, getEdgesOnly, inAnyBlob, out inBlob);
                    if (newBlob == null)
                        continue;
                    blobs.Add(newBlob);
                    inBlobs.Add(inBlob);
                }
            }
            return blobs;
        }

        /// <summary>
        /// Merge any blobs that fall in each other's square bounds, to reduce the amount of stray pixels.
        /// Bounds are inflated by the amount of pixels specified in mergeThreshold.
        /// </summary>
        /// <param name="blobs">The collection of blobs. The objects in this are adapted.</param>
        /// <param name="width">width of full image. Use -1 to detect from blob bounds.</param>
        /// <param name="height">Height of full image. Use -1 to detect from blob bounds.</param>
        /// <param name="inBlobs">Boolean arrays that contain whether pixels are in a blob. If not null, these are adapted too.</param>
        /// <param name="mergeThreshold">The found blobs are merged based on their square bounds. This is the amount of added pixels when checking these bounds. Use -1 to disable all merging.</param>
        public static void MergeBlobs(List<List<Point>> blobs, Int32 width, Int32 height, List<Boolean[,]> inBlobs, Int32 mergeThreshold)
        {
            if (width == -1 || height == -1)
            {
                width = -1;
                height = -1;
                Int32 nrOfBlobs = blobs.Count;
                for (Int32 i = 0; i < nrOfBlobs; ++i)
                {
                    List<Point> blob = blobs[i];
                    Int32 nrOfPoints = blob.Count;
                    for (Int32 j = 0; j < nrOfPoints; ++j)
                    {
                        Point point = blob[j];
                        Int32 pointX = point.X;
                        Int32 pointY = point.Y;
                        if (width < pointX)
                            width = pointX;
                        if (height < pointY)
                            height = pointY;
                    }
                }
                // because width and height are sizes, not highest coordinates.
                width++;
                height++;
            }
            Boolean continueMerge = mergeThreshold >= 0;
            List<Rectangle> collBounds = new List<Rectangle>();
            List<Rectangle> collBoundsInfl = new List<Rectangle>();
            Rectangle imageBounds = new Rectangle(0, 0, width, height);
            Int32 blobsCount = blobs.Count;
            if (continueMerge)
            {
                for (Int32 i = 0; i < blobsCount; ++i)
                {
                    Rectangle rect = GetBlobBounds(blobs[i]);
                    collBounds.Add(rect);
                    Rectangle rectInfl = Rectangle.Inflate(rect, mergeThreshold, mergeThreshold);
                    collBoundsInfl.Add(Rectangle.Intersect(imageBounds, rectInfl));
                }
            }
            while (continueMerge)
            {
                continueMerge = false;
                for (Int32 i = 0; i < blobsCount; ++i)
                {
                    List<Point> blob1 = blobs[i];
                    if (blob1.Count == 0)
                        continue;
                    Boolean[,] inBlob1 = inBlobs == null ? null : inBlobs[i];
                    Rectangle checkBounds = collBoundsInfl[i];
                    for (Int32 j = 0; j < blobsCount; ++j)
                    {
                        if (i == j)
                            continue;
                        List<Point> blob2 = blobs[j];
                        Int32 blob2Count = blob2.Count;
                        if (blob2Count == 0)
                            continue;
                        // collBounds corresponds to blobs in length.
                        Rectangle bounds2 = collBounds[j];
                        if (!checkBounds.IntersectsWith(bounds2))
                            continue;
                        // should be safe without checks; there are already
                        // checks against duplicates in these collections.
                        continueMerge = true;
                        blob1.AddRange(blob2);
                        // Mark all points on the ref to the inBlobs[i] boolean array. Easier to use the points list for this instead of the second inBlobs array.
                        if (inBlob1 != null)
                        {
                            for (Int32 k = 0; k < blob2Count; ++k)
                            {
                                Point p = blob2[k];
                                inBlob1[p.Y, p.X] = true;
                            }
                        }
                        Rectangle rect1New = GetBlobBounds(blob1);
                        collBounds[i] = rect1New;
                        Rectangle rect1NewInfl = Rectangle.Inflate(rect1New, mergeThreshold, mergeThreshold);
                        collBoundsInfl[i] = Rectangle.Intersect(imageBounds, rect1NewInfl);
                        blob2.Clear();
                        // don't bother clearing inBlob2 or colbounds[j]; they don't get referenced anymore,
                        // and the cleared blob's boolean array gets filtered out at the end.
                    }
                }
            }
            // Filter out removed entries.
            Int32[] nonEmptyIndices = Enumerable.Range(0, blobsCount).Where(i => blobs[i].Count > 0).ToArray();
            Int32 nrOfNonEmpty = nonEmptyIndices.Length;
            // Nothing to remove.
            if (nrOfNonEmpty == blobsCount)
                return;
            if (inBlobs != null)
            {
                List<Boolean[,]> trimmedInBlobs = new List<Boolean[,]>();
                for (Int32 i = 0; i < nrOfNonEmpty; ++i)
                    trimmedInBlobs.Add(inBlobs[nonEmptyIndices[i]]);
                inBlobs.Clear();
                inBlobs.AddRange(trimmedInBlobs);
            }
            List<List<Point>> trimmedBlobs = new List<List<Point>>();
            for (Int32 i = 0; i < nrOfNonEmpty; ++i)
                trimmedBlobs.Add(blobs[nonEmptyIndices[i]]);
            blobs.Clear();
            blobs.AddRange(trimmedBlobs);
        }

        /// <summary>
        /// If the current point clears the threshold, and is not already in the current blobs, builds a list of all points adjacent to the current point.
        /// Loop this over every pixel of an image to detect all blobs.
        /// </summary>
        /// <typeparam name="T">Type of the list to detect equal neighbours in. This system allows any kind of data to be taken as input.</typeparam>
        /// <param name="pointX">X-coordinate of the current point.</param>
        /// <param name="pointY">Y-coordinate of the current point.</param>
        /// <param name="data">Image data array. It is processed as one pixel per coordinate.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="clearsThreshold">Function to check if the pixel at the given coordinates clears the threshold. Should be of the format (imgData, yVal, xVal) => Boolean.</param>
        /// <param name="allEightEdges">When scanning for pixels to add to the blob, scan all eight surrounding pixels rather than just top, left, bottom, right.</param>
        /// <param name="getEdgesOnly">True to make a blob containing only the edge points. The 'inBlob' and 'inAnyBlob' arrays will still have all points marked.</param>
        /// <param name="inAnyBlob">array of booleans with true values for the coordinates of all points already added to blobs. If not null, this is checked, and will be updated with the new added points.</param>
        /// <param name="inBlob">array of booleans with true values for the coordinates that are in the returned blob.</param>
        /// <returns>A list containing all points in the new blob, or null if the point was either already in <see cref="inAnyBlob"/>, or the given point itself does not pass <see cref="clearsThreshold"/>. Can be null.</returns>
        public static List<Point> MakeBlobForPoint<T>(Int32 pointX, Int32 pointY, T data, Int32 width, Int32 height, Func<T, Int32, Int32, Boolean> clearsThreshold, Boolean allEightEdges, Boolean getEdgesOnly, Boolean[,] inAnyBlob, out Boolean[,] inBlob)
        {
            // If the point is already in a blob, or if it doesn't clear the threshold, abort.
            if ((inAnyBlob != null && inAnyBlob[pointY, pointX]) || !clearsThreshold(data, pointY, pointX))
            {
                inBlob = null;
                return null;
            }
            // Initialize blob
            List<Point> blob = new List<Point>();
            // existence check optimisation in the form of a boolean grid that is kept synced with the points in the collection.
            inBlob = new Boolean[height, width];
            // setting up all variables to use, making sure nothing needs to be fetched inside the loops
            Point[] currentEdge = new Point[1];
            Int32 lastX = width - 1;
            Int32 lastY = height - 1;
            List<Point> nextEdge = new List<Point>();
            Boolean[,] inNextEdge = new Boolean[height, width];
            Int32 clearLen = inNextEdge.Length;
            // starting point
            currentEdge[0] = new Point(pointX, pointY);
            Int32 currentEdgeCount = currentEdge.Length;
            // Start looking.
            while (currentEdgeCount > 0)
            {
                // 1. Add current edge collection to the blob.
                // Memory-unoptimised: add all points.
                if (!getEdgesOnly)
                    blob.AddRange(currentEdge);
                for (Int32 i = 0; i < currentEdgeCount; ++i)
                {
                    Point p = currentEdge[i];
                    Int32 x = p.X;
                    Int32 y = p.Y;
                    // Mark point in boolean array for quick checks later.
                    inBlob[y, x] = true;
                    // Optimisation: keep a combined boolean array of all blobs for quick checking at the start.
                    if (inAnyBlob != null)
                        inAnyBlob[y, x] = true;
                    // Memory-optimised: add edge points only. inBlob will still contain all points.
                    if (getEdgesOnly &&
                        (x == 0 || y == 0 || x == lastX || y == lastY
                         || !clearsThreshold(data, y - 1, x)
                         || !clearsThreshold(data, y, x - 1)
                         || !clearsThreshold(data, y, x + 1)
                         || !clearsThreshold(data, y + 1, x)))
                        blob.Add(p);
                }
                // 2. Search all neighbouring pixels of the current neighbours list.
                // Set starting capacity of next edge to 2 times the amount of points in the current edge, to avoid too many resizes.
                nextEdge.Capacity = currentEdgeCount * 2;
                for (Int32 i = 0; i < currentEdgeCount; ++i)
                {
                    Point ep = currentEdge[i];
                    // 3. gets all (4 or 8) neighbouring pixels.
                    List<Point> neighbours = GetNeighbours(ep.X, ep.Y, lastX, lastY, allEightEdges);
                    Int32 neighboursCount = neighbours.Count;
                    for (Int32 j = 0; j < neighboursCount; ++j)
                    {
                        Point p = neighbours[j];
                        Int32 x = p.X;
                        Int32 y = p.Y;
                        // 4. If the point is not already in the blob or in the new edge collection, and clears the threshold, add it to the new edge collection.
                        if (inBlob[y, x] || inNextEdge[y, x] || !clearsThreshold(data, y, x))
                            continue;
                        nextEdge.Add(p);
                        inNextEdge[y, x] = true;
                    }
                }
                // 5. Replace edge collection contents with new edge collection.
                currentEdge = nextEdge.ToArray();
                currentEdgeCount = currentEdge.Length;
                nextEdge.Clear();
                Array.Clear(inNextEdge, 0, clearLen);
            }
            return blob;
        }

        /// <summary>
        /// Gets the list of neighbouring points around one point in an image that are inside the full image bounds.
        /// </summary>
        /// <param name="x">X-coordinate of the point to get neighbours of.</param>
        /// <param name="y">Y-coordinate of the point to get neighbours of.</param>
        /// <param name="lastX">Last valid X-coordinate on the image.</param>
        /// <param name="lastY">Last valid Y-coordinate on the image.</param>
        /// <param name="allEight">True to include diagonal neighbours.</param>
        /// <returns>The list of all valid neighbours around the given coordinate.</returns>
        private static List<Point> GetNeighbours(Int32 x, Int32 y, Int32 lastX, Int32 lastY, Boolean allEight)
        {
            // Init to max value to avoid constant list expand operations.
            List<Point> neighbours = new List<Point>(allEight ? 8 : 4);
            //Direct neighbours
            if (y > 0)
                neighbours.Add(new Point(x, y - 1));
            if (x > 0)
                neighbours.Add(new Point(x - 1, y));
            if (x < lastX)
                neighbours.Add(new Point(x + 1, y));
            if (y < lastY)
                neighbours.Add(new Point(x, y + 1));
            if (!allEight)
                return neighbours;
            // Diagonals.
            if (x > 0 && y > 0)
                neighbours.Add(new Point(x - 1, y - 1));
            if (x < lastX && y > 0)
                neighbours.Add(new Point(x + 1, y - 1));
            if (x > 0 && y < lastY)
                neighbours.Add(new Point(x - 1, y + 1));
            if (x < lastX && y < lastY)
                neighbours.Add(new Point(x + 1, y + 1));
            return neighbours;
        }

        public static Rectangle GetBlobBounds(List<Point> blob)
        {
            if (blob.Count == 0)
                return new Rectangle(0, 0, 0, 0);
            Int32 minX = Int32.MaxValue;
            Int32 maxX = 0;
            Int32 minY = Int32.MaxValue;
            Int32 maxY = 0;
            Int32 blobCount = blob.Count;
            for (Int32 i = 0; i < blobCount; ++i)
            {
                Point p = blob[i];
                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }
            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static List<Point> GetBlobEdgePoints(List<Point> blob, Int32 imageWidth, Int32 imageHeight)
        {
            Boolean[,] pointInList = new Boolean[imageHeight, imageWidth];
            Int32 blobCount = blob.Count;
            for (Int32 i = 0; i < blobCount; ++i)
            {
                Point p = blob[i];
                pointInList[p.Y, p.X] = true;
            }
            List<Point> edgePoints = new List<Point>();
            Int32 lastX = imageWidth - 1;
            Int32 lastY = imageHeight - 1;

            for (Int32 i = 0; i < blobCount; ++i)
            {
                Point p = blob[i];
                Int32 x = p.X;
                Int32 y = p.Y;
                // Image edge is obviously a blob edge too.
                if (x == 0 || y == 0 || x == lastX || y == lastY
                    || !pointInList[y - 1, x]
                    || !pointInList[y, x - 1]
                    || !pointInList[y, x + 1]
                    || !pointInList[y + 1, x])
                    edgePoints.Add(p);
            }
            return edgePoints;
        }
    }
}