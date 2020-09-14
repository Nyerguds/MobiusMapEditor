using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ToolDialog()
        {
            // TODO this current reflection approach does not work with tool windows that have a type parameter
            defaultPositionPropertySettingInfo = Properties.Settings.Default.GetType().GetProperty(GetType().Name + "DefaultPosition");
            if (defaultPositionPropertySettingInfo != null)
            {
                Location = (Point)defaultPositionPropertySettingInfo.GetValue(Properties.Settings.Default);
            }
        }

        public abstract void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList);

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);

            if (defaultPositionPropertySettingInfo != null)
            {
                defaultPositionPropertySettingInfo.SetValue(Properties.Settings.Default, Location);
                Properties.Settings.Default.Save();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Tool.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tool.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            Tool?.Dispose();
            base.Dispose(disposing);
        }
    }
}
