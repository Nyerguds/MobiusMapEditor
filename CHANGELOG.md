# Mobius Map Editor - Change log

## Features added by Rampastring:

* Downsized tool graphics by a user-configurable factor so you can see more placeable object types at once on sub-4K monitors.
* Improved zoom levels.
* Fixed a couple of crashes.
* Made tool windows remember their previous position upon closing and re-opening them.
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks.

## Features and fixes by Nyerguds:

### v1.4.0.0:

Released on 08 Jul 2022 at 21:37 UTC

* Fixed overlay height overflow bug in Rampa's new UI.
* Fixed map tiles list duplicating every time the "Map" tool window is opened in Rampa's version.
* Added CONC and ROAD pavement to TD. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in team types.
* Removed the Aircraft from the placeable units in TD. \[NOTE: made into a setting in v1.4.3.0\]
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc).
* Fixed case sensitivity related crashes in TD team types.
* TD triggers without a team type will now automatically get "None" filled in as team type, fixing the malfunctioning of their repeat status.
* Added \[Ctrl\]+\[N\], \[Ctrl\]+\[O\], \[Ctrl\]+\[S\] etc. shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tool window to adjust crater stage.
* Fixed TD terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
* RA "Spied by..." trigger event now shows the House to select. \[NOTE: reverted in v1.4.4.0\]
* Added "Add" buttons in triggers and team types dialogs.
* Fixed tab order in triggers and team types dialogs.
* Fixed crash in "already exists" messages for triggers and team types.
* Randomised tiberium on save, like the original WW editor does. (This is purely cosmetic; the game re-randomises it on map load.) \[NOTE: changed to fixed seed in v1.6.0.0\]
* Added ability to place bibs as smudge type. They only show up as a top-left corner though. \[NOTE: properly implemented in v1.4.3.0\]

### v1.4.0.1:

Released on 13 Jul 2022 at 07:35 UTC

* Added "All supported types (\*.ini;\*.bin;\*.mpr)" as default filter when opening files.
* Added Drag & Drop support for opening map files.
* Added command line file argument support, which allows setting the editor as application for opening ini/mpr files.
* Changed the order of the choices for the "Edge" setting of the Houses to a more logical North, East, South, West.
* House Edge reading now corrects values with case differences so they show up in the dropdown.
* Fixed order of the Multi-House colors. It seems the error is not in the editor, but in bizarre mixed-up team color names in the remastered game itself.
* Remapped Neutral (TD only) and Special as yellow, as they are in the game.
* All tool windows will now save their position.
* Tool windows for which no position was previously set will center themselves on the right edge of the editor.
* Some things, like crates, were missing names. This has been fixed.
* All objects except map tilesets will now show a real name and their internal code.
* Added ASCII restriction to trigger and team type names, since the map formats don't support UTF-8. (Except on the Briefing, apparently, since the GlyphX part handles that.)
* Made "Already exists" check on trigger and team type names case insensitive, since that is how the game handles them.
* Triggers and team types dialogs have a new logic for generating names for new entries that should never run out.
* Triggers and team types dialogs support the delete key for deleting an entry in the list.
* Triggers and team types dialogs have "Rename" added to the context menu when right-clicking an item.
* Triggers and team types dialogs now warn when cancelling if changes were made.
* "Add" button in triggers and team types dialogs gets disabled when the internal maximum amount of items for the type is reached.
* Changed the default build level in TD maps from 99 to 98. Level 99 allows building illegal objects that can break the game.
* The Briefing text area will now accept \[Enter\] for adding line breaks without this closing the window. Previously, \[Ctrl\]+\[Enter\] had to be used for this, which is pretty awkward.
* The Briefing text area now has a scrollbar.
* Fixed placement of illegal tiles caused by incorrect filtering on which tiles from a template should be included. This is the problem which caused tiles that showed as black blocks in classic graphics. It is also the problem that made RA maps contain indestructible bridges.
* Map tile placement can now be dragged, allowing easily filling an area with water or other tiles. This also works for removing tiles.
* Removing tiles will now obey the actual occupied cells of the selected tile, rather than just clearing the bounding box, making it more intuitive.
* Creating an RA trigger with Action "Text Trigger" will no longer cause an error to be shown.
* Trigger controls no longer jump around slightly when selecting different options.
* Using the mouse wheel will now change the tiberium field size per 2, like a normal arrow click would.

### v1.4.0.2:

Released on 14 Jul 2022 at 20:22 UTC

* Fixed bug that cleared all map templates on save in v1.4.0.1 (whoops).
* Fixed bug in the team types list that showed the wrong context menu options on right click.
* Fixed bug that the status bar did not show the map placement shortcuts hints on initial load.
* The editor no longer exits if it cannot connect to Steam. Instead, workshop publishing will simply be disabled if the Steamworks interface can't be initialised.
* The texture manager will now properly dispose all loaded image objects when a different map is loaded.
* Added \*.ini to the list of possible extensions for saving RA maps, to support opening pre-Remaster missions.
* If a building has no direction to set and shows no dropdown for it, the "Direction" label is now also removed.
* Structure graphics are now correctly centered on their full building size.
* The damaged state of buildings is now shown at strength values of 128 and below, rather than only below that value. \[NOTE: adjusted further in v1.4.4.0\]
* Damaged states now work correctly on all buildings, with a vastly simpler and more general internal logic.
* Using the mouse wheel will now change the strength of objects in increments of 4.
* IQ of all Houses in RA now defaults to 0.
* Fixed TD Gunboat facing and damage states logic.
* Fixed bug causing bad refresh when previewing the placement of a single cell selected from a template with an empty top right corner cell.
* The "clear1" tile is now explicitly shown in the tiles list. It acts as 1x1 eraser.
* Team type "Priority" value (recruit priority) is now capped at 15. \[NOTE: reverted in v1.4.0.3; did not reflect reality\]

### v1.4.0.3:

Released on 27 Jul 2022 at 09:47 UTC

* The editor now tries to automatically detect the game installation folder in Steam.
* Fixed refresh errors in preview images when resizing tool windows.
* All overlay items will now show a preview icon of the same size.
* Fixed errors in tree sizes.
* The 'clamping' logic that prevented tool windows from being dragged outside usable screen bounds had a bug that this prevented it from being dragged onto a different monitor. This is now fixed.
* Added "Theme" to the map settings.
* Removed "Percent" from the map settings. It is an unused Dune II leftover.
* Added "Classic only" labels to "Carryover Money" and "Theme" to indicate these options will only work when playing the missions in the original game.
* All videos available in the Remaster are now shown in the video lists in the "Map settings" dialog. \[NOTE: reverted for RA in v1.4.1.0\]
* Added missing entries (videos not included in the Remaster) to the RA and TD video lists, with a 'Classic only' indicator.
* In the team types dialog, the rather confusing use of the internal name "Missions" was changed to a more intuitive "Orders".
* Added tooltips for all team type options.
* Team type orders now show a tooltip on the Argument field indicating the meaning of the value to give, and, if needed, the possible values to choose from. \[NOTE: replaced by choice lists in v1.4.1.0\]
* Fixed tab order of the team type options.
* The dropdowns in the grids in the team types dialog now respond without having to click multiple times.
* Removed the previously-added cap on the team type "Priority" value after feedback from users and checking the source code.
* The CONC and ROAD overlay types now show the same graphics as in-game. This is technically just a dummy graphic the game uses when not finding object graphics. The version in the editor is a reconstruction.
* Removed limitation on placing resources on the top and bottom row of the map.

### v1.4.1.0:

Released on 20 Aug 2022 at 22:37 UTC

