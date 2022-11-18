## Change log

### Features added by Rampastring:

* Downsized tool graphics by a user-configurable factor so you can see more placeable object types at once on sub-4K monitors.
* Improved zoom levels.
* Fixed a couple of crashes.
* Made tool windows remember their previous position upon closing and re-opening them.
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks.

### Features and fixes by Nyerguds:

#### v1.4.0.0:

Released on 08 Jul 2022 at 21:37 UTC

* Fixed overlay height overflow bug in Rampa's new UI.
* Fixed map tiles list duplicating every time the "Map" tool window is opened in Rampa's version.
* Split off internal overlay type "decoration", used for pavements and civilian buildings.
* Added CONC and ROAD pavement. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in teamtypes.
* Removed the Aircraft from the placeable units in TD. [NOTE: made into a setting in v1.4.3.0]
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc).
* Fixed case sensitivity related crashes in TD teamtypes.
* TD triggers without a teamtype will now automatically get "None" filled in as teamtype, fixing the malfunctioning of their repeat status.
* Added [Ctrl]+[N], [Ctrl]+[O], [Ctrl]+[S] etc. shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tool window to adjust crater stage.
* Fixed TD terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
* RA "Spied by..." trigger event now shows the House to select. [NOTE: reverted in v1.4.4.0]
* Added "Add" buttons in triggers and teamtypes dialogs.
* Fixed tab order in triggers and teamtypes dialogs.
* Fixed crash in "already exists" messages for triggers and teamtypes.
* Randomised tiberium on save, like the original WW editor does. (This is purely cosmetic; the game re-randomises it on map load.)
* Added ability to place bibs as smudge type. They won't show their full size in the editor at the moment, though.

#### v1.4.0.1:

Released on 13 Jul 2022 at 07:35 UTC

* Added "All supported types (\*.ini;\*.bin;\*.mpr)" as default filter when opening files.
* Added Drag & Drop support for opening map files.
* Added command line file argument support, which allows setting the editor as application for opening ini/mpr files.
* House Edge reading now corrects values with case differences so they show up in the dropdown.
* Centralised the House Edge array on the House class, and changed its order to a more logical North, East, South, West.
* Fixed order of the Multi-House colors. It seems the error is not in the editor, but in bizarre mixed-up team color names in the remastered game itself.
* Remapped Neutral (TD only) and Special as yellow, as they are in the game.
* All tool windows will now save their position.
* Tool windows for which no position was previously set will center themselves on the right edge of the editor.
* Some things, like crates, were missing names. This has been fixed.
* All objects except map tilesets will now show a real name and their internal code.
* Added ASCII restriction to trigger and teamtype names, since the map formats don't support UTF-8. (Except on the Briefing, apparently, since the GlyphX part handles that.)
* Made "Already exists" check on trigger and teamtype names case insensitive, since that is how the game handles them.
* Triggers and teamtypes dialogs have a new logic for generating names for new entries that should never run out.
* Triggers and teamtypes dialogs support the delete key for deleting an entry in the list.
* Triggers and teamtypes dialogs have "Rename" added to the context menu when right-clicking an item.
* Triggers and teamtypes dialogs now warn when cancelling if changes were made.
* "Add" button in triggers and teamtypes dialogs gets disabled when the internal maximum amount of items for the type is reached.
* Changed the default build level in TD maps from 99 to 98. Level 99 allows building illegal objects that can break the game.
* The Briefing text area will now accept [Enter] for adding line breaks without this closing the window. Previously, [Ctrl]+[Enter] had to be used for this, which is pretty awkward.
* The Briefing text area now has a scrollbar.
* Fixed placement of illegal tiles caused by incorrect filtering on which tiles from a template should be included. This is the problem which caused tiles that showed as black blocks in classic graphics. It is also the problem that made RA maps contain indestructible bridges.
* Map tile placement can now be dragged, allowing easily filling an area with water or other tiles. This also works for removing tiles.
* Removing tiles will now obey the actual occupied cells of the selected tile, rather than just clearing the bounding box, making it more intuitive.
* Creating an RA trigger with Action "Text Trigger" will no longer cause an error to be shown.
* Trigger controls no longer jump around slightly when selecting different options.
* Using the mouse wheel will now change the tiberium field size per 2, like a normal arrow click would.

