//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class MapPanel : Panel
    {
        private bool updatingCamera;
        private Rectangle cameraBounds;
        public Rectangle CameraBounds => cameraBounds;
        private Point lastScrollPosition;

        private (Point map, SizeF client)? referencePositions;

        private Matrix mapToViewTransform = new Matrix();
        private Matrix viewToPageTransform = new Matrix();

        private Matrix compositeTransform = new Matrix();
        private Matrix invCompositeTransform = new Matrix();

        private readonly HashSet<Point> invalidateCells = new HashSet<Point>();
        private bool fullInvalidation;

        private Image mapImage;
        public Image MapImage
        {
            get => mapImage;
            set
            {
                if (mapImage != value)
                {
                    mapImage = value;
                    UpdateCamera();
                }
            }
        }

        private double minZoom = 1.0;
        public double MinZoom
        {
            get => minZoom;
            set
            {
                if (minZoom != value)
                {
                    minZoom = value;
                    Zoom = zoom;
                }
            }
        }

        private double maxZoom = 8.0;
        public double MaxZoom
        {
            get => maxZoom;
            set
            {
                if (maxZoom != value)
                {
                    maxZoom = value;
                    Zoom = zoom;
                }
            }
        }

        private double zoomStep = 1;
        public double ZoomStep
        {
            get => zoomStep;
            set
            {
                if (zoomStep != value)
                {
                    zoomStep = value;
                    Zoom = (Zoom / zoomStep) * zoomStep;
                }
            }
        }

        private double zoom = 1;
        public double Zoom
        {
            get => zoom;
            set
            {
                AdjustZoom(value, true);
            }
        }

        private void AdjustZoom(double value, bool fromMousePos)
        {

            var newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, value));
            if (zoom != newZoom)
            {
                zoom = newZoom;

                var clientPosition = fromMousePos ? PointToClient(MousePosition) : new Point(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
                referencePositions = (ClientToMap(clientPosition), new SizeF(clientPosition.X / (float)ClientSize.Width, clientPosition.Y / (float)ClientSize.Height));

                UpdateCamera();
            }
        }

        /// <summary>
        /// Jump to the specified location on the map.
        /// </summary>
        /// <param name="metrics">Cellmetrics of the map, to know the full size.</param>
        /// <param name="position">Rectangle to jump to.</param>
        /// <param name="doZoom">True to zoom in as far as possible to the chosen area.</param>
        public void JumpToPosition(CellMetrics metrics, Rectangle position, bool doZoom)
        {
            JumpToPosition(metrics, position.X, position.Y, position.Width, position.Height, doZoom);
        }

        /// <summary>
        /// Jump to the specified location on the map.
        /// </summary>
        /// <param name="metrics">Cellmetrics of the map, to know the full size.</param>
        /// <param name="location">Cell to jump to.</param>
        /// <param name="doZoom">True to zoom in as far as possible to the chosen area.</param>
        public void JumpToPosition(CellMetrics metrics, Point location, bool doZoom)
        {
            JumpToPosition(metrics, location.X, location.Y, 1, 1, doZoom);
        }

        /// <summary>
        /// Jump to the specified location on the map.
        /// </summary>
        /// <param name="metrics">Cellmetrics of the map, to know the full size.</param>
        /// <param name="cellPointX">X-coordinate of the top left point to jump to</param>
        /// <param name="cellPointY">Y-coordinate of the top left point to jump to</param>
        /// <param name="cellsWidth">Width in cells of the area to center on.</param>
        /// <param name="cellsHeight">Height in cells of the area to center on.</param>
        /// <param name="doZoom">True to zoom in as far as possible to the chosen area.</param>
        public void JumpToPosition(CellMetrics metrics, int cellPointX, int cellPointY, int cellsWidth, int cellsHeight, bool doZoom)
        {
            if (doZoom)
            {
                // Ensure scrollbars; otherwise, ClientRectangle values are wrong.
                this.Zoom = this.maxZoom;
            }
            int scaleFull = Math.Min(this.ClientRectangle.Width, this.ClientRectangle.Height);
            bool isWidth = scaleFull == this.ClientRectangle.Width;
            double mapSize = isWidth ? metrics.Width : metrics.Height;
            // pixels per tile at zoom level 1.
            // Technically can't handle non-square cells, but, if anyone messes with that they have more problems than this function.
            double basicTileSize = scaleFull / mapSize;
            double zoom = this.Zoom;
            if (doZoom)
            {
                zoom = Math.Min(this.maxZoom, isWidth? ((double)metrics.Width / cellsWidth) : ((double)metrics.Height / cellsHeight));
                this.Zoom = zoom;
            }
            // Convert cell position to actual position on image.
            int cellX = (int)Math.Round(basicTileSize * zoom * (cellPointX + (cellsWidth / 2.0d)));
            int cellY = (int)Math.Round(basicTileSize * zoom * (cellPointY + (cellsHeight / 2.0d)));
            // Get location to use to center the chosen rectangle on the screen.
            int x = cellX - this.ClientRectangle.Width / 2;
            int y = cellY - this.ClientRectangle.Height / 2;
            this.AutoScrollPosition = new Point(x, y);
        }

        public void IncreaseZoomStep()
        {
            AdjustZoom(zoom + (zoom * zoomStep), false);
        }

        public void DecreaseZoomStep()
        {
            AdjustZoom(zoom - (zoom * zoomStep), false);
        }

        private bool smoothScale = Globals.MapSmoothScale;
        public bool SmoothScale
        {
            get => smoothScale;
            set
            {
                if (smoothScale != value)
                {
                    smoothScale = value;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool FocusOnMouseEnter { get; set; }

        public event EventHandler<RenderEventArgs> PreRender;
        public event EventHandler<RenderEventArgs> PostRender;

        public MapPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        public Point MapToClient(Point point)
        {
            var points = new Point[] { point };
            compositeTransform.TransformPoints(points);
            return points[0];
        }

        public Size MapToClient(Size size)
        {
            var points = new Point[] { (Point)size };
            compositeTransform.VectorTransformPoints(points);
            return (Size)points[0];
        }

        public Rectangle MapToClient(Rectangle rectangle)
        {
            var points = new Point[] { rectangle.Location, new Point(rectangle.Right, rectangle.Bottom) };
            compositeTransform.TransformPoints(points);
            return new Rectangle(points[0], new Size(points[1].X - points[0].X, points[1].Y - points[0].Y));
        }

        public Point ClientToMap(Point point)
        {
            var points = new Point[] { point };
            invCompositeTransform.TransformPoints(points);
            return points[0];
        }

        public Size ClientToMap(Size size)
        {
            var points = new Point[] { (Point)size };
            invCompositeTransform.VectorTransformPoints(points);
            return (Size)points[0];
        }

        public Rectangle ClientToMap(Rectangle rectangle)
        {
            var points = new Point[] { rectangle.Location, new Point(rectangle.Right, rectangle.Bottom) };
            invCompositeTransform.TransformPoints(points);
            return new Rectangle(points[0], new Size(points[1].X - points[0].X, points[1].Y - points[0].Y));
        }

        public void Invalidate(Map invalidateMap)
        {
            if (!fullInvalidation)
            {
                invalidateCells.Clear();
                fullInvalidation = true;
                Invalidate();
            }
        }

        public void Invalidate(Map invalidateMap, Rectangle cellBounds)
        {
            if (fullInvalidation)
            {
                return;
            }
            Invalidate(invalidateMap, cellBounds.Points());
        }

        public void Invalidate(Map invalidateMap, IEnumerable<Point> locations)
        {
            if (fullInvalidation)
            {
                return;
            }

            var count = invalidateCells.Count;
            invalidateCells.UnionWith(locations);
            if (invalidateCells.Count > count)
            {
                var overlapCells = invalidateMap.Overlappers.Overlaps(invalidateCells).ToHashSet();
                invalidateCells.UnionWith(overlapCells);
                Invalidate();
            }
        }

        public void Invalidate(Map invalidateMap, IEnumerable<Rectangle> cellBounds)
        {
            if (fullInvalidation)
            {
                return;
            }
            Invalidate(invalidateMap, cellBounds.SelectMany(c => c.Points()));
        }

        public void Invalidate(Map invalidateMap, Point location)
        {
            if (fullInvalidation)
            {
                return;
            }
            Invalidate(invalidateMap, location.Yield());
        }

        public void Invalidate(Map invalidateMap, int cell)
        {
            if (fullInvalidation)
            {
                return;
            }
            if (invalidateMap.Metrics.GetLocation(cell, out Point location))
            {
                Invalidate(invalidateMap, location);
            }
        }

        public void Invalidate(Map invalidateMap, IEnumerable<int> cells)
        {
            if (fullInvalidation)
            {
                return;
            }

            Invalidate(invalidateMap, cells
                .Where(c => invalidateMap.Metrics.GetLocation(c, out Point location))
                .Select(c =>
                {
                    invalidateMap.Metrics.GetLocation(c, out Point location);
                    return location;
                })
            );
        }

        public void Invalidate(Map invalidateMap, ICellOverlapper overlapper)
        {
            if (fullInvalidation)
            {
                return;
            }

            var rectangle = invalidateMap.Overlappers[overlapper];
            if (rectangle.HasValue)
            {
                Invalidate(invalidateMap, rectangle.Value);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (FocusOnMouseEnter && Form.ActiveForm != null)
            {
                Focus();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Zoom += (Zoom * ZoomStep * Math.Sign(e.Delta));
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            UpdateCamera();
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);

            InvalidateScroll();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.Clear(BackColor);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            InvalidateScroll();

            PreRender?.Invoke(this, new RenderEventArgs(pe.Graphics, fullInvalidation ? null : invalidateCells));
            Image mapImg = mapImage;
            if (mapImg != null)
            {
                pe.Graphics.Transform = compositeTransform;
                var oldCompositingMode = pe.Graphics.CompositingMode;
                var oldCompositingQuality = pe.Graphics.CompositingQuality;
                var oldInterpolationMode = pe.Graphics.InterpolationMode;
                var oldPixelOffsetMode = pe.Graphics.PixelOffsetMode;
                MapRenderer.SetRenderSettings(pe.Graphics, SmoothScale);
                pe.Graphics.DrawImage(mapImg, 0, 0);
                pe.Graphics.CompositingMode = oldCompositingMode;
                pe.Graphics.CompositingQuality = oldCompositingQuality;
                pe.Graphics.InterpolationMode = oldInterpolationMode;
                pe.Graphics.PixelOffsetMode = oldPixelOffsetMode;
            }
            PostRender?.Invoke(this, new RenderEventArgs(pe.Graphics, fullInvalidation ? null : invalidateCells));
#if DEVELOPER
            if (Globals.Developer.ShowOverlapCells)
            {
                var invalidPen = new Pen(Color.DarkRed);
                foreach (var cell in invalidateCells)
                {
                    pe.Graphics.DrawRectangle(invalidPen, new Rectangle(cell.X * Globals.MapTileWidth, cell.Y * Globals.MapTileHeight, Globals.MapTileWidth, Globals.MapTileHeight));
                }
            }
#endif
            invalidateCells.Clear();
            fullInvalidation = false;
        }

        private void UpdateCamera()
        {
            if (mapImage == null)
            {
                return;
            }

            if (ClientSize.IsEmpty)
            {
                return;
            }

            updatingCamera = true;

            var mapAspect = (double)mapImage.Width / mapImage.Height;
            var panelAspect = (double)ClientSize.Width / ClientSize.Height;
            var cameraLocation = cameraBounds.Location;

            var size = Size.Empty;
            if (panelAspect > mapAspect)
            {
                size.Height = (int)(mapImage.Height / zoom);
                size.Width = (int)(size.Height * panelAspect);
            }
            else
            {
                size.Width = (int)(mapImage.Width / zoom);
                size.Height = (int)(size.Width / panelAspect);
            }

            var location = Point.Empty;
            var scrollSize = Size.Empty;
            if (size.Width < mapImage.Width)
            {
                location.X = Math.Max(0, Math.Min(mapImage.Width - size.Width, cameraBounds.Left));
                scrollSize.Width = mapImage.Width * ClientSize.Width / size.Width;
            }
            else
            {
                location.X = (mapImage.Width - size.Width) / 2;
            }

            if (size.Height < mapImage.Height)
            {
                location.Y = Math.Max(0, Math.Min(mapImage.Height - size.Height, cameraBounds.Top));
                scrollSize.Height = mapImage.Height * ClientSize.Height / size.Height;
            }
            else
            {
                location.Y = (mapImage.Height - size.Height) / 2;
            }

            cameraBounds = new Rectangle(location, size);
            RecalculateTransforms();

            if (referencePositions.HasValue)
            {
                var mapPoint = referencePositions.Value.map;
                var clientSize = referencePositions.Value.client;

                cameraLocation = cameraBounds.Location;
                if (scrollSize.Width != 0)
                {
                    cameraLocation.X = Math.Max(0, Math.Min(mapImage.Width - cameraBounds.Width, (int)(mapPoint.X - (cameraBounds.Width * clientSize.Width))));
                }
                if (scrollSize.Height != 0)
                {
                    cameraLocation.Y = Math.Max(0, Math.Min(mapImage.Height - cameraBounds.Height, (int)(mapPoint.Y - (cameraBounds.Height * clientSize.Height))));
                }
                if (!scrollSize.IsEmpty)
                {
                    cameraBounds.Location = cameraLocation;
                    RecalculateTransforms();
                }

                referencePositions = null;
            }

            SuspendDrawing();
            AutoScrollMinSize = scrollSize;
            AutoScrollPosition = (Point)MapToClient((Size)cameraBounds.Location);
            lastScrollPosition = AutoScrollPosition;
            ResumeDrawing();

            updatingCamera = false;

            Invalidate();
        }

        private void RecalculateTransforms()
        {
            mapToViewTransform.Reset();
            mapToViewTransform.Translate(cameraBounds.Left, cameraBounds.Top);
            mapToViewTransform.Scale(cameraBounds.Width, cameraBounds.Height);
            mapToViewTransform.Invert();

            viewToPageTransform.Reset();
            viewToPageTransform.Scale(ClientSize.Width, ClientSize.Height);

            compositeTransform.Reset();
            compositeTransform.Multiply(viewToPageTransform);
            compositeTransform.Multiply(mapToViewTransform);

            invCompositeTransform.Reset();
            invCompositeTransform.Multiply(compositeTransform);
            invCompositeTransform.Invert();
        }

        private void InvalidateScroll()
        {
            if (updatingCamera)
            {
                return;
            }

            if ((lastScrollPosition.X != AutoScrollPosition.X) || (lastScrollPosition.Y != AutoScrollPosition.Y))
            {
                var delta = ClientToMap((Size)(lastScrollPosition - (Size)AutoScrollPosition));
                lastScrollPosition = AutoScrollPosition;

                var cameraLocation = cameraBounds.Location;
                if (AutoScrollMinSize.Width != 0)
                {
                    cameraLocation.X = Math.Max(0, Math.Min(mapImage.Width - cameraBounds.Width, cameraBounds.Left + delta.Width));
                }
                if (AutoScrollMinSize.Height != 0)
                {
                    cameraLocation.Y = Math.Max(0, Math.Min(mapImage.Height - cameraBounds.Height, cameraBounds.Top + delta.Height));
                }
                if (!AutoScrollMinSize.IsEmpty)
                {
                    cameraBounds.Location = cameraLocation;
                    RecalculateTransforms();
                }

                Invalidate();
            }
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        private void SuspendDrawing()
        {
            SendMessage(Handle, WM_SETREDRAW, false, 0);
        }

        private void ResumeDrawing()
        {
            SendMessage(Handle, WM_SETREDRAW, true, 0);
        }
    }
}