* Fixed dimensions of RA's ore mine, Snow theater ice floes and Interior theater boxes, and one of the Desert theater rocks in TD.
* Added \*.ini to the list of possible extensions for opening RA maps. Apparently before I only added it for saving.
* The editor will now accept nonstandard extensions from drag & drop without any issues. For TD maps, it will need to find the accompanying bin or ini file with the correct extension.
* Files opened from filenames with nonstandard extensions will not change these extensions when saving the file. This also means RA maps opened from a .ini file will no longer change the extension to .mpr when saving.
* Terrain objects will now only pop up a properties box for setting a trigger on TD maps.
* Optimised loading so the editor will skip loading objects from different theaters.
* User settings (game folder, invite warning, and the dialog locations) will now be properly ported over from previous versions.
* Added support for loading mod xml info and graphics through the "ModsToLoad" setting in "CnCTDRAMapEditor.exe.config". The syntax is a semicolon-separated list, with each entry either a Steam workshop ID, or a folder under "Documents\CnCRemastered\Mods\". As folder, the path must contain the "Tiberian_Dawn" or "Red_Alert" part at the start. That prefix folder will also be used as consistency check for the mod type as defined inside "ccmod.json". Mods given by folder name will also be looked up in the Steam workshop folders, with the prefix folder used only for the consistency check. Mods do NOT have to be enabled in the game to work in the editor. \[NOTE: game prefix requirement for paths removed when this was split into settings per game in v1.4.4.0\]
* Added support for the unique pattern of TD's "conc" pavement. You will need the "ConcretePavementTD" mod to actually see that, though. This mod is filled in by default in the editor's mod loading settings, meaning it will automatically be used if found. \[NOTE: mod name changed to GraphicsFixesTD in v1.4.4.0\]
* Fixed loading and saving of the videos set in the map options dialog, so no more errors pop up there.
* Reverted videos list for Red Alert; the game can only handle videos that are in its internal hardcoded list.
* Made video names freely editable for TD missions. Any mod-added video in TD is playable from missions. Be warned that when a video is not found, this may cause the game to hang for several minutes.
* The preview selection in the Steam publish dialog will now open in the correct folder.
* The new setting "NoMetaFilesForSinglePlay" in "CnCTDRAMapEditor.exe.config" will suppress the generation of .json and .TGA file when saving single player missions to disc. Not writing them is now the default behavior. This does not affect the Steam workshop upload behavior.
* The rendered previews will now show all map contents, to give a better representation of what is on the map. Note that for single play missions, this preview is generated in the folder but is optional.
* Removed crater types CR2 to CR6; they don't work correctly in either game and will just show the smallest size of CR1. Any craters of other types encountered on map load will now be converted to CR1. \[NOTE: made into a setting in v1.4.3.0\]
* The team types dialog no longer uses data grids for its teams and orders.
* Team types now show full names for unit types.
* The input for arguments for orders in the team types dialog now correctly adapts to the type of each order, giving dropdowns for special choices lists and for waypoints.
* The waypoints that can be selected for an RA team type now correctly start from -1 as "(none)".
* Fixed color of "Special" in RA to have the same color as Spain.
* Fixed trigger Events and Actions retaining their argument data when changing their type, causing the UI to pick the equivalent data on whatever list or control popped up for the new type.
* RA triggers now show human-readable data for the Event and Action arguments.
* The editor no longer locks up when the triggers dialog shows an empty list of team types or (previously-saved) triggers because none were made yet.
* Removed Aircraft section handling. Aircraft were never able to be pre-placed in the original game, and the re-enabled sections in the Remasters have issues; aircraft will still spawn in the air and fly somewhere close. \[NOTE: made into a setting in v1.4.3.0\]
* Like walls, overlay placement and removing can now be dragged to affect multiple cells.
* All waypoint will now be shown with their coordinates.
* Added "Jump to" button on the waypoints tool. This will only have any effect when zoomed in.
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
* In Tiberian Dawn, buildings where "Prebuilt" is disabled will now show as House "None", with black team color, since Tiberian Dawn has no real restrictions on which House can build these.
* The Interior tiles "wall0002" to "wall0022" are now grouped per type into three dummy-entries "wallgroup1", "wallgroup2" and "wallgroup3", to simplify random placement of these tiles and to remove clutter from the tiles list.

### v1.4.1.1:

Released on 22 Aug 2022 at 09:28 UTC

* The Red Alert team type order "Guard Area" now correctly has 'time' as argument type, rather than a waypoint.
* Added a system to detect singleplayer missions from the original games and automatically mark them as singleplayer if they conform to the classic naming scheme for singleplayer missions, and contain a Lose and Win trigger.
* Functions asking to save unsaved changes (New/Open/Publish/Close) will now actually abort if you choose to save the unsaved opened map but it doesn't pass the basic waypoints validation, rather than giving the validation fail message and then continuing anyway.
* The title of the window will now show "Untitled.ini" or "Untitled.mpr" when you have a new but unsaved map opened. \[NOTE: changed from filename to map name in v1.4.4.0\]
* The title of the window will now show an asterisk behind the filename to indicate that the current file has unsaved changes.
* Maps loaded from file are now seen as 'modified' if any issues were detected that made the editor change or discard data during the loading process.
* The triggers check feedback (TD only) now also uses the large window used for showing the map load errors.

### v1.4.2.0:

Released on 05 Sep 2022 at 14:25 UTC

* Menu items related to map actions will now be disabled if no map is loaded.
* Fixed "Destroy attached object" trigger in RA not being seen as valid on units.
* Added power balance evaluation tool.
* If a map is saved without a map name set in the map settings, the filename will be filled in as name.
* Expanded the "View" menu to enable and disable drawing for all possible map elements and indicators on the map.
* Vehicle previews are now shown in a more dynamic south-west facing.
* When a map is opened, the editor will load theater-specific icons into the toolstrip.
* Resource placement is now disabled in Interior theater.
* Map loading now checks if map objects exist in the specified theater. \[NOTE: made into a setting in v1.4.3.0\]
* An image export function has been added. This will mirror the current items enabled in the "View" menu. Its size is determined by the "ExportScale" setting in "CnCTDRAMapEditor.exe.config". \[NOTE: made into a full dialog, with the setting changed to DefaultExportScale, in v1.4.4.0\]
* Fixed a glitch that made the trigger dropdown of the opened tool stop functioning correctly after editing the triggers.
* Template 'BRIDGE1H' in RA now shows its full available tileset. Seems this is an odd corner case where Westwood were the ones who forgot to cut it out properly, but that does make its two last tiles valid.
* The editor will now detect when, on Red Alert maps, the obsolete tile with id 255 is used as 'clear' terrain, and will only show a single message about it. There is an "IgnoreRaObsoleteClear" setting in "CnCTDRAMapEditor.exe.config" to disable filtering out this tile, though that is only useful for research purposes.
* Fixed the map panel crashing when a repaint is done on an already-closed map. This happened when a map was opened, then another one was loaded but showed the load errors dialog, and then something (like "Show Desktop") triggered a repaint of the editor.
* Changed all scaling settings to floating point numbers, making them much more accurate.
* Building labels for fake buildings and for the rebuild priority will now scale correctly with the map scaling settings, meaning they should always remain the same size relative to the building.
* Indicator lines like the map border, cell occupation indicators and the yellow selection box will now scale with the map scaling settings, meaning they don't become bigger when reducing the graphics scale.
* Aftermath units can now be enabled and disabled in the "Map Settings" window. Disabling this will clear any expansion units from the map and from the team types.
* When Aftermath units are detected on map load, but the Aftermath units setting was not enabled in the ini, the loading system will enable the Aftermath units setting.
* All waypoints in the triggers and team types dialogs now show their coordinates.
* Vastly optimised map bounds dragging.
* Map bounds dragging will no longer revert when releasing the \[Ctrl\] key before the mouse button.
* The label shown when dragging the map border now appears on the inside of the bounds, exactly a cell away from the bounds, and only updates on every cell change.
* Map bounds dragging will change the border at the moment you cross the halfway point between cells, rather than when entering a new cell, making it much more intuitive.
* Drag-scrolling, which is normally middle mouse button, now also works by holding down the space bar, to support devices (like laptops touch pads) without middle mouse button.
* Drag-scrolling is now much more accurate.
* Fixed an issue in the triggers where data got reset if you switched between two triggers that both had a field that contained the same numeric data.
* Team types now show the House in the list.
* Team types in Red Alert maps now filter out the triggers list to unit-applicable triggers only.
* Team types and triggers can now be selected by clicking anywhere on their row, rather than having to click specifically on the text in the row.
* The check on multiplayer waypoints being placed down now correctly checks only the specific player start waypoints, rather than just any waypoints including the special singleplay ones.
* The possible multiplayer waypoints to place on a map now go from 0 to 15. \[NOTE: reverted in v1.4.3.2\]
* If the map is marked as single player scenario, the first waypoints are no longer indicated as player start positions with a "P" prefix.
* Mods will now only be loaded for maps targeted at their respective game, meaning common assets can be overridden differently by TD and RA mods. \[NOTE: mod settings split up per game in v1.4.4.0\]