#### v1.4.0.2:

Released on 14 Jul 2022 at 20:22 UTC

* Fixed the bug that cleared all map templates on save in v1.4.0.1 (whoops).
* Fixed the bug in the teamtypes list that showed the wrong context menu options on right click.
* Fixed the bug that the status bar did not show the map placement shortcuts hints on initial load.
* The editor no longer exits if it cannot connect to Steam. Instead, workshop publishing will simply be disabled if the Steamworks interface can't be initialised.
* The texture manager will now properly dispose all loaded image objects when a different map is loaded.
* Added \*.ini to the list of possible extensions for saving RA maps, to support opening pre-Remaster missions.
* If a building has no direction to set and shows no dropdown for it, the "Direction" label is now also removed.
* Structure graphics are now correctly centered on their full building size.
* The damaged state of buildings is now shown at strength values of 128 and below, rather than only below that value. [NOTE: adjusted further in v1.4.4.0]
* Damaged states now work correctly on all buildings, with a vastly simpler and more general internal logic.
* Using the mouse wheel will now change the strength of objects in increments of 4.
* IQ of all Houses in RA now defaults to 0.
* Fixed Gunboat facing and damage states logic.
* Fixed bug causing bad refresh when previewing the placement of a single cell selected from a template with an empty top right corner cell.
* The "clear1" tile is now explicitly shown in the tiles list. It acts as eraser.
* Teamtype "Priority" value (recruit priority) is now capped at 15. [NOTE: reverted in v1.4.0.3]

#### v1.4.0.3:

Released on 27 Jul 2022 at 09:47 UTC

* The editor now tries to automatically detect the game installation folder in Steam.
* Fixed refresh errors in preview images when resizing tool windows.
* All overlay items will now show a preview icon of the same size.
* Fixed errors in tree sizes.
* The 'clamping' logic that prevented tool windows from being dragged outside usable screen bounds had a bug that this prevented it from being dragged onto a different monitor. This is now fixed.
* Added "Theme" to the map settings.
* Removed "Percent" from the map settings. It is an unused Dune II leftover.
* Added "Classic only" labels to "Carryover Money" and "Theme" to indicate these options will only work when playing the missions in the original game.
* All videos available in the Remaster are now shown in the video lists in the "Map settings" dialog. [NOTE: reverted for RA in v1.4.1.0]
* Added missing entries (videos not included in the Remaster) to the RA and TD video lists, with a 'Classic only' indicator.
* In the teamtypes dialog, the rather confusing use of the internal name "Missions" was changed to a more intuitive "Orders".
* Added tooltips for all teamtype options.
* Teamtype orders now show a tooltip on the Argument field indicating the meaning of the value to give, and, if needed, the possible values to choose from. [NOTE: replaced by choice lists in v1.4.1.0]
* Fixed tab order of the teamtype options.
* The dropdowns in the grids in the teamtypes dialog now respond without having to click multiple times.
* Removed the previously-added cap on the teamtype "Priority" value after feedback from users and checking the source code.
* The CONC and ROAD overlay types now show the same graphics as in-game. This is technically just a dummy graphic the game uses when not finding object graphics. The version in the editor is a reconstruction.
* Removed limitation on placing resources on the top and bottom row of the map.

#### v1.4.1.0:

Released on 20 Aug 2022 at 22:37 UTC

