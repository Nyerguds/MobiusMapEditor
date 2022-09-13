using System;
using System.Collections.Generic;
using System.Drawing;

namespace MobiusEditor.Event
{
    public class MapRefreshEventArgs : EventArgs
    {
        public ISet<Point> Points { get; private set; }

        public MapRefreshEventArgs(ISet<Point> points)
        {
            Points = points;
        }
    }
}