### v1.4.3.0:

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
* Fixed bug where tool windows can be closed with \[Alt\]+\[F4\], causing the editor to crash when trying to re-open them.
* Fixed tab order on the "New Map" dialog, so the radio buttons are selected by default.
* Mobile Radar Jammer and Mobile Gap Generator now show different facings for their "turrets".
* Fixed bug in the power balance tool which made it ignore the first House.
* Added silo storage capacity tool.
* Added section in the map settings for scenario-specific options in RA maps.
* Added rules editing field for RA maps that allows editing/adding ini sections not handled by the editor. Changing building bibs, power and resource storage in this will immediately affect the editor.
* Dragging a building's bib over smudge will no longer remove the smudge, unless it's actually placed down on it.
* Undoing a building's placement or moving will now restore any smudge the building's bib replaced.
* Added map template flood fill mode (\[Ctrl\]+\[Shift\]+\[Left-Click\]) and flood fill clear mode (\[Ctrl\]+\[Shift\]+\[Right-Click\]).
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
* While holding \[Ctrl\] in Map mode to enable bounds editing mode, diagonals will now be drawn inside the bounds rectangle to easily see the center. \[NOTE: expanded to full map symmetry indicators in v1.4.4.0\]
* While holding \[Ctrl\] in Map mode to enable bounds editing mode, the whole bounds rectangle can now be moved by clicking inside it and dragging it around.
* While holding \[Ctrl\] in Map mode to enable bounds editing mode, you will no longer select tiles when clicking.

### v1.4.3.1:

Released on 14 Sep 2022 at 16:52 GMT

* Fixed a crash when flood-clearing with a template that crossed the map bounds.
* When flood-clearing with a template containing cells on both sides of the map bounds, the clear operation will now ignore the map bounds.
* Flood fill clear will now no longer ignore if the user has a single cell from a template selected.

### v1.4.3.2:

Released on 14 Sep 2022 at 21:20 GMT

* Fixed a crash in the smudge restore system when you delete a smudge or building that is too close to the map edge.
* Reduced maximum multiplayer start positions in the editor to 8, since the games apparently can't show more.

### v1.4.4.0:

Released on 14 Nov 2022 at 22:25 GMT

* When your mouse cursor is inside the map bounds and you press \[Ctrl\] in Map mode to enable bounds editing mode, your cursor will now immediately change to the Move cursor, without requiring any mouse movement.
* The status bar at the bottom will now explicitly mention the sub-position of the infantry under the mouse cursor.
* When loading a map, if a map's file name identifies it as classic single player mission, this will no longer mark the mission as "modified" by the loading process. This will make it simpler to open classic maps for reference without getting a save prompt on close. Do note that lots of classic maps contain errors in triggers being linked to wrong objects, and the automatic fixes for that **will** still mark the map as modified.
* Fixed issues with the editor window getting focused simply by moving the mouse over it. The main window can steal focus from the tool window, but not from other applications.
* In Waypoints and Cell Triggers editing mode, the \[PageUp\], \[PageDown\], \[Home\] and \[End\] keys will now let you go through the dropdown items list. \[PageUp\] and \[PageDown\] will act as normal arrow keys.
* In Waypoints editing mode, pressing \[Shift\] and a key will select special waypoints: \[F\]lare, \[H\]ome, \[R\]einforce, \[S\]pecial.
* The shortcut keys to select the different editing modes (normally Q, W, E, R, T, Y, A, S, D, F, G) now work on keyboard position, meaning they will also work in the same logical way on AZERTY keyboards.
* Fixed bug where the checked states of the Houses in the Map Settings would reset when changing the "Single-Player" checkbox.
* The \[Aftermath\] section is no longer ignored in the map settings' Rules editor, so Aftermath detail settings can be added. The actual "NewUnitsEnabled" setting in this is ignored, though; toggling the expansion units can only be done in the Basic settings.
* Undo/Redo tracking will now also undo the map's modified-status.
* Added support for the C&C95 v1.06 briefing line split format.
* Added Tiberian Dawn Megamap support. (Sole Survivor map type)
* Made the editor store its dll mess in a subfolder, like the retail version does.
* Sole Survivor is now supported as separate game type in the editor. Its waypoints include four special crate spawning hotspots. These maps don't support owned objects (infantry, units, structures) by default, though this can be re-enabled using the "NoOwnedObjectsInSole" setting in "CnCTDRAMapEditor.exe.config". Crates are always disabled, however, and cannot be enabled.
* The damaged state of buildings now works correctly per game; for TD it shows below 50%, for RA it shows at 50%.
* Changed waypoints to actual map objects, indicated using the green beacon graphic that was already used as icon for the Waypoints editing mode. They can be disabled in the View menu.
* Waypoint labels are now drawn at the bottom of the cell, in the same style as the building rebuild priority labels. They can be disabled in the View menu, but like the building labels, they will not be drawn if Waypoints are not enabled.
* Multiplayer starting points are now shown as colored flags. For TD, Nod's metallic blue will be used for the 7th flag (P6). For SS, which has classic color order configured, metallic gray is the 4th flag (TM3), and the new bright blue is the 7th (TM6). For both TD and SS, the 8th flag has its value hardcoded as the purple from RA.
* Waypoints now show a preview in placement mode.
* On Sole Survivor maps, there is a special "Football goal areas" indicator that shows how much area around the flag needs to be left open to be paved with concrete in Football mode. These can be disabled under "View" → "Indicators".
* The game name of the opened map type is now shown in the title bar.
* Changed the editor name in the title to "Mobius Map Editor".
* Red Alert maps are now specifically detected on the presence of the "\[MapPack\]" section. If this is not present, and there is no .bin file, it loads as TD map without map templates.
* Restricted Red Alert trigger and team type names to the same lengths as Tiberian Dawn; 4 for triggers, 8 for team types.
* Pressing \[Enter\] in Waypoints mode will now jump to the selected waypoint.
* Fixed bug in the overlap detection system that made it always give "&lt;unknown&gt;" for the overlapped cell on Terrain objects.
* Split mods up into ModsToLoadTD, ModsToLoadRA and ModsToLoadSS. Entries in the list no longer require the game folder prefix.
* The Civilian buildings V12 and V13 (haystacks) are now also available in TD Winter theater.
* The trigger "Any: Cap=Win,Des=Lose" is now also seen as flag to autodetect classic single play scenarios.
* On Tiberian Dawn maps, the editor will now fall back to Temperate graphics when not finding the Winter graphics for the Haystack buildings/overlays. This happens the same way in the game, since the original igloos that were supposed to be there were sadly not remastered.
* Fixed a situation where triggers were selectable on unbuilt buildings.
* Improved the look of the trigger info icon on Terrain object properties and in the Celltriggers window. This was already done on other objects.
* Added a dialog for the image export where the user can select the specific layers and set the scale factor. There is a tool to set the dimensions in pixels, but the internally used metric is the scale factor, so the final size will not always match that input.
* Middle mouse and space no longer make the cursor change on the map tile preview panel.
* Added multithreading to all heavy processing functions. This means the window will no longer freeze up while loading or saving maps, but will instead have all functions disabled, while showing a little box that shows what it is doing.
* In Celltrigger mode, if a selected trigger is already linked to objects, the trigger labels on these objects will be indicated in yellow.
* Changed references to the "ConcretePavementTD" mod in "CnCTDRAMapEditor.exe.config" to its new name; "GraphicsFixesTD".
* Changed tool clamping logic to only need a minimum size to remain inside the screen, rather than the entire tool. This minimum can be set in the setting "MinimumClampSize" in "CnCTDRAMapEditor.exe.config".
* Steam publish dialog now has buttons to easily copy the map name and briefing from the mission.
* Steam publish dialog description now properly supports multiline.
* Steam publish dialog will now properly restore the location of a previously-used custom generated thumbnail.
* In addition to the Remaster's one "Text=" line briefing, the editor now writes the classic style briefings into maps too, as "1=", "2=", etc. lines split at human-readable length. This classic-style briefing can be disabled with the setting "WriteClassicBriefing" in "CnCTDRAMapEditor.exe.config".
* Directions for vehicles are now limited to 8, as they are in the actual game.
* Re-enabling a building's "prebuilt" status on TD maps will now set the House to the classic opposing House, rather than always defaulting to the first item in the list.
* Added "Tools" → "Statistics" menu to house the "Power Balance" and "Silo Storage" tools, and added a "Map Objects" tool giving an overview of used objects.
* Renaming triggers and team types will now correctly apply the renames to all things linking to them.
* Fixed bug on RA triggers where Event 2 was not hidden if the first opened trigger was one that should hide it.
* Added the ability to clone triggers and team types.
* For Red Alert speech and sound triggers, the original filenames are now shown alongside the description.
* For Red Alert text triggers, the text ID is now shown in front of the text.
* Trigger and team type editing actions are now added to the undo/redo history. They will give a warning with a message box when undoing or redoing.
* Fixed the maximum number of triggers being set to 200 for Red Alert, instead of 80.
* Red Alert globals are now limited to 0 to 29, since that is how they are defined in the game code.
* The editor now specifically checks for the presence of the \[Basic\] and \[Map\] sections to see whether a file is indeed a C&C map.
* Added checks on triggers containing Events or Actions that don't have their required house/team type/trigger filled in.
* The automatic clearing of the obsolete clear terrain in RA1 will no longer mark the map as modified.
* Red Alert solo mission detection now correctly takes into account win and lose triggers set to non-player houses.
* The Red Alert trigger action "Spied by..." was changed to "Spied on by anybody", and its house argument was removed.
* Added new indicator for "Map symmetry".
* Fixed bug in reporting on which cell objects are overlapping.
* When clearing a trigger from a unit or building, the error screen will now report the cell of the affected object.
* Unit and structure reading now happens before terrain and overlay, so in the event of an overlap, the units or structures will be preserved.