* Fixed dimensions of RA's ore mine, Snow theater ice floes and Interior theater boxes, and one of the Desert theater rocks in TD.
* Added \*.ini to the list of possible extensions for opening RA maps. Apparently before I only added it for saving.
* The editor will now accept nonstandard extensions from drag & drop without any issues. For TD maps, it will need to find the accompanying bin or ini file with the correct extension.
* Files opened from filenames with nonstandard extensions will not change these extensions when saving the file. This also means RA maps opened from a .ini file will no longer change the extension to .mpr when saving.
* Terrain objects will now only pop up a poperties box for setting a trigger on TD maps.
* Optimised loading so the editor will skip loading objects from different theaters.
* User settings (game folder, invite warning, and the dialog locations) will now be properly ported over from previous versions.
* Added support for loading mod xml info and graphics through the "ModsToLoad" setting in "CnCTDRAMapEditor.exe.config". The syntax is a semicolon-separated list, with each entry either a Steam workshop ID, or a folder under "Documents\CnCRemastered\Mods\". As folder, the path must contain the "Tiberian_Dawn" or "Red_Alert" part at the start. That prefix folder will also be used as consistency check for the mod type as defined inside "ccmod.json". Mods given by folder name will also be looked up in the Steam workshop folders, with the prefix folder used only for the consistency check. Mods do NOT have to be enabled in the game to work in the editor. [NOTE: game prefix requirement for paths removed when this was split into settings per game in v1.4.4.0]
* Added support for the unique pattern of TD's "conc" pavement. You will need the "ConcretePavementTD" mod to actually see that, though. This mod is filled in by default in the editor's mod loading settings, meaning it will automatically be used if found. [NOTE: mod name changed to GraphicsFixesTD in v1.4.4.0]
* Fixed loading and saving of the videos set in the map options dialog, so no more errors pop up there.
* Reverted videos list for Red Alert; the game can only handle videos that are in its internal hardcoded list.
* Made video names freely editable for TD missions. Any mod-added video in TD is playable from missions. Be warned that when a video is not found, this may cause the game to hang for several minutes.
* The preview selection in the Steam publish dialog will now open in the correct folder.
* The new setting "NoMetaFilesForSinglePlay" in "CnCTDRAMapEditor.exe.config" will suppress the generation of .json and .TGA file when saving single player missions to disc. Not writing them is now the default behavior. This does not affect the Steam workshop upload behavior.
* The rendered previews will now show all map contents, to give a better representation of what is on the map. Note that for single play missions, this preview is generated in the folder but is optional.
* Removed crater types CR2 to CR6; they don't work correctly in either game and will just show the smallest size of CR1. Any craters of other types encountered on map load will now be converted to CR1. [NOTE: made into a setting in v1.4.3.0]
* The teamtypes dialog no longer uses data grids for its teams and orders.
* Teamtypes now show full names for unit types.
* The input for arguments for orders in the teamtypes dialog now correctly adapts to the type of each order, giving dropdowns for special choices lists and for waypoints.
* The waypoints that can be selected for an RA teamtype now correctly start from -1 as "(none)".
* Fixed color of "Special" in RA to have the same color as Spain.
* Fixed the fact trigger Events and Actions retained their argument data when changing their type, meaning the UI would pick the equivalent data on whatever list or control popped up for the new type.
* RA triggers now show human-readable data for the Event and Action arguments.
* The editor no longer locks up when the triggers dialog shows an empty list of teamtypes or (previously-saved) triggers because none were made yet.
* Removed Aircraft section handling. Aircraft were never able to be pre-placed in the original game, and the re-enabled sections in the Remasters have issues; aircraft will still spawn in the air and fly somewhere close. [NOTE: made into a setting in v1.4.3.0]
* Like walls, overlay placement and removing can now be dragged to affect multiple cells.
* All waypoint will now be shown with their coordinates.
* Added "Jump to..." button on the waypoints tool. This will only have any effect when zoomed in.
* Clicking overlapping waypoints multiple times will cycle to the next one in the list on each click. Right-clicking will cycle backwards.
* When deleting overlapping waypoints, if the currently selected waypoint is one of them, that one will be deleted first.
* Map indicators will now be painted in this order: map boundaries, celltriggers, waypoints, building labels, object triggers. The later ones will be on top and thus most visible.
* Map indicators for the type you are currently editing are now always drawn last, and thus the most visible. (e.g. overlapping celltriggers and waypoints)
* Unit/building/infantry tools now paint the object trigger labels last, so they no longer get painted over by the occupied cell indicators.
* For assets / mods loading, TGA files that are not inside a .zip file can now also load their positioning metadata from accompanying .meta files.
* Factory doors will no longer be seen as semitransparent on the placement preview.
* Fixed incorrect cleanup of internal tool objects, which could cause odd bugs like two selected cells being visible on the tileset tool.
* Terrain and structure editing mode will now draw the green overall bounds of all objects, and only then the red occupied cells. Before, both were drawn per object and could cause odd overlaps.
* Optimised all calculations related to centering objects in their bounding box and drawing them on the map.
* Infantry are now positioned more accurately.
* The terrain tool now uses a list box like all the other tools, instead of the awkward dropdown list.
* The smudge tool now allows setting the size in the preview window, and picking craters with a different size from the map.
* The "MapScaleFactor" and "PreviewScaleFactor" settings in "CnCTDRAMapEditor.exe.config" can adjust the downscaling factor for respectively the map graphics and the preview graphics. Higher values will reduce quality, but will make the editor more responsive. By default, previews in tool windows will now use higher quality graphics than the map. Setting a negative value will enable smooth scaling. (Not advised, but it's available)
* When removing a trigger, all celltriggers and objects linking to that trigger will now get their trigger cleared. Before, this only happened for structures.
* The triggers available for linking to objects and cells are now filtered out to only those triggers with an Event (or Action, in RA) that can be linked to that object type. This will also affect the cleanup of triggers if a trigger's Event or Action was changed to something not compatible with the objects it was linked to.
* An "Info" icon next to the trigger dropdowns in the placement tool windows will give an explanation of which trigger Events and Actions work for that type.
* For celltriggers and waypoints, the item selected in the tool dropdown will now be highlighted on the map in yellow.
* When you select a unit to place, a logical default Mission (order to execute) is now selected.
* The Celltrigger tool will now always be enabled, even if there are no placeable triggers available. This way, people can still check the "Info" icon on the tool to see the requirements for placeable triggers.
* The brush size on the resource tool will now adjust itself if an incorrect (even) value is manually entered on it.
* Map loading validation will now also validate terrain templates, meaning corrupted maps have a much higher likelihood to give correct feedback. This will probably cause a lot of messages on the ghost tiles caused by the original map editor not correctly cutting out complex shapes on objects like bridges and map decorations, but those just mean your map got cleaned up to a valid state.
* Map validation will now be done _before_ the "Save File" dialog opens.
* Ini reading will now trim the value, like the original game does, allowing entries of the type "key = value".
* Fixed potential crashes in the generation of map validation messages (when encountering empty lines like "21=").
* The Smudge window caused a crash when trying to show the bibs in Red Alert Interior theater. The bibs are now filtered out and no longer shown for Interior.
* Structures can no longer be put in an illegal state where "Prebuilt" is disabled but the rebuild priority is set to -1.
* Fixed refresh issues that occurred when moving the mouse out of the map area while still in placement mode.
* Fixed incorrect tooltip placement when using bounds dragging mode on a different monitor.
* Red Alert's Interior tileset now supports randomising the 1x1 tiles that contain alternate versions. This type will now show all alternates on a blue grid in the preview window. Specific tiles can still be picked the usual way if you do not want random ones.
* If mods add extra tiles to existing 1x1 tilesets, these will be treated as 1x1 with alternates too.
* Tanya's default coloring in the editor preview is now Allied, and the M.A.D. Tank's color is now Soviet.
* Changed Red Alert's trigger action "Destroy attached building" to a more accurate "Destroy attached object", seeing as it even works from celltrigger to kill units.
* Bibs are now shown as part of the building boundaries.
* Bibs boundary checking can be disabled with a global setting.
* The overlap detection in the map loading now correctly scans the full footprint of buildings and terrain objects, and will now correctly report the blocking object.
* Sounds lists in RA triggers now have descriptions.
* If you try to save an opened file but the folder it was loaded from is deleted, it will no longer give an error, but revert to "Save As" behavior.
* The chosen preview image in the Steam upload dialog will now also be used as in-game preview for the map.
* In TD maps, a building that is set to be rebuilt but is not built from the start will now show as House "None".
* The Interior tiles "wall0002" to "wall0022" are now grouped per type into three dummy-entries "wallgroup1", "wallgroup2" and "wallgroup3", to simplify random placement of these tiles and to remove clutter from the tiles list.

#### v1.4.1.1:

Released on 22 Aug 2022 at 09:28 UTC

* The Red Alert teamtype order "Guard Area" now correctly has 'time' as argument type, rather than a waypoint.
* Added a system to detect singleplayer missions from the original games and automatically mark them as singleplayer if they conform to the classic naming scheme for singleplayer missions, and contain a Lose and Win trigger.
* Functions asking to save unsaved changes (New/Open/Publish/Close) will now actually abort if you choose to save the unsaved opened map but it doesn't pass the basic waypoints validation, rather than giving the validation fail message and then continuing anyway.
* The title of the window will now show "Untitled.ini" or "Untitled.mpr" when you have a new but unsaved map opened. [NOTE: changed from filename to map name in v1.4.4.0]
* The title of the window will now show an asterisk behind the filename to indicate that the current file has unsaved changes.
* Maps loaded from file are now seen as 'modified' if any issues were detected that made the editor change or discard data during the loading process.
* The triggers check feedback (TD only) now also uses the large window used for showing the map load errors.

#### v1.4.2.0:

Released on 05 Sep 2022 at 14:25 UTC

* Menu items related to map actions will now be disabled if no map is loaded.
* Fixed "Destroy attached object" trigger in RA not being seen as valid on units.
* Added power balance evaluation tool.
* If a map is saved without a map name set in the map settings, the filename will be filled in as name.
* Expanded the "View" menu to enable and disable drawing for all possible map elements and indicators on the map.
* Vehicle previews are now shown in a more dynamic south-west facing.
* When a map is opened, the editor will load theater-specific icons into the toolstrip.
* Resource placement is now disabled in Interior theater.
* Map loading now checks if map objects exist in the specified theater. [NOTE: made into a setting in v1.4.3.0]
* An image export function has been added. This will mirror the current items enabled in the "View" menu. Its size is determined by the "ExportScale" setting in "CnCTDRAMapEditor.exe.config". [NOTE: made into a full dialog, with the setting changed to DefaultExportScale, in v1.4.4.0]
* Fixed a glitch that made the trigger dropdown of the opened tool stop functioning correctly after editing the triggers.
* Template 'BRIDGE1H' in RA now shows its full available tileset. Seems this is an odd corner case where Westwood were the ones who forgot to cut it out properly, but that does make its two last tiles valid.
* The editor will now detect when, on Red Alert maps, the obsolete tile with id 255 is used as 'clear' terrain, and will only show a single message about it. There is an "IgnoreRaObsoleteClear" setting in "CnCTDRAMapEditor.exe.config" to disable filtering out this tile, though that is only useful for research purposes.
* Fixed the map panel crashing when a repaint is done on an already-closed map. This happenen when a map was opened, then another one was loaded but showed the load errors dialog, and then something (like "Show Desktop") triggered a repaint of the editor.
* Changed all scaling settings to floating point numbers, making them much more accurate.
* Building labels for fake buildings and for the rebuild priority will now scale correctly with the map scaling settings, meaning they should always remain the same size relative to the building.
* Indicator lines like the map border, cell occupation indicators and the yellow selection box will now scale with the map scaling settings, meaning they don't become bigger when reducing the graphics scale.
* Aftermath units can now be enabled and disabled in the "Map Settings" window. Disabling this will clear any expansion units from the map and from Teamtypes.
* When Aftermath units are detected on map load, but the Aftermath units setting was not enabled in the ini, the loading system will enable the Aftermath units setting.
* All waypoints in the triggers and teamtypes dialogs now show their coordinates.
* Vastly optimised map bounds dragging.
* Map bounds dragging will no longer revert when releasing the [Ctrl] key before the mouse button.
* The label shown when dragging the map border now appears on the inside of the bounds, exactly a cell away from the bounds, and only updates on every cell change.
* Map bounds dragging will change the border at the moment you cross the halfway point between cells, rather than when entering a new cell, making it much more intuitive.
* Drag-scrolling, which is normally middle mouse button, now also works by holding down the space bar, to support devices (like laptops touch pads) without middle mouse button.
* Drag-scrolling is now much more accurate.
* Fixed an issue in the triggers where data got reset if you switched between two triggers that both had a field that contained the same numeric data.
* Teamtypes now show the House in the list.
* Teamtypes in Red Alert maps now filter out the triggers list to unit-applicable triggers only.
* Teamtypes and triggers can now be selected by clicking anywhere on their row, rather than having to click specifically on the text in the row.
* The check on multiplayer waypoints being placed down now correctly checks only the specific player start waypoints, rather than just any waypoints including the special singleplay ones.
* The possible multiplayer waypoints to place on a map now go from 0 to 15. [NOTE: reverted in v1.4.3.2]
* If the map is marked as single player scenario, the first waypoints are no longer indicated as player start positions with a "P" prefix.
* Mods will now only be loaded for maps targeted at their respective game, meaning common assets can be overridden differently by TD and RA mods. [NOTE: mod settings split up per game in v1.4.4.0]

#### v1.4.3.0:

Released on 13 Sep 2022 at 21:46 GMT

* Fixed a bug where the default House in TD maps was set to "None", causing them to crash the game.
* Bibs placed as the 'smudge' type now show their full size, and can be placed in ways that makes them partially overlap. As long as at least one cell of a bib exists, the bib exists.
* If a json is saved for a singleplayer map, its waypoints list now only contains the Home waypoint, since saving the first waypoints as start locations is pointless for them.
* The smudge placement mode will now show a red cursor when hovering over bibs attached to buildings, to indicate those can't be replaced or removed.
* All actions in the editor now have undo/redo functionality.
* The amount of stored undo/redo actions can now be set in the "UndoRedoStackSize" setting in "CnCTDRAMapEditor.exe.config".
* Undo/Redo actions can be cleared using a new option in the Edit menu. This can be used in case too many might make the editor laggy.
* Disabling Aftermath units will now clear the Undo/Redo history, to avoid conflicts.
* Fixed undo/redo of map bounds dragging; it was severely bugged and often reduced the bounds to minimum size.
* Fixed bug where tool windows can be closed with [Alt]+[F4], causing the editor to crash when trying to re-open them.
* Fixed tab order on the "New Map" dialog, so the radio buttons are selected by default.
* Mobile Radar Jammer and Mobile Gap Generator now show different facings for their "turrets".
* Fixed a bug in the power balance tool which made it ignore the first House.
* Added a silo storage capacity tool.
* Added a section in the map settings for scenario-specific options in RA maps.
* Added a rules editing field for RA maps that allows editing/adding ini sections not handled by the editor. Changing building bibs, power and resource storage in this will immediately affect the editor.
* Dragging a building's bib over smudge will no longer remove the smudge, unless it's actually placed down on it.
* Undoing a building's placement or moving will now restore any smudge the building's bib replaced.
* Added map template flood fill mode ([Ctrl]+[Shift]+[Left-Click]) and flood fill clear mode ([Ctrl]+[Shift]+[Right-Click]).
* Enabling/disabling Indicator items in the View menu no longer does a full refresh of the map, making it nearly instant.
* Added options to re-enable Aircraft and full craters list.
* Changed all tweak options to have True as default value.
* Fixed the fact the Oil Pump (V19) was not usable in Interior theater.
* Added specific "FilterTheaterObjects" option for the behavior of filtering out theater-illegal objects.
* Added the Desert civilian buildings to RA (though they won't show unless "FilterTheaterObjects" is disabled).
* The building tool preview now shows bibs.
* Object previews for multi-cell types in the tool window now show the occupied cell indicators.
* Placement previews now show the occupied cell indicators.
* Civilian buildings in RA maps will no longer change their colors to the House that owns them. This does not apply to TD, since the TD civilian buildings actually change their color in-game.
* Fake buildings once again show the "Fake" label on the preview in the buildings list. This was accidentally removed when splitting off the label drawing to an indicator.
* For RA, changing the "Base" House in the map settings will now also change the House on the preview pane if it is not set as Prebuilt.
* Improved the system to detect blocking objects on map load.
* The "Sellable" and "Rebuild" options for RA buildings are now disabled if the structure is not set as Prebuilt.
* While holding [Ctrl] in Map mode to enable bounds editing mode, diagonals will now be drawn inside the bounds rectangle to easily see the center. [NOTE: expanded to full map symmetry indicators in v1.4.4.0]
* While holding [Ctrl] in Map mode to enable bounds editing mode, the whole bounds rectangle can now be moved by clicking inside it and dragging it around.
* While holding [Ctrl] in Map mode to enable bounds editing mode, you will no longer select tiles when clicking.

#### v1.4.3.1:

Released on 14 Sep 2022 at 16:52 GMT

* Fixed a crash when flood-clearing with a template that crossed the map bounds.
* When flood-clearing with a template containing cells on both sides of the map bounds, the clear operation will now ignore the map bounds.
* Flood fill clear will now no longer ignore if the user has a single cell from a template selected.

#### v1.4.3.2:

Released on 14 Sep 2022 at 21:20 GMT

* Fixed a crash in the smudge restore system when you delete a smudge or building that is too close to the map edge.
* Reduced maximum multiplayer start positions in the editor to 8, since the games apparently can't show more.

#### v1.4.4.0:

Released on 14 Nov 2022 at 22:25 GMT

* When your mouse cursor is inside the map bounds and you press [Ctrl] in Map mode to enable bounds editing mode, your cursor will now immediately change to the Move cursor, without requiring any mouse movement.
* The status bar at the bottom will now explicitly mention the sub-position of the infantry under the mouse cursor.
* When loading a map, if a map's file name identifies it as classic single player mission, this will no longer mark the mission as "modified" by the loading process. This will make it simpler to open classic maps for reference without getting a save prompt on close. Do note that lots of classic maps contain errors in triggers being linked to wrong objects, and the automatic fixes for that **will** still mark the map as modified.
* Fixed issues with the editor window getting focused simply by moving the mouse over it. The main window can steal focus from the tool window, but not from other applications.
* In Waypoints and Cell Triggers editing mode, the PageUp, PageDown, Home and End keys will now let you go through the dropdown items list. PageUp and PageDown will act as normal arrow keys.
* In Waypoints editing mode, pressing [Shift] and a key will select special waypoints: [F]lare, [H]ome, [R]einforce, [S]pecial.
* The shortcut keys to select the different editing modes (normally Q, W, E, R, T, Y, A, S, D, F, G) now work on keyboard position, meaning they will also work in the same logical way on AZERTY keyboards.
* Fixed bug where the checked states of the Houses in the Map Settings would reset when changing the "Single-Player" checkbox.
* The [Aftermath] section is no longer ignored in the map settings' Rules editor, so Aftermath detail settings can be added. The actual "NewUnitsEnabled" setting in this is ignored, though; toggling the expansion units can only be done in the Basic settings.
* Undo/Redo tracking will now also undo the map's modified-status.
* Added support for the C&C95 v1.06 briefing line split format.
* Added Tiberian Dawn Megamap support. (Sole Survivor map type)
* Made the editor store its dll mess in a subfolder, like the retail version does.
* Sole Survivor is now supported as separate game type in the editor. Its waypoints include four special crate spawning hotspots. These maps don't support owned objects (infantry, units, structures) by default, though this can be re-enabled using the "NoOwnedObjectsInSole" setting in "CnCTDRAMapEditor.exe.config". Crates are always disabled, however, and cannot be enabled.
* The damaged state of buildings now works correctly per game; for TD it shows below 50%, for RA it shows at 50%.
* Changed waypoints to actual map objects, indicated using the green beacon graphic that was already used as icon for the Waypoints editing mode. They can be disabled in the View menu.
* Waypoint labels are now drawn at the bottom of the cell, in the same style as the building rebuild priority labels. They can be disabled in the View menu, but like the building labels, they will not be drawn if Waypoints are not enabled.
* Multiplayer starting points are now shown as colored flags. For TD, Nod's metallic blue will be used for the 7th flag (P6). For SS, which has classic colour order configured, metallic gray is the 4th flag (TM3), and the new bright blue is the 7th (TM6). For both TD and SS, the 8th flag has its value hardcoded as the purple from RA.
* Waypoints now show a preview in placement mode.
* On Sole Survivor maps, there is a special "Football goal areas" indicator that shows how much area around the flag needs to be left open to be paved with concrete in Football mode. These can be disabled under "View" → "Indicators".
* The game name of the opened map type is now shown in the title bar.
* Changed the editor name in the title to "Mobius Map Editor".
* Red Alert maps are now specifically detected on the presence of the "[MapPack]" section. If this is not present, and there is no .bin file, it loads as TD map without map templates.
* Restricted Red Alert trigger and teamtype names to the same lengths as Tiberian Dawn; 4 for triggers, 8 for Teamtypes.
* Pressing [Enter] in Waypoints mode will now jump to the selected waypoint.
* Fixed a bug in the overlap detection system that made it always give "<unknown>" for the overlapped cell on Terrain objects.
* Split mods up into ModsToLoadTD, ModsToLoadRA and ModsToLoadSS. Entries in the list no longer require the game folder prefix.
* The Civilian buildings V12 and V13 (haystacks) are now also available in TD Winter theater.
* The trigger "Any: Cap=Win,Des=Lose" is now also seen as flag to autodetect classic single play scenarios.
* On Tiberian Dawn maps, the editor will now fall back to Temperate graphics when not finding the Winter graphics for the Haystack buildings/overlays. This happens the same way in the game, since the original igloos that were supposed to be there were sadly not remastered.
* Fixed a situation where triggers were selectable on unbuilt buildings.
* Improved the look of the trigger info icon on Terrain object properties and in the Celltriggers window. This was already done on other objects.
* Added a dialog for the image export where the user can slect the specific layers and set the scale factor. There is a tool to set the dimensions in pixels, but the internally used metric is the scale factor, so the final size will not always match that input.
* Middle mouse and space no longer make the cursor change on the map tile preview panel.
* Added multithreading to all heavy processing functions. This means the window will no longer freeze up while loading or saving maps, but will instead have all functions disabled, while showing a little box that shows what it is doing.
* In Celltrigger mode, if a selected trigger is already linked to objects, the trigger labels on these objects will be indicated in yellow.
* Changed references to the "ConcretePavementTD" mod in "CnCTDRAMapEditor.exe.config" to its new name; "GraphicsFixesTD".
* Changed tool clamping logic to only need a minimum size to remain inside the window, rather than the entire tool. This minimum can be set in the setting "MinimumClampSize" in "CnCTDRAMapEditor.exe.config".
* Steam publish dialog now has buttons to easily copy the map name and briefing from the mission.
* Steam publish dialog description now properly supports multiline.
* Steam publish dialog will now properly restore the location of a previously-used custom generated thumbnail.
* In addition to the Remaster's one "Text=" line briefing, the editor now writes the classic style briefings into maps too, as "1=", "2=", etc. lines split at human-readable length This classic-style briefing can be disabled with the setting "WriteClassicBriefing" in "CnCTDRAMapEditor.exe.config".
* Directions for vehicles are now limited to 8, as they are in the actual game.
* Re-enabling a building's "prebuilt" status on TD maps will now set the House to the classic opposing House, rather than always defaulting to the first item in the list.
* Added "Tools" → "Statistics" menu to house the "Power Balance" and "Silo Storage" tools, and added a "Map Objects" tool giving an overview of used objects.
* Renaming triggers and teamtypes will now correctly apply the renames to all things linking to them.
* Fixed a bug on RA triggers where Event 2 was not hidden if the first opened trigger was one that should hide it.
* Added the ability to clone triggers and teamtypes.
* For Red Alert speech and sound triggers, the original filenames are now shown alongside the description.
* For Red Alert text triggers, the text ID is now shown in front of the text.
* Trigger and teamtype editing actions are now added to the undo/redo history. They will give a warning with a message box when undoing or redoing.
* Fixed the fact the maximum number of triggers was set to 200 for Red Alert, instead of 80.
* Red Alert globals are now limited to 0 to 29, since that is how they are defined in the game code.
* The editor now specifically checks for the presence of the [Basic] and [Map] sections to see whether a file is indeed a C&C map.
* Added checks on triggers containing Events or Actions that don't have their required House/Teamtype/Trigger filled in.
* The automatic clearing of the obsolete clear terrain in RA1 will no longer mark the map as modified.
* Red Alert solo mission detection now correctly takes into account win and lose triggers set to non-player houses.
* The Red Alert trigger action "Spied by..." was changed to "Spied on by anybody", and its house argument was removed.
* Added new indicator for "Map symmetry".
* Fixed bug in reporting on which cell objects are overlapping.
* When clearing a trigger from a unit or building, the error screen will now report the cell of the affected object.
* Unit and structure reading now happens before terrain and overlay, so in the event of an overlap, the units or structures will be preserved.

#### v1.4.4.1:

--Unreleased--

* Added igloos (haystacks) to the Overlay in Sole Survivor's Winter theater.
* Fixed refresh bug where a ghost image of the label indicating a heavy operation remained while repainting the map. The label is now only removed after the repaint.

