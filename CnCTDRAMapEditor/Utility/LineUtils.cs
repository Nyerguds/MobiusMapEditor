//  Cohen–Sutherland clipping algorithm, deigned by Danny Cohen and Ivan Sutherland.
//  Actual code taken from https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm

using System;
using System.Drawing;

namespace MobiusEditor.Utility
{

    public static class LineUtils
    {
        [Flags]
        private enum OutCode {
            Inside /**/ = 0,
            Left   /**/ = 1 << 0,
            Right  /**/ = 1 << 1,
            Bottom /**/ = 1 << 2,
            Top    /**/ = 1 << 3,
        }

        private static OutCode ComputeOutCode(double xmax, double xmin, double ymax, double ymin, double x, double y)
        {
            OutCode code = OutCode.Inside;  // initialised as being inside of clip window

            if (x < xmin)           // to the left of clip window
                code |= OutCode.Left;
            else if (x > xmax)      // to the right of clip window
                code |= OutCode.Right;
            if (y < ymin)           // below the clip window
                code |= OutCode.Bottom;
            else if (y > ymax)      // above the clip window
                code |= OutCode.Top;
            return code;
        }

        // Cohen–Sutherland clipping algorithm clips a line from
        // P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with 
        // diagonal from (xmin, ymin) to (xmax, ymax).
        public static bool CohenSutherlandLineClip(RectangleF clipBounds, ref PointF p0, ref PointF p1)
        {
            double x0 = p0.X;
            double y0 = p0.Y;
            double x1 = p1.X;
            double y1 = p1.Y;

            double xmax = clipBounds.Right;
            double xmin = clipBounds.X;
            double ymax = clipBounds.Bottom;
            double ymin = clipBounds.Y;

            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            OutCode outcode0 = ComputeOutCode(xmax, xmin, ymax, ymin, x0, y0);
            OutCode outcode1 = ComputeOutCode(xmax, xmin, ymax, ymin, x1, y1);
            bool accept = false;

            while (true)
            {
                if ((outcode0 | outcode1) == 0)
                {
                    // bitwise OR is 0: both points inside window; trivially accept and exit loop
                    accept = true;
                    break;
                }
                else if ((outcode0 & outcode1) != 0)
                {
                    // bitwise AND is not 0: both points share an outside zone (LEFT, RIGHT, TOP,
                    // or BOTTOM), so both must be outside window; exit loop (accept is false)
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    double x = 0;
                    double y = 0;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    OutCode outcodeOut = outcode1 > outcode0 ? outcode1 : outcode0;

                    // Now find the intersection point;
                    // use formulas:
                    //   slope = (y1 - y0) / (x1 - x0)
                    //   x = x0 + (1 / slope) * (ym - y0), where ym is ymin or ymax
                    //   y = y0 + slope * (xm - x0), where xm is xmin or xmax
                    // No need to worry about divide-by-zero because, in each case, the
                    // outcode bit being tested guarantees the denominator is non-zero
                    if ((outcodeOut & OutCode.Top) != 0)
                    {           // point is above the clip window
                        x = x0 + (x1 - x0) * (ymax - y0) / (y1 - y0);
                        y = ymax;
                    }
                    else if ((outcodeOut & OutCode.Bottom) != 0)
                    { // point is below the clip window
                        x = x0 + (x1 - x0) * (ymin - y0) / (y1 - y0);
                        y = ymin;
                    }
                    else if ((outcodeOut & OutCode.Right) != 0)
                    {  // point is to the right of clip window
                        y = y0 + (y1 - y0) * (xmax - x0) / (x1 - x0);
                        x = xmax;
                    }
                    else if ((outcodeOut & OutCode.Left) != 0)
                    {   // point is to the left of clip window
                        y = y0 + (y1 - y0) * (xmin - x0) / (x1 - x0);
                        x = xmin;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = ComputeOutCode(xmax, xmin, ymax, ymin, x0, y0);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = ComputeOutCode(xmax, xmin, ymax, ymin, x1, y1);
                    }
                }
            }
            if (accept)
            {
                p0 = new PointF((float)x0, (float)y0);
                p1 = new PointF((float)x1, (float)y1);
            }
            return accept;
        }
    }
}