### v1.4.5.0:

Released on 03 Apr 2023 at 19:20 GMT

* Added igloos (haystacks) to the Overlay in Sole Survivor's Winter theater.
* Fixed refresh bug where a ghost image of the label indicating a heavy operation remained while repainting the map. The label is now only removed after the following map repaint.
* Fixed issues with the editor not loading old missions with DOS special characters in them. Specific ini sections are now loaded and saved in specific text encodings; the maps are normally DOS-437, but their remaster-specific contents support UTF-8.
* Fixed an issue with TD triggers not linking to team types if the team type reference has case differences compared to the actual team type name.
* Added turrets to RA ships, and rotors to helicopters.
* When air units are enabled to be placed down, this no longer excludes winged airplanes. They'll just fly off the map, but that could be used for its cinematic effect in missions.
* Added the ability to open .pgm archives, to easily check the contents of packed workshop maps. Export to this format is not supported, though.
* Restored some of the "useless" orders possible to set on preplaced units; it turns out "Unload" on air units is the only order that makes them land on the intended spot.
* The drawing order of overlapping objects is now based on the center of the graphics, rather than the bottom, which means buildings or trees will no longer overlap objects placed on their lower but unoccupied cells.
* Repair Facilities are now treated as flat buildings, meaning they won't overlap things placed on their top cells.
* Increased the size of waypoint labels.
* When holding the mouse over a bib, the status bar will now also show if it is attached to a building.
* The "Rules" section in the map settings has been renamed to "INI Rules & Tweaks", and is now also available for TD.
* Custom ini keys that are added to the \[Basic\] and \[Map\] sections, and to any of the House sections, are now preserved, and editable in the "INI Rules & Tweaks" section. This will allow passive support for modded features.
* Previews are now shown for Celltrigger placement.
* Fixed errors in the detection of Aftermath expansion units on maps.
* Fixed map load collision detection of multi-cell objects to detect crossing outside the map.
* Fixed the NoOwnedObjectsInSole option to also correctly toggle loading and saving of these objects.
* Added option in image export to only export the map bounds. This is enabled by default for multiplayer maps.
* The Map Objects Statistics tool now shows the classic game limits for TD maps. (RA has no changes in these.)
* Disabled the ability to export to .meg type via the menu shortcut when the editor is not compiled in developer mode.
* Removed confusion on the Loop / Existence / Persistence setting in triggers, by renaming it to "Executes", with choices "On first triggering", "When all linked objects are triggered", and "On each triggering".
* Red Alert's multi-event types for triggers are now written out in full rather than using programming symbols like '||' and '&&'.
* Added trigger filter tool, with search criteria for house, execute type, multi-event type, event, action, team type and global.
* Deselecting the selected tile in the Map tool by clicking in the blank space will now properly show that that reverts the selection to Clear terrain.
* The total map resources value on the resource placement tool will now be calculated correctly (including the game bug that makes the last harvested stage never give any money).
* The resource placement tool will now specifically show a value for the total map resources accessible inside the map bounds.
* Reinstated the restriction on placing overlay on the first and last row on the map, to correctly emulate the fact the games do not place overlay there.
* Added "New map from image" feature, so the rough layout of a map can be designed in an image editor. Each color in the image can be assigned to a tile.
* A new "Safe tile dragging" feature ensures that drag-placing a line of tileset pieces will now tile them without overwriting most of the placed content on each new moved cell.
* Red Alert Interior Theater no longer allows placing down Smudge types.
* Double-clicking on map objects with mouse buttons other than the left one will no longer open the object properties.
* Fixed bug treating the Commando as unarmed unit, defaulting its orders to "Stop" instead of "Guard". (Not that this has any effect in-game.)
* Fixed map panels (such as the main map and the preview showing the template to place) affecting the mouse cursor on a global scale rather than only inside their own panel.
* Fixed bug that made the cursor change when in map bounds editing mode but the cursor is outside the valid map area.
* On the Map tool, selection of the specific sub-cell now responds correctly when holding down the mouse button and dragging to different cells.
* All picking of objects on the map (usually right-click) now responds correctly when holding down the mouse button and dragging to different cells.
* Being in placement mode, or dragging around objects, will now show a placement grid. In Resources mode, where holding down Shift is not required for placement, holding Shift will also show the grid.
* Fixed bug in Smudge mode where clicking on a crater always selects the crater's maximum size in the placement template.
* Fixed bug in Smudge mode where the automatic restoring of partially deleted bibs only worked inside the map bounds.
* Added correct map objects overview and save validation for Sole Survivor, which does not use the expanded Remastered maximum values.
* Added more options in the settings file, and reordered the settings in the settings file and manual file.
* Fixed bugs that occurred when disabling the theater-filtering of items.
* Pressing Alt+F4 to close the editor will now also work when a tool window is selected.
* The tool windows for infantry, units and structures will now optimally use the available space.
* Added option to randomise drag-placed map tileset blocks using equivalent tileset blocks.
* Added logic to always draw crates on top of other objects.
* Added automatic logic for Red Alert maps to fix corrupted tiles such as the "indestructible" bridges, rather than just clearing them.
* Skipping over disabled sections in the ini (like the Aircraft) now adds a remark to the errors list and marks the map as modified, to alert the user that data will be lost on re-save.
* Added detection for incorrect use of the "Allow Win" action in Tiberian Dawn.
* Fixed reading of \[Base\] entries; it should loop over all 3-digit integers up to the "Count" value, not parse the keys as integers.
* Fixed triggers list corrupting on the Terrain tool when editing the triggers after the Terrain tool had already been loaded.
* Fixed trigger not showing on the preview for placing down a Terrain object.
* The trigger on the placement preview of a Unit, Building, Infantry or Terrain object is now shown as semitransparent.
* Choosing to save on a save prompt will now no longer abort the action that prompted the save prompt; it will be remembered and executed after the save is done.
* Fixed saving of Steam publish ID in the map publishing process.
* The preview path text field in the Steam Publish dialog is now read-only.
* The map preview in the Steam Publish dialog will now consistently show when hovering over the preview path text field.
* Fixed saving of '@' characters in the Steam workshop item description.
* The map save after the Steam publish will no longer re-generate the thumbnail, making it much faster.
* The Steam publish operation will now clean up all generated temporary files.
* Fixed the issue where the object property popup does not properly show all options. This was caused by it using a different font.
* When unchecking the Prebuilt status of a building and then re-checking it, the original values for the building's other properties (owner, strength, trigger...) will be restored. This does not work if the changes were confirmed by closing the popup. This property-restoring also works on the selection window template.
* Added indicators for reveal radiuses around waypoints, and shroud / jam radiuses around Gap Generators, Mobile Gap Generators, Radar Jammers and Tesla Tanks. Unless specifically enabled in the "View → "Extra Indications" menu, these will only be shown when manipulating the objects directly; while placing them, moving them, and editing their properties.
* Added "Jump to next usage" button on celltriggers, which, when pressed multiple times, will cycle the viewed map location through all different placed-down clusters of the selected celltrigger.
* Added Word Wrap button to the error message dialog.
* When a new unit type is added to a team type, its amount will now default to 1.
* Optimised the repaint process when enabling or disabling layers.
* Added "Outlines on overlapped crates" option under "View" → "Indicators". An extra option "Tools" → "Options" → "Show crate outline indicators on all crates" will make it ignore the "overlapped" requirement and show on all crates.
* Fixed issues with videos sometimes not being saved / loaded for Red Alert missions.
* When loading a map, the editor will zoom in and reposition to exactly show the map bounds area with a 1-cell border around it. This behaviour is controlled by the "ZoomToBoundsOnLoad" setting.
* Items are now sorted by map cell number when saved, and capitalised like they are in original maps.
* Waypoints in Tiberian Dawn maps are now saved as they were in classic maps, with all waypoints listed last to first, and the unused ones filled in with '-1'.
* Optimised the redrawing when enabling/disabling layers in "View" → "Layers".
* Celltriggers are now slightly more transparent when outside Celltrigger editing mode.
* Added character count to the briefing screen. if WriteClassicBriefing is enabled a warning wil be given if the amount exceeds the maximum the classic game can handle. If the warning is ignored, the classic briefing will truncated on the maximum it can handle, to prevent game crashes.
* Fixed saving of Red Alert's classic briefing, to also obey the classic internal maximum, and to correctly split on line break @ characters.

