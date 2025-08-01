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

// EDITMODE: uncomment this to edit the form in the visual editor
//#define EDITMODE

using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public partial class SmudgeToolDialog :
#if !EDITMODE
        ToolDialog<SmudgeTool>
#else
        Form
#endif
    {
        public SmudgeToolDialog(Form parentForm)
#if !EDITMODE
            : base(parentForm)
#endif
        {
            InitializeComponent();
        }

        public SmudgeToolDialog(Form parentForm, IGamePlugin plugin)
            : this(parentForm)
        {
            smudgeProperties.Initialize(plugin, true);
        }

#if !EDITMODE
        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            smudgeTypeListBox.Types = plugin.Map.SmudgeTypes
                .Where(t => !Globals.FilterTheaterObjects || t.ExistsInTheater)
                .OrderBy(t => t.ID);
            Tool = new SmudgeTool(mapPanel, activeLayers, toolStatusLabel, smudgeTypeListBox, smudgeTypeMapPanel, smudgeProperties, plugin, undoRedoList);
        }
#endif
    }
}
