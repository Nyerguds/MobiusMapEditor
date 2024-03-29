﻿//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiusEditor.Widgets
{
    public class MouseCellChangedEventArgs : EventArgs
    {
        public Point OldCell { get; private set; }

        public Point NewCell { get; private set; }

        public MouseButtons MouseButtons { get; private set; }

        public MouseCellChangedEventArgs(Point oldCell, Point newCell, MouseButtons mouseButtons)
        {
            OldCell = oldCell;
            NewCell = newCell;
            MouseButtons = mouseButtons;
        }
    }

    public class NavigationWidget : IWidget
    {
        private Cursor currentCursor = Cursors.Default;
        public Cursor CurrentCursor
        {
            get { return currentCursor; }
            set
            {
                currentCursor = value;
                if (mapPanel != null)
                    mapPanel.Cursor = IsDragging() ? Cursors.SizeAll : currentCursor;
            }
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);

        private Pen defaultMouseoverPen;

        private Color penColor = Color.Yellow;
        public Color PenColor
        {
            get
            {
                return penColor;
            }
            set
            {
                if (value.ToArgb() == penColor.ToArgb() && defaultMouseoverPen != null)
                {
                    return;
                }
                penColor = value;
                Pen p = this.defaultMouseoverPen;
                this.defaultMouseoverPen = new Pen(value, Math.Max(1, CellSize.Width / 16));
                if (p != null)
                {
                    try { p.Dispose(); }
                    catch { /* Ignore*/ }
                }
            }
        }

        private readonly MapPanel mapPanel;
        private bool includeNavigation;
        public Size CellSize { get; private set; }

        private Point? startScrollMouseLocation;
        private Point? startScrollFromLocation;

        public CellMetrics Metrics { get; private set; }

        /// <summary>
        /// Retrieves the currently visible bounds of the map, rounded down at the lower bounds and rounded up at the higher bounds.
        /// This is used for optimising the refreshing of map indicators, so they only get painted if visible in the currently shown area.
        /// </summary>
        public Rectangle VisibleBounds
        {
            get
            {
                Rectangle visibleArea = mapPanel.ClientToMap(mapPanel.ClientRectangle);
                int visibleTopLeftCellX = visibleArea.X / CellSize.Width;
                int visibleTopLeftCellY = visibleArea.Y / CellSize.Height;
                int visibleBottomRightCellX = visibleArea.Right / CellSize.Width + (visibleArea.Right % CellSize.Width == 0 ? 0 : 1);
                int visibleBottomRightCellY = visibleArea.Bottom / CellSize.Height + (visibleArea.Bottom % CellSize.Height == 0 ? 0 : 1);
                Rectangle visibleMapArea = new Rectangle(
                    visibleTopLeftCellX,
                    visibleTopLeftCellY,
                    Math.Max(1, visibleBottomRightCellX - visibleTopLeftCellX),
                    Math.Max(1, visibleBottomRightCellY - visibleTopLeftCellY));
                visibleMapArea.Intersect(Metrics.Bounds);
                return visibleMapArea;
            }
        }
        /// <summary>Last map cell inside the map bounds.</summary>
        public Point MouseCell { get; private set; }
        /// <summary>Cell the cursor is on, even if it is out of bounds.</summary>
        public Point ActualMouseCell { get; private set; }
        /// <summary>True if the mouse cursor is inside the map bounds</summary>
        public bool MouseInBounds { get; private set; }
        public Point ClosestMouseCellBorder { get; private set; }
        public Point MouseSubPixel { get; private set; }

        private Size mouseoverSize = new Size(1, 1);
        public Size MouseoverSize
        {
            get => mouseoverSize;
            set => mouseoverSize = value.IsEmpty ? Size.Empty : new Size(value.Width | 1, value.Height | 1);
        }

        public SizeF ZoomedCellSize
        {
            get
            {
                float mapScale = mapPanel.ClientSize.Width > mapPanel.ClientSize.Height
                    ? mapPanel.ClientSize.Width / (float)mapPanel.CameraBounds.Width
                    : mapPanel.ClientSize.Height / (float)mapPanel.CameraBounds.Height;
                return new SizeF(mapScale * mapPanel.MapImage.Width / Metrics.Width, mapScale * mapPanel.MapImage.Height / Metrics.Height);
            }
        }

        public event EventHandler<MouseCellChangedEventArgs> MouseCellChanged;
        public event EventHandler<MouseCellChangedEventArgs> BoundsMouseCellChanged;
        public event EventHandler<MouseCellChangedEventArgs> ClosestMouseCellBorderChanged;

        public NavigationWidget(MapPanel mapPanel, CellMetrics metrics, Size cellSize, bool includeNavigation)
        {
            this.mapPanel = mapPanel;
            Metrics = metrics;
            this.CellSize = cellSize;
            this.PenColor = Color.Yellow;
            this.includeNavigation = includeNavigation;
        }

        public void Refresh()
        {
            OnMouseMove(mapPanel.PointToClient(Control.MousePosition));
        }

        public bool IsDragging()
        {
            return includeNavigation && ((Control.MouseButtons & MouseButtons.Middle) != MouseButtons.None || (GetAsyncKeyState(32) & 0x8000) != 0);
        }

        private void DisableDragging()
        {
            startScrollMouseLocation = null;
            startScrollFromLocation = null;
            if (mapPanel != null)
            {
                mapPanel.Cursor = currentCursor;
                mapPanel.SuspendMouseZoom = false;
            }
        }

        private bool CheckIfDragging()
        {
            bool isDragging = IsDragging();
            if (!isDragging)
            {
                DisableDragging();
                return false;
            }
            mapPanel.SuspendMouseZoom = true;
            if (isDragging && !startScrollMouseLocation.HasValue)
            {
                startScrollMouseLocation = mapPanel.PointToClient(Control.MousePosition);
                startScrollFromLocation = mapPanel.AutoScrollPosition;
                if (mapPanel != null)
                {
                    mapPanel.Cursor = Cursors.SizeAll;
                }
                // Only return true if already dragging, not when initialising.
                return false;
            }
            return isDragging;
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            CheckIfDragging();
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            CheckIfDragging();
        }

        private void MapPanel_KeyDown(Object sender, KeyEventArgs e)
        {
            if (CheckIfDragging() || startScrollMouseLocation.HasValue)
            {
                return;
            }
        }

        private void MapPanel_KeyUp(Object sender, KeyEventArgs e)
        {
            CheckIfDragging();
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(mapPanel.PointToClient(Control.MousePosition));
        }

        private void OnMouseMove(Point location)
        {
            if (CheckIfDragging() && startScrollMouseLocation.HasValue && startScrollFromLocation.HasValue)
            {
                if (mapPanel != null)
                    mapPanel.Cursor = Cursors.SizeAll;
                Point delta = new Point(location.X - startScrollMouseLocation.Value.X, location.Y - startScrollMouseLocation.Value.Y);
                Point newScrollPos = new Point(-startScrollFromLocation.Value.X - delta.X, -startScrollFromLocation.Value.Y - delta.Y);
                // Minimize drag drift. If less than an entire zoomed pixel is moved, the system seems to bug out
                // and not scroll at all, while the control's AutoScrollPosition still registers it as a scroll.
                double pixelX = ZoomedCellSize.Width / Globals.MapTileSize.Width;
                double pixelY = ZoomedCellSize.Width / Globals.MapTileSize.Height;
                // Round down to exact zoomed pixel.
                newScrollPos.X = (int)((int)(newScrollPos.X / pixelX) * pixelX);
                newScrollPos.Y = (int)((int)(newScrollPos.Y / pixelY) * pixelY);
                if (!delta.IsEmpty)
                {
                    mapPanel.AutoScrollPosition = newScrollPos;
                }
            }
            Point newCell = GetMouseCellPosition(location, out Point subPixel);
            MouseSubPixel = subPixel;
            Point newClosestMouseCellBorder = newCell;
            if (MouseSubPixel.X >= Globals.PixelWidth / 2)
            {
                newClosestMouseCellBorder.X += 1;
            }
            if (MouseSubPixel.Y >= Globals.PixelHeight / 2)
            {
                newClosestMouseCellBorder.Y += 1;
            }
            bool mouseCellChanged = ActualMouseCell != newCell;
            bool closestChanged = ClosestMouseCellBorder != newClosestMouseCellBorder;
            if (mouseCellChanged)
            {
                bool mouseInBounds = Metrics.Contains(newCell);
                this.MouseInBounds = mouseInBounds;
                var oldCellBounds = MouseCell;
                var oldCell = ActualMouseCell;
                Point newCellBounds = newCell;
                newCellBounds.X = Math.Max(0, Math.Min(Metrics.Width - 1, newCellBounds.X));
                newCellBounds.Y = Math.Max(0, Math.Min(Metrics.Height - 1, newCellBounds.Y));
                MouseCell = newCellBounds;
                ActualMouseCell = newCell;
                if (oldCell != newCell)
                {
                    MouseCellChanged?.Invoke(this, new MouseCellChangedEventArgs(oldCell, newCell, Control.MouseButtons));
                }
                if (oldCellBounds != newCellBounds)
                {
                    BoundsMouseCellChanged?.Invoke(this, new MouseCellChangedEventArgs(oldCellBounds, newCellBounds, Control.MouseButtons));
                }
            }
            // This excludes the outer border, but that's okay; it's not allowed for border dragging anyway.
            if (closestChanged && Metrics.Contains(newClosestMouseCellBorder) && newClosestMouseCellBorder.X > 0 && newClosestMouseCellBorder.Y > 0)
            {
                var oldCell = ClosestMouseCellBorder;
                ClosestMouseCellBorder = newClosestMouseCellBorder;
                ClosestMouseCellBorderChanged?.Invoke(this, new MouseCellChangedEventArgs(oldCell, ClosestMouseCellBorder, Control.MouseButtons));
            }
            if (closestChanged || mouseCellChanged)
            {
                // This is the normal Repaint-triggering invalidate, not the full map re-render one.
                mapPanel.Invalidate();
            }
        }

        public Point GetMouseCellPosition(Point location, out Point subPixel)
        {
            Point newMousePosition = mapPanel.ClientToMap(location);
            //Console.WriteLine("Mouse pos: (" + newMousePosition.X + ", " + newMousePosition.Y + ")");
            subPixel = new Point(
                (newMousePosition.X * Globals.PixelWidth / CellSize.Width) % Globals.PixelWidth,
                (newMousePosition.Y * Globals.PixelHeight / CellSize.Height) % Globals.PixelHeight
            );
            //Console.WriteLine("Mouse pos subcell: (" + subPixel.X + ", " + subPixel.Y + ")");
            if (newMousePosition.X < 0)
            {
                newMousePosition.X -= CellSize.Width;
            }
            if (newMousePosition.Y < 0)
            {
                newMousePosition.Y -= CellSize.Height;
            }
            //Console.WriteLine("Mouse pos cell: (" + (newMousePosition.X / CellSize.Width) + ", " + (newMousePosition.Y / CellSize.Height) + ")");
            return new Point(newMousePosition.X / CellSize.Width, newMousePosition.Y / CellSize.Height);
        }

        public void Render(Graphics graphics)
        {
            RenderRect(graphics, MouseCell);
        }

        public void RenderRect(Graphics graphics, Point mouseCell)
        {
            if (defaultMouseoverPen == null)
            {
                // Forces creation of pen object
                this.PenColor = penColor;
            }
            Pen p = defaultMouseoverPen;
            if (!MouseoverSize.IsEmpty && p != null)
            {
                var rect = new Rectangle(new Point(mouseCell.X * CellSize.Width, mouseCell.Y * CellSize.Height), CellSize);
                rect.Inflate(CellSize.Width * (MouseoverSize.Width / 2), CellSize.Height * (MouseoverSize.Height / 2));
                graphics.DrawRectangle(p, rect);
            }
        }

        public void Activate()
        {
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            if (includeNavigation)
            {
                // somehow, these key events don't seem to enable well; they
                // only work after enabling/disabling at least once. No clue why.
                // For this reason, arrow navigation was moved to MainForm.
                (this.mapPanel as Control).KeyDown += this.MapPanel_KeyDown;
                (this.mapPanel as Control).KeyUp += this.MapPanel_KeyUp;
            }
            this.mapPanel.MouseMove += MapPanel_MouseMove;
        }

        public void Deactivate()
        {
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            if (includeNavigation)
            {
                (this.mapPanel as Control).KeyDown -= this.MapPanel_KeyDown;
                (this.mapPanel as Control).KeyUp -= this.MapPanel_KeyUp;
            }
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            DisableDragging();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Deactivate();
                    if (this.defaultMouseoverPen != null)
                    {
                        try { this.defaultMouseoverPen.Dispose(); }
                        catch { /* Ignore*/ }
                    }
                    this.defaultMouseoverPen = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