### v1.5.0.0:

Released on 09 Jul 2023 at 14:58 GMT

Feature updates:

* Added support for reading and using classic game files, making the editor independent from the C&C Remaster.
* Added the ability for classic mode to use classic mods, reading files from the "ccdata" folder in any C&C Remastered mod. This includes the ability to read any sc*.mix files that happen to be in these folders.
* Vastly optimised editor responsiveness by only drawing map indicators inside the shown area, rather than processing them for the whole map.
* Optimised preview generation by not rendering the preview in full resolution first.
* Added an option under "Extra Indicators" to show the map tile passability. This will also be shown on the map tile tool's preview pane.
* Preview generation will now add waypoint flags to multiplayer maps.
* Changed "outlines on overlapped crates" option to "outlines on overlapped objects", and made it work for units and infantry too.
* Pressing the \[PageUp\] and \[PageDown\] buttons while the main window is selected will now consistently move through the tool's item choices, in all editing modes.
* Removed \[ and \] as shortcuts to affect resource paint size because they did not work consistently on foreign keyboards. The functionality was also changed to \[PageUp\] and \[PageDown\].
* The Resources tool no longer evaluates resources placed outside the map bounds. Resources outside the map are now always shown at their minimum size, and tinted red, to indicate they don't have any impact on the map.
* Added a reference to the GraphicsFixesRA mod in the ModsToLoadRA setting.
* Added "EnforceObjectMaximums" setting that can be disabled to remove save checks on the game engine's object maximums.
* In multi-monitor environments, the editor will now always open on the monitor where the mouse cursor is.
* The "Visibility" value in the Steam section now saves as simple number (0=public, 1=friends, 2=private). The old long text lines can still be interpreted though.
* Added a logic to reduce cell edge artifacts when exporting as smoothed image.
* Added checks on the validation of special waypoints to make sure they are actually inside the map bounds.
* Added a warning when RA ant units or structures are used in the map, but no rule definitions for them exist in the ini.
* Added an option in the trigger filter dialog to filter on triggers. This will filter out the trigger itself, and any triggers destroying or forcing the selected trigger.
* When an RA trigger is set to type E1→A1, E2→A2, the controls for the events and actions will be reordered to accurately represent this.
* Added zoom options to the View menu.
* Added F-keys as shortcuts for the "Extra Indicator" options in the View menu.
* The user can now place map tiles partially outside the map at the top and left side.
* Team types now show full unit names.
* The argument dropdown for "Built It" triggers now shows the available theaters on theater-specific buildings in the list.
* Tile randomising now avoids identical adjacent tiles.
* Units, buildings and waypoints with a radius will now show that radius more clearly in placement preview mode.
* Red Alert data concerning rules.ini data, and map tileset data (dimensions, tile usage, land types) is now read from the original classic files.
* Changing rules will now only clear undo/redo history if bibs were actually changed.
* The tool windows will now remember what was last selected in them when they are reselected or reloaded.
* The selected House will now be retained across the infantry, units, and structures tools.

Map logic updates:

* All overlay placement is now correctly restricted to not be allowed on the top or bottom row of the map, showing red indicators when in placement mode.
* Resource placement with a brush size larger than 1 shows red cells inside the brush area when hovering over the top or bottom cells of the map. At size 1, the brush is simply completely red.
* Enabling/disabling expansion units in RA missions will now take into account any rule changes done by "aftrmath.ini".
* Changing the "solo mission" option for RA missions will now take into account any rule changes done by "mplayer.ini".

Program bug fixes:

