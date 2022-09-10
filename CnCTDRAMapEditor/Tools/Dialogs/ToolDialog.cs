using MobiusEditor.Interface;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using MobiusEditor.Utility;
using MobiusEditor.Event;
using MobiusEditor.Model;
using MobiusEditor.Controls;

namespace MobiusEditor.Tools.Dialogs
{
    public abstract class ToolDialog<T> : Form, IToolDialog where T : ITool
    {
        public T Tool { get; set; }
        public ITool GetTool() => Tool;
        public void SetTool(ITool value) => Tool = (T)value;

        private PropertyInfo defaultPositionPropertySettingInfo;
        private Point? startLocation;
        private Rectangle? parentBounds;

        public ToolDialog(Form parentForm)
        {
            defaultPositionPropertySettingInfo = Properties.Settings.Default.GetType().GetProperty(GetType().Name + "DefaultPosition");
            if (defaultPositionPropertySettingInfo != null)
            {
                startLocation = (Point)defaultPositionPropertySettingInfo.GetValue(Properties.Settings.Default);
                if (startLocation.Value.X == 0 && startLocation.Value.Y == 0)
                {
                    startLocation = null;
                    if (parentForm != null)
                    {
                        Rectangle rec = parentForm.Bounds;

                        int parentWidth = rec.Width;
                        int parentHeight = rec.Height;
                        int parentX = Math.Max(0, rec.X);
                        int parentY = Math.Max(0, rec.Y);
                        if (parentForm.WindowState == FormWindowState.Maximized)
                        {
                            if (rec.X < 0)
                                parentWidth = parentWidth + rec.X * 2;
                            if (rec.Y < 0)
                                parentHeight += (rec.Y * 2);
                        }
                        parentBounds = new Rectangle(parentX, parentY, parentWidth, parentHeight);
                    }
                }
            }
            this.FormClosing += this.ObjectToolDialog_FormClosing;
        }

        protected abstract void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList);

        public void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            // Fixed error: this creates a new Tool internally but never disposed the old one, resulting in lingering event bindings.
            if (this.Tool != null)
            {
                this.Tool.Dispose();
                this.Tool = default(T);
            }
            InitializeInternal(mapPanel, activeLayers, toolStatusLabel, mouseToolTip, plugin, undoRedoList);
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            saveDefaultPosition();
        }

        protected void saveDefaultPosition()
        {
            if (defaultPositionPropertySettingInfo != null)
            {
                defaultPositionPropertySettingInfo.SetValue(Properties.Settings.Default, Location);
                Properties.Settings.Default.Save();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (!startLocation.HasValue)
            {
                Rectangle screen = Screen.FromControl(this).WorkingArea;
                Rectangle rec = parentBounds.GetValueOrDefault(screen);
                int x = Math.Min(screen.Width - 50, Math.Max(0, rec.X + rec.Width - this.Width - 10));
                int y = Math.Min(screen.Height - 10, Math.Max(0, rec.Y + (rec.Height - this.Height) / 2));
                Point loc = new Point(x, y);
                startLocation = loc;
            }
            Location = startLocation.Value;
            // execute any further Shown event handlers
            base.OnShown(e);
        }
        
        private void ObjectToolDialog_FormClosing(System.Object sender, FormClosingEventArgs e)
        {
            // Prevents users from closing the form with Alt+F4
            e.Cancel = e.CloseReason == CloseReason.UserClosing;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tool.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            Tool?.Dispose();
            this.Tool = default(T);
            base.Dispose(disposing);
        }
    }
}