* Fixed an error in the sorting of ini sections that messed up the linking of team types to triggers when the team type names ended on numbers going up to 10 and the lower numbers were not padded with zeroes.
* Applied DPI changes that might fix issues with objects drawing weirdly on some people's systems.
* Fixed an issue where RA triggers with waypoint "None" set in them would have that value corrupted to 255 after a reload of the map, causing other systems in the editor to crash.
* Fixed mixup between actions and events in the TD trigger reading checks.
* Fixed bug in the resource value calculation for gems.
* Rule errors that occur after closing the Map settings dialog will now show in a window with scrollable area, just like the errors shown when opening a maps.
* Added missing overlap checks to buildings on the rebuild-list, so they can no longer disappear on map load without any warning.
* Fixed bug in the team types window where the amount of added classes and orders would not unlock when maxed out on the previously selected item.
* Added missing names for items that were lacking them in the available game text resources.
* Harvestable resource will now show the actual name of the graphics they're showing, rather than always showing "TI01" / "GOLD01" / "GEM01".
* Changed TD Technology Center to its real name, instead of "Prison".
* Fixed swapped power usage and production of RA Gap Generator.
* Set correct preview House for the ant units and buildings. This only affects classic files mode though.
* Fixed bug where the Template tool and the "new from image" dialog used a tile size derived from the map scale factor instead of the preview scale factor to determine in which tile the user clicked.
* Fixed bugs in text indicator scaling.
* Added better fallbacks for missing graphics.
* Fixed an issue where map tiles could get the "contains random tiles" status assigned, but when loading a new theater this information wasn't cleared.
* Fixed multiplayer start position flags getting stretched to the full tile size instead of centered.
* The invite warning and game path asking dialogs now have the editor's icon rather than a default icon.
* Fixed crate outlines being linked to the visibility of waypoints instead of overlay.
* Fixed tab order in image export dialogs.
* The long shadows of terrain decorations like trees now correctly overlap anything standing under them.
* Fixed rounding issues in image export dialog, and added cell size info and a tool to set the scale by cell size.
* The placement grid is now also shown when using the flood-fill function.
* Fixed the cell selection indicator and the bottom info bar not immediately refreshing when using the arrow keys to scroll around the map.
* Fixed bug in the dragging logic that made the mouse position and map desynchronise or not work at all when dragging very slowly.
* Fixed bugs in the logic to zoom to the map bounds. It is now slightly more accurate.
* Fixed crashes that occurred when dragging bibs and buildings out of the right or bottom of the map bounds.
* Added map load checks on failing to detect the House of units, structures, triggers and teams. This includes a logic for RA to substitute the prerelease House Italy with its final version, Ukraine.
* Fixed bug where the radius painting of the placement preview for gap generators wouldn't work correctly because it could get mixed up with buildings set to be built later.
* The automatic tiling of clear terrain used a logic that was incorrect for the larger maps in Red Alert and Sole Survivor. This has now been fixed.
* Fixed case difference issues in triggers linked to objects and celltriggers.
* Fixed the fact disabling expansion units did not clear the undo/redo history if none were currently in use on the map or in team types.
* Adding and removing cell triggers now enables and disables the "jump to" button correctly, and updates the jump locations.

### v1.5.0.1:

Released on 12 Jul 2023 at 08:28 GMT

* Fixed the classic mode setting wrongly being stated as "ClassicGraphics" instead of "UseClassicFiles" on the game folder selection dialog.
* Fixed bugs that crashed the undo/redo function on the waypoints tool.
* Fixed tree names not getting initialised.
* Fixed Tiberian Dawn's Blossom Trees showing the "barnacled" state at the start of their mutation, rather than their fully mutated state.
* Fixed color corruption that occurred on some systems when showing semitransparent images in Classic mode. All internal image handling now uses 32 bpp images.
* Fixed a crash in the mod lookup logic on PCs where Steam isn't installed.
* Fixed bug where selecting an incorrect C&C Remastered folder would still close the dialog and save the incorrect path after showing the warning.
* Groups of trees are now indicated as "Trees" instead of "Tree".
* Opening a file or saving a new file will now default to the last folder from which a map was opened.
* Fixed bug where having custom ini content in managed sections (like Basic and the houses) would make the fully filled section show up in "INI Rules & Tweaks" after saving.
* Custom-added ini keys on managed sections are now added to the end of the section rather than at the start.
* Fixed crashes when selecting WallGroups in Interior theater when map passability indication is enabled.
* Fixed issues related to previewing the placement of WallGroup tiles on the map.
* Disabled DPI awareness logic, because apparently it only had negative side effects.

### v1.5.0.2:

Released on 05 Aug 2023 at 13:50 GMT

* The Cell Trigger tool's "jump to" function will now also jump to placed objects with the attached trigger, to easily review what uses it.
* The "jump to" function in Waypoints and Cell Triggers mode will now properly update the mouse cell info in the bottom bar.
* The \[Briefing\] and \[Base\] sections can now also accept custom ini keys (which can be useful for mods).
* Undo/redo operations are now blocked while mouse drag operations (moving or drag-placing objects) are in progress, to prevent corrupting the list of undo/redo operations.
* Fixed bug where having a specific tile from a random tiles group selected would still show the first tile of the group as placement preview.
* Added the ability to export map previews without the map templates layer. This will give an image with a transparent background.
* The tooltips showing which trigger criteria are needed for using a trigger in a certain situation will indicate which of the criteria are fulfilled by the currently selected trigger.
* The tooltips showing trigger criteria will now say "usable" events/actions rather than "required".
* The Zoom shortcuts now work on more keys than just the numpad ones.
* Fixed issues with how objects overlapped with each other. This doesn't 100% match the game, but the game's method for doing overlaps is often very strange and unrealistic.
* Refined the "outlines on overlapped objects" logic to only outline objects that are actually high enough up compared to the other object to be considered overlapped.
* Fixed a bug where Terrain objects would not show a placement preview over bibs.
* Fixed a bug in Walls mode where changing to a different type did not refresh the preview.
* Fixed a bug where switching back to the Terrain tool would show the previously-selected type in the preview, but would not select it in the list.
* Fixed a bug in Map mode where using the \[PageUp\] or \[Home\] key to select the Clear terrain would not scroll to the item in the list.
* Added setting to enable DPI awareness mode, for users with indicator scaling issues.
* Triggers on TD Terrain entries are now optional in the ini; terrain entries without the trigger part will no longer be skipped as malformed data.
* When loading a map and objects are detected as overlapping a multi-cell object like a building or tree, on top of giving the cell where the overlap occurred, the system will now also report the placement cell of that object.
* Overhauled the undo/redo system of building priorities, to remove inconsistencies.
* Modifying the rebuild priority of a placed building will now adjust the rebuild priorities of the other structures, as already happened when adding and removing them.
* Fixed bug where two buildings of different types placed on the same cell (e.g. a guard tower on the top-left corner of a repair bay) would not correctly be distinguished as different entries by the rebuild priority system, causing one of them to lose its rebuild status.
* The "Learning" and "Mercenary" options in the TD team types editor are now crossed out in the UI, and indicated on the tooltip as having no effect.
* Megamaps for TD can now be published on the Steam workshop, provided they are singleplayer scenarios. They will give a warning about the nonstandard format, though this is not re-shown for the same map on subsequent uploads.
* Added forgotten \[PageUp\]/\[PageDown\]/\[Home\]/\[End\] shortcuts support to the Walls tool.
* Buildings now all have a set height to control overlaps.
* Civilian building V37, The Studio, is now seen as a flat building, lower than even the repair bay, so its top left corner can't overlap other objects. (Sadly, the game does not do this.)
* Fixed column resizing in the triggers list so it always maximizes the clickable area to select a trigger, and never gives scrollbars unless needed.
* The mplayer.ini file tweaks are now only applied if RA's Aftermath expansion units are enabled, as it should.
* The editor will now be allowed to load in classic mode without the RA expansion files present. Any missing graphics will be substituted by dummy graphics.
* Overhauled the "New Map" dialog using list boxes, to have more consistent tab order control, and to allow more easily modding in additional games / theaters.
* Added "Single-Player Scenario" checkbox on the "New Map" dialog.
* Added remap logic to fix Einstein's colors in classic mode so he doesn't just look like Dr. Mobius. This uses the remap system used by the other RA civilians.
* The power evaluation tool now takes buildings' damaged status into account when calculating the produced power.
* Multiple overlapping multiplayer flags will now be drawn as multiple flags on one cell, scaling down later overlapping ones.
* Multiple overlapping non-flag waypoints will no longer show up brighter on the map.
* Added option to let the editor remember tool selection information between different opened maps of the same type. This option is disabled by default.
* Left-clicking on smudge no longer copies its properties into the tool, since left-click is needed to change smudge properties.
* Removed "NoMetaFilesForSinglePlay" option, since the meta files are always 100% useless in single play, and instead added "ClassicProducesNoMetaFiles" to suppress the creation of such files for all maps in classic mode.
* In old Red Alert maps which use Italy instead of Ukraine, The house settings for \[Italy\] will now be applied to Ukraine.

### v1.5.0.3:

Released on 31 Jan 2023 at 20:50 GMT

* Added support for fan-added theaters available in CnCNet: Snow for TD; Desert, Jungle, Barren and Cave for RA. These only work in classic mode, and if their theater mix files are found in the configured classic files folder.
* Mouse zoom is now disabled during drag-scroll operations, since it invariably messed up the position calculations.
* Map templates, Smudge and Overlay are no longer restricted to specific theaters; if the files exist in a theater, they are allowed.
* Theater-sensitive civilian buildings are no longer restricted to specific theaters; if they exist in a theater, they are allowed. Military buildings, including the theater-sensitive Missile Silo and Pillbox in RA, are always usable, no matter whether the theater has graphics for them.
* On Interior maps, the "ConvertRaObsoleteClear" logic will now generate spots of passable terrain outside the map border on any point where passable terrain touches the border, to allow potential reinforcements to enter the map there.
* When editing a TD map, the team types window will no longer show the trigger info label, since TD team types can't have a linked trigger.
* Added fix for semicolons cutting off briefings in TD, and a warning in RA.
* The Base and Briefing sections can no longer remain behind as empty sections in the extra ini rules.
* The Digest section will now also be removed from TD / SS maps.
* Upgraded the logic to detect missing rules for ant-related objects on RA maps, so it includes the Mandible weapon, and checks in triggers too. The check now also goes through the rules files, so when using a rules mod that does define these things, no warnings will be shown.
* Added config option for the behaviour to recolor the classic DOS Einstein to RA95/remastered colors.
* Fixed the alliances list in the house settings scrolling past the first selected alliance.
* Red Alert now allows the special grouping Houses "Allies" and "Soviet" in their alliances list.
* Houses now always automatically add their own house name in their alliances list if it is missing.
* Sole Survivor maps with all multi-Houses enabled will now have all those beyond Multi4 disabled on map load, to avoid needlessly expanding the ini.
* Added "Info" menu, containing general program info, a link to the Github website, and an update check function.
* Added update check on startup. This option can be turned off using the "CheckUpdatesOnStartup" option in the config file.
* Fixed the CONC pavement connecting in TD to completely match the way the game does it. There is also an alternate mode available, through the parameter "FixConcretePavement", which shows the pavement as it was intended to be if the game's connection logic actually functioned correctly.
* Renamed RA trigger "Destroyed, Fakes, All..." to "All Fakes Destroyed".
* Prefixed all RA "Text Trigger" options with the expansion they belong to.
* Fixed RA's "Auto Base Building..." trigger; its on and off values were switched.
* All argument lists for trigger Events and Actions that contain a "None" item will now select that by default.
* The tool window is now always hidden while a dialog is opened.
* Fixed ampersands not showing in the "Recent Files" menu.
* Added indicator of line wrap mode in text dialog.
* Added \[Ctrl\]+scrollwheel for going through item lists.
* Added an "EditorLanguage" option that can be used to change the language of the game text loaded in the editor. The default value is "Auto", which makes it autodetect the language using the system language.
* Added an option to control the behaviour of allowing walls as structures, with an owner. Since this is a behavior tweaks option, and those are all enabled by default, it is called "DontAllowWallsAsBuildings". \[NOTE: Later changed to "OverlayWallsOnly"\]

### v1.6.0.0:

Released on 22 Sep 2024 at 15:00 GMT

* Added .mix files to the supported formats to open, giving the ability to load official maps straight from the game's internal archives.
* Implemented a new scaling method that vastly reduces the saving time of multiplayer map thumbnails and image exports.
* Compacted the space around the objects in the Map tool, making it show a lot more items at once.
* The editor will now only connect to Steam if the Publish function is opened, making it possible to leave the editor open while testing maps in-game.
* Buildings and solid overlay types (walls, fields) are now their own separate placement layer, which does not interfere with units, infantry and Terrain, which allows overlapping these things without issues.
* Classic mode now uses classic pixel fonts, allowing for much nicer image exports of maps at their original size in classic mode.
* Dropping a file into the editor that causes a dialog to be opened (like mix files and images) will no longer freeze the source you dragged the file from while the dialog in the editor is open.
* Removed theater restrictions on Terrain objects; if the object is found in the theater, it will be shown.
* Fixed bug that prevented the Red Alert civilian "C3" from loading, and that prevented the Einstein color fix from working.
* The "New from image" function now gives a warning if the chosen image is not the expected size for the selected map type.
* In the trigger editor, when hovering over the trigger action dropdown when it has one of the Tiberian Dawn "Dstry Trig '????'" actions selected, a tooltip will show with the contents of the linked trigger to destroy.
* In the trigger editor, when hovering over the trigger action dropdown when it has the Tiberian Dawn "DZ at 'Z'" action selected, a tooltip will show the currently set location of the Flare waypoint.
* In the trigger editor, when hovering over a dropdown containing the list of team types, a tooltip will show a brief overview of the selected team type.
* Added a tooltip to all places that show a triggers dropdown, to show the contents of the currently selected trigger.
* Added mission load checks and warnings on team type arguments, and on the limits of units/orders list lengths.
* Added system to avoid loading the same mod from both the workshop items and from the local mods under the Documents folder. If given by name, local mods are given priority.
* The \[Enter\] shortcut for the "Jump To" function is now indicated in the status bar in Waypoints and Celltriggers mode.
* Added a specific "PreviewScaleClassic" setting, rather than automatically calculating it from the one meant for the Remaster graphics.
* Fixed Red Alert map load not giving feedback on the removal of Aircraft.
* Added a warning indicator on the Autocreate option in the team types editor if the "Max Allowed" setting is set to 0.
* Enabling map terrain type indicators will now show a different color for each terrain type, editable per type in the settings file.
* The placement previews for map templates will now always show the terrain types. This indication will be less pronounced when the map terrain type indicators option is disabled.
* Added extra indicators (shortcut: F4) for cells occupied by objects on the map.
* Added outlines for overlapped buildings and Terrain objects. They will only be considered overlapped if all of their cells that contain graphics are considered overlapped, since that would imply the object is obscured from view.
* Added outlines for overlapped overlay and walls. This only affects solid overlay that obstructs the map, not pavement types. Note that for walls, this is disabled by default (see "OutlineColorWall" in [the manual](MANUAL.md#colors-and-transparency)).
* The overlap algorithm that determines if an outline is needed now works per infantry sub-cell position rather than per whole cell, giving much more accurate results.
* The overlap algorithm that determines if an outline is needed now keeps the draw-order of objects in mind, so an object that is drawn over another object will never be considered to be overlapped by that second one.
* Fixed issue that caused any unknown video names configured on TD missions to be marked as "Remaster only".
* Fixed a bug where the values of team type orders, trigger events and trigger actions were saved as value "-1" when a new item's value was not changed from its default.
* Fixed RA sound effect string "water impace".
* Fixed bug in RA trigger reading where it would complain about House "None", fix it back to house None, and erroneously show the trigger's Persistence value instead of the House value in the error report.
* Added indication in House settings that the configured house credits value is multiplied by 100 in-game.
* Removed incorrect indication on RA's "Credits Exceed" trigger event that claimed the value was multiplied by 100.
* Fixed RA "Build Aircraft Type" trigger event to correctly save the unit ID.
* Added checks on Events in Red Alert triggers that crash the game if no House is set on the trigger itself.
* Fixed the fact RA mission rules were only applied after the map was populated, which could make buildings disappear from overlapping with bibs that were actually disabled in the rules.
* Fixed classic mode using the high resolution hash pattern to draw the terrain type indicators on the map tiles tool window, which looked out of place on classic graphics.
* Fixed a crash that happened on Tiberian Dawn maps when switching to Infantry or Unit editing mode after having been in the Building tool, if the template building there had the Prebuilt option unchecked, leaving the House set to "none".
* Fixed overlapping multiplayer starting point flags not working in combination with Sole Survivor football areas.
* Rebalanced the tint used for the House-colored range and outline indicators to be more similar in the remastered and classic view.
* Fixed the "FAKE" label on fake buildings not being shown as more transparent on placement previews.
* Optimised all indicator drawing by caching the generated images and overlap outlines.
* Fixed an issue in the redo of drag-moving infantry, that created a ghost duplicate of the dragged infantry unit that could cause crashes.
* Fixed an issue with the main editor window becoming hard to select if it was minimised during a load or save operation, because it would select the tool window instead of restoring the main window. To fix this, the tool window is now never shown if the main window is minimised.
* Added an indicator to the building tool to indicate whether a building is capturable.
* Added support for C&C95 v1.06c's bib disabling and capturability rule tweaks.
* Fixed trigger changes on Terrain objects not immediately refreshing the map.
* Fixed an internal issue with the unit/infantry/building properties popup not cleaning up its internal resources after closing.
* Waypoints now use the "Select" cursor graphics as indicator on the map, rather than the green beacon that was barely visible behind the text.
* Map resources randomisation now uses a fixed seed, retaining the variety while preventing the resources from becoming completely different on each resave.

### v1.6.1.0:

Unreleased

* Changed the executable name to "MobiusMapEditor.exe".
* Overhauled Steam Publish UI; it always shows the preview image, and has a list of optional tags (for multiplayer).
* When exporting a map to image, selecting the "Only export map area inside bounds" option will now deselect the "Map Boundaries" indicator. When exporting multiplayer maps, if the "DefaultExportMultiInBounds" is enabled, map boundaries will automatically be deselected.
* For singleplayer maps, the Steam Publish UI now generates a thumbnail showing the mission's start situation around the Home waypoint, focused specifically on the player's units inside it.
* The Steam workshop preview generated for a multiplayer map will now show enlarged start location flags with very visible circle indications underneath them. The option to add these is also added to the image export window.
* Added the ability to choose which games to use the editor for, with the "EnabledGames" setting. This allows e.g. disabling Sole Survivor to reduce UI clutter.
* Overhauled the map file loading system to massively simplify it internally. This fixes the bug that prevented Tiberian Dawn and Sole Survivor maps from opening from the .bin file.
* Added a new overlap detection logic for objects of the Terrain, Building and Overlay types that only gives them an outline if at least 50% of their non-shadow graphics (as determined by infantry sub-positions) are considered overlapped.
* Fixed bug that prevented overlap outlines from working on terrain objects that had any cells that only contained shadow.
* Fixed bug that occasionally showed building priority as "0" on preview buildings if they showed an overlap outline.
* Added option "IgnoreShadowOverlap" to not draw overlap outlines on objects that are only overlapped by shadows.
* Added option "EnforceTriggerTypes" that can be disabled to allow any triggers on any object types. This is mainly for research purposes.
* Fixed bug that when the option to show crates on top is enabled, they were still outlined by the "outlines on overlapped objects" function, despite never being overlapped.
* Removed randomness from RA map resources on save, to optimise compression of the OverlayPack ini section. Both the editor and the game re-apply it on load anyway.
* Fixed bug where opening and confirming the map setting without making any changes would clear the map's "modified" status.
* Fixed crash when trying to publish a freshly opened new map with no content added at all.
* Fixed crash when trying to publish a map opened from mix file. It will now always require being saved to disc first.
* The editor title now explicitly shows when the editor is connected to Steam.
* The editor will now only connect to Steam after performing all the required map validations.
* Added the ability to open and save the Nintendo 64 map type for Tiberian Dawn. This type does not support megamaps, and has some limitations in its tileset and units.
* When removing the obsolete old "clear" terrain in RA interior maps, the generated open areas for allowing reinforcements now use the default dark floor type, rather than the first-found passable 1x1 tile in the tileset list.
* When FilterTheaterObjects is disabled, the icons in the top bar will now still prefer an existing object to show as example.
* Fixed bug that made the fallback dummy texture not show correctly after loading a second map where it is used.
* Fixed bug where the hashing of occupied buildings didn't work unless either map land types hashing was also enabled, or there were units on the map to also show the hashing for.
* Expanded the validation system to allow specific validations for the type that was chosen to save to, such as the Nintendo 64 version.
* Fixed bug where pressing \[Ctrl\]+\[Shift\]+\[S\] to access the "Save As" function would create a ghost image on the map of the currently selected object to place.
* Added logic to snap unsupported facing direction values in the ini to their nearest match rather, than resetting them to North.
* Fixed feedback on facing direction parsing not giving the object's cell number.
* Fixed missing first letter of entries in the "Open from Mix" list when the "ClassicPath" setting for that game ends on a backslash.
* Fixed an issue in the loading of resources from embedded .mix archives where it would take the last found occurrence instead of the first one. This caused it not to load the Aftermath version of the Convoy Truck, which has proper shadows.
* Fixed issues with waypoint flags getting redrawn multiple times, resulting in a completely black shadow. The flags are now also semitransparent, like all other waypoint graphics.
* Sole Survivor football area indications are now no longer handled as indicators, so they can be properly painted below the waypoint flags.
* Changed the Missile Silo preview to be shown as USSR building.
* Fixed issue with undo system restoring old saved "modified" statuses instead of marking the map as unmodified when restoring the point when the last save happened.
* Fixed issue with redo system always marking the map as "modified" even when restoring to the point where the last save happened.
* Added logic to crop unit graphics to a maximum size of 3x3 cells, since that is the maximum refreshed area for a unit. This mitigates issues in the game files such as the classic German RA's uncropped helicarrier.
* When the editor only has a single game enabled in "EnabledGames", the "Open from Mix" menu will leave out the folder with the game name.
* Added compatibility system for loading different language and version variations of the classic Tiberian Dawn strings file.
* Red Alert's "FAKE" labels in classic graphics mode are now loaded from the game graphics rather than generated from text, meaning they adapt to the game language.
* Tiberian Dawn trigger events will now give a detailed tooltip when hovering over the Event dropdown.
* When selecting the Home waypoint, single player missions will now show a "Start view" box indicating the initial area the player will see when starting the mission. This will show both the DOS and Win95 viewport, which roughly correspond to respectively being fully zoomed in and fully zoomed out in the Remaster. Like the area reveal indicators on waypoints, this can be shown permanently through an option in the Extra Indicators, with this new one linked to the F8 key.
* A warning is now shown when trying to publish a map but not setting it "Public", since the C&C Remaster relies on an external maps server that can only find the items if they're fully public.
* Fixed inconsistencies in zoom level when zooming in and out. The calculation for the zoom factor when zooming out is now the exact inversion of the one done when zooming in.
* Added more tooltips to the Teamtypes options.
* Singleplayer map detection will now only happen if the "SoloMission" key is not present inside the ini file.
* Smudge objects can now be painted by dragging the mouse around. Like for the map tiles, this logic prevents overlapping placement when drag-placing multi-cell objects such as the bibs.
* Smudge objects can now be bulk-removed by moving around the cursor while the right mouse button is pressed down.
* Right-clicking on placed down buildings that have overlay equivalents, or vice versa (such as the fields and haystacks) will now select the corresponding item even if the item you clicked is the other type version.
* Added support for the expanded House colours in C&C1 accesible by putting ColorScheme / SecondaryScheme / RadarScheme in the ini under the house's settings section. This only works in Classic graphics.
* If the editor loses focus while dragging an object, this will no longer prevent the undo action for that drag operation from being stored. It will instead immediately end the drag operation at the current mouse position.
* Removed the ability to place walls as structures (by disabling "OverlayWallsOnly") in Red Alert maps; it simply crashes the game.
* Fixed an issue where certain options or rules changes in the Map dialog would not refresh on the map afterwards.
* Improved teamtype tooltips in the Triggers window to show the full list of orders they execute.
