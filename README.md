## C&C Tiberian Dawn and Red Alert Map Editor

An enhanced version of the C&C Tiberian Dawn and Red Alert Map Editor based on the source code released by Electronic Arts. The goal of the project is simply to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base, enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though, but post them as issues.

---

## Usage

The creators of the map editor have chosen to build a manual into the editor, but it might not be immediately obvious. Look at the bottom bar, and it will tell you for the currently selected editing type what your mouse buttons will do, and which modifier keys will change to different placement modes. Once you hold down such a key, the bottom bar text will change, further explaining what your mouse buttons will do in this specific mode.

Besides that, the scroll wheel will allow zooming in and out, and the middle mouse button will allow you to quickly drag-scroll around the map.

Specific options about the map and the scripting elements can be found in the "Settings" menu:

* "Map" will allow you to set the map's name, indicate whether it is a single player mission, set videos for mission startup, win, and lose, and configure the player's House (for singleplayer) and set alliances and other settings for all the Houses used on the map.
* "Teamtypes" will allow you to define teams that can be used in the map's scripting, or that will simply be created randomly as attack teams by the AI Houses.
* "Triggers" will allow you to make scripts that execute on the map. Note that scripting is mostly a singleplayer thing, and is severely limited in multiplayer, especially in Tiberian Dawn.

For Tiberian Dawn maps, the triggers dialog has a "Check" button that will check if any configurations in the triggers might not work, or might even cause game crashes. For more info on that, see [the TD triggers overview guide I wrote on Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=2824756756). Note that this is not a scripting guide; it is an overview of what each trigger event and action will accept as inputs, and produce as output, highlighting potential issues and some workarounds. Unfortunately, I'm not very well-versed in Red Alert triggers, so there is no equivalent check function (or guide) for Red Alert.

---

## Configuration

The file "CnCTDRAMapEditor.exe.config" contains settings to customise the editor. This is what they do:

* **ModsToLoad**: semicolon (or comma) separated list of mod entries. A mod entry can either be a Steam workshop ID, or a path of the type "Tiberian_Dawn\ModName" or "Red_Alert\ModName". The paths will initially be looked up under My Documents, but the loading system will also check the Steam workshop files, and use the game prefix part to verify the mod's targeted game. Note that due to the order in which files are loaded into the editor, mods are **not** loaded conditionally per map type based on this targeted game. The editor also has no way to check which mods are actually enabled in the game, and will load anything that is configured and of which the files can be found.
* **NoMetaFilesForSinglePlay**: Suppresses the generation of .tga and .json files for single player maps saved to disc. This does not affect Steam uploads.
* **IgnoreBibs**: Ignore bibs in the placement of buildings. Note that if you're not careful with this, this might prevent proper rebuilding of AI bases.
* **MapScale**: Scaling multiplier for the size at which assets are rendered on the map; higher means lower quality. This will make the UI more responsive. Negative values will enable smooth scaling, which gives nicer graphics but will make the UI noticeable _less_ responsive. Defaults to 0.5.
* **PreviewScale**: Scaling multiplier for the size at which assets are rendered on the preview tools. Negative values will enable smooth scaling, but this usually doesn't look good on the upscaled preview graphics. Defaults to 1
* **ExportScale**: Scaling multiplier for the size at which an exported image will be generated through "Tools" → "Export As Image". Negative values will enable smooth scaling. Defaults to -0.5.
* **ObjectToolItemSizeMultiplier**: Floating-point multiplication factor for downsizing the item icons on the selection lists on the tool windows.
* **TemplateToolTextureSizeMultiplier**: Floating-point multiplication factor for the size of tiles shown on the Map tool. This scaling is somehow done relative to the screen size; not sure.
* **MaxMapTileTextureSize**: Maximum for the size of the tiles shown on the Map tool. Leave on 0 to disable.
* **IgnoreRaObsoleteClear**: Prevents the clearing of tiles with ID 255 on RA maps. This is purely a research option for analysing modern changes on old maps.

The **ModsToLoad** setting will have the `Tiberian_Dawn\ConcretePavementTD` mod set by default, to complete the incomplete TD Remastered graphics set, meaning it will automatically be loaded if found.

You can find the mod [on the Steam workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2844969675) and [on ModDB](https://www.moddb.com/games/command-conquer-remastered/addons/concretepavementtd).

---

## Change log

### Features added by Rampastring:

* Downsized tool graphics by a user-configurable factor so you can see more placeable object types at once on sub-4K monitors.
* Improved zoom levels.
* Fixed a couple of crashes.
* Made tool windows remember their previous position upon closing and re-opening them.
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks.

### Features and fixes by Nyerguds:

v1.4.0.0:

* Fixed overlay height overflow bug in Rampa's new UI.
* Fixed map tiles list duplicating every time the "Map" tool window is opened in Rampa's version.
* Split off internal overlay type "decoration", used for pavements and civilian buildings.
* Added CONC and ROAD pavement. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in teamtypes.
* Removed the Aircraft from the placeable units in TD.
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc.)
* Fixed case sensitivity related crashes in TD teamtypes.
* TD triggers without a teamtype will now automatically get "None" filled in as teamtype, fixing the malfunctioning of their repeat status.
* Added Ctrl+N, Ctrl+O, Ctrl+S etc. shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tool window to adjust crater stage.
* Fixed TD terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
* RA "Spied by..." trigger event now shows the House to select.
* Added "Add" buttons in triggers and teamtypes dialogs.
* Fixed tab order in triggers and teamtypes dialogs.
* Fixed crash in "already exists" messages for triggers and teamtypes.
* Randomised tiberium on save, like the original WW editor does. (this is purely cosmetic; the game re-randomises it on map load.)
* [EXPERIMENTAL] Added ability to place bibs as smudge type. They won't show their full size in the editor at the moment, though.

v1.4.0.1:

* Added "All supported types (\*.ini;\*.bin;\*.mpr)" as default filter when opening files.
* Added Drag & Drop support for opening map files.
* Added command line file argument support, which allows setting the editor as application for opening ini/mpr files.
* House Edge reading now corrects values with case differences so they show up in the dropdown.
* Centralised the House Edge array on the House class, and changed its order to a more logical North, East, South, West.
* Fixed order of the Multi-House colours. It seems the error is not in the editor, but in bizarre mixed-up team colour names in the remastered game itself.
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

v1.4.0.2:

* Fixed the bug that cleared all map templates on save in v1.4.0.1 (whoops).
* Fixed the bug in the teamtypes list that showed the wrong context menu options on right click.
* Fixed the bug that the status bar did not show the map placement shortcuts hints on initial load.
* The editor no longer exits if it cannot connect to Steam. Instead, workshop publishing will simply be disabled if the Steamworks interface can't be initialised.
* The texture manager will now properly dispose all loaded image objects when a different map is loaded.
* Added \*.ini to the list of possible extensions for saving RA maps, to support opening pre-Remaster missions.
* If a building has no direction to set and shows no dropdown for it, the "Direction" label is now also removed.
* Structure graphics are now correctly centered on their full building size.
* The damaged state of buildings is now shown at strength values of 128 and below, rather than only below that value.
* Damaged states now work correctly on all buildings, with a vastly simpler and more general internal logic.
* Using the mouse wheel will now change the strength of objects in increments of 4.
* IQ of all Houses in RA now defaults to 0.
* Fixed gunboat facing and damage states logic.
* Fixed bug causing bad refresh when previewing the placement of a single cell selected from a template with an empty top right corner cell.
* The "clear1" tile is now explicitly shown in the tiles list.
* Teamtype "Priority" value (recruit priority) is now capped at 15.

v1.4.0.3:

* The editor now tries to automatically detect the game installation folder in Steam.
* Fixed refresh errors in preview images when resizing tool windows.
* All overlay items will now show a preview icon of the same size.
* Fixed errors in tree sizes.
* The 'clamping' logic that prevented tool windows from being dragged outside usable screen bounds had a bug that this prevented it from being dragged onto a different monitor. This is now fixed.
* Added "Theme" to the map settings.
* Removed "Percent" from the map settings. It is an unused Dune II leftover.
* Added "Classic only" labels to "Carryover Money" and "Theme" to indicate these options will only work when playing the missions in the original game.
* All videos available in the Remaster are now shown in the video lists in the "Map settings" dialog.
* Added missing entries (videos not included in the Remaster) to the RA and TD video lists, with a 'Classic only' indicator.
* In the teamtypes dialog, the rather confusing use of the internal name "Missions" was changed to a more intuitive "Orders".
* Added tooltips for all teamtype options.
* Teamtype orders now show a tooltip on the Argument field indicating the meaning of the value to give, and, if needed, the possible values to choose from.
* Fixed tab order of the teamtype options.
* The dropdowns in the grids in the teamtypes dialog now respond without having to click multiple times.
* Removed the previously-added cap on the teamtype "Priority" value after feedback from users and checking the source code.
* The CONC and ROAD overlay types now show the same graphics as in-game. This is technically just a dummy graphic the game uses when not finding object graphics. The version in the editor is a reconstruction.
* Removed limitation on placing resources on the top and bottom row of the map.

v1.4.1.0:

* Fixed dimensions of RA's ore mine, Snow theater ice floes and Interior theater boxes, and one of the Desert theater rocks in TD.
* Added \*.ini to the list of possible extensions for opening RA maps. Apparently before I only added it for saving.
* The editor will now accept nonstandard extensions from drag & drop without any issues. For TD maps, it will need to find the accompanying bin or ini file with the correct extension.
* Files opened from filenames with nonstandard extensions will not change these extensions when saving the file. This also means RA maps opened from a .ini file will no longer change the extension to .mpr when saving.
* Terrain objects will now only pop up a poperties box for setting a trigger on TD maps.
* Optimised loading so the editor will skip loading objects from different theaters.
* User settings (game folder, invite warning, and the dialog locations) will now be properly ported over from previous versions.
* Added support for loading mod xml info and graphics through the "ModsToLoad" setting in "CnCTDRAMapEditor.exe.config". The syntax is a semicolon-separated list, with each entry either a Steam workshop ID, or a folder under "Documents\CnCRemastered\Mods\". As folder, the path must contain the "Tiberian_Dawn" or "Red_Alert" part at the start. That prefix folder will also be used as consistency check for the mod type as defined inside "ccmod.json". Mods given by folder name will also be looked up in the Steam workshop folders, with the prefix folder used only for the consistency check. Mods do NOT have to be enabled in the game to work in the editor.
* Added support for the unique pattern of TD's "conc" pavement. You will need the "ConcretePavementTD" mod to actually see that, though. This mod is filled in by default in the editor's mod loading settings, meaning it will automatically be used if found.
* Fixed loading and saving of the videos set in the map options dialog, so no more errors pop up there.
* Made video names freely editable for TD missions. Any mod-added video in TD is playable from missions. Be warned that when a video is not found, this may cause the game to hang for several minutes.
* The preview selection in the Steam publish dialog will now open in the correct folder.
* The new setting "NoMetaFilesForSinglePlay" in "CnCTDRAMapEditor.exe.config" will suppress the generation of .json and .TGA file when saving single player missions to disc. Not writing them is now the default behaviour. This does not affect the Steam workshop upload behaviour.
* The rendered previews will now show all map contents, to give a better representation of what is on the map. Note that for single play missions, this preview is generated in the folder but is optional.
* Removed crater types CR2 to CR6; they don't work correctly in either game and will just show the smallest size of CR1. Any craters of other types encountered on map load will now be converted to CR1.
* The teamtypes dialog no longer uses data grids for its teams and orders.
* Teamtypes now show full names for unit types.
* The input for arguments for orders in the teamtypes dialog now correctly adapts to the type of each order, giving dropdowns for special choices lists and for waypoints.
* The waypoints that can be selected for an RA teamtype now correctly start from -1 as "(none)".
* Fixed colour of "Special" in RA to have the same colour as Spain.
* Fixed the fact trigger Events and Actions retained their argument data when changing their type, meaning the UI would pick the equivalent data on whatever list or control popped up for the new type.
* RA triggers now show human-readable data for the Event and Action arguments.
* The editor no longer locks up when the triggers dialog shows an empty list of teamtypes or (previously-saved) triggers because none were made yet.
* Removed Aircraft section handling. Aircraft were never able to be pre-placed in the original game, and the re-enabled sections in the Remasters have issues; aircraft will still spawn in the air and fly somewhere close.
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
* The "MapScaleFactor" and "PreviewScaleFactor" settings in the "CnCTDRAMapEditor.exe.config" file can adjust the downscaling factor for respectively the map graphics and the preview graphics. Higher values will reduce quality, but will make the editor more responsive. By default, previews in tool windows will now use higher quality graphics than the map. Setting a negative value will enable smooth scaling. (Not advised, but it's available)
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
* Red Alert interior theater no longer crashes when trying to show the bibs in the Smudge tool window.
* Structures can no longer be put in an illegal state where "Prebuilt" is disabled but the rebuild priority is set to -1.
* Fixed a crash in the RA triggers caused by the removal of the Aircraft types from the placeable objects.
* Fixed refresh issues that occurred when moving the mouse out of the map area while still in placement mode.
* Fixed incorrect tooltip placement when using bounds dragging mode on a different monitor.
* Red Alert's Interior tileset now supports randomising the 1x1 tiles that contain alternate versions. This type will now show all alternates on a blue grid in the preview window. Specific tiles can still be picked the usual way if you do not want random ones.
* If mods add extra tiles to existing 1x1 tilesets, these will be treated as 1x1 with alternates too.
* Tanya's default colouring in the editor preview is now Allied, and the M.A.D. Tank's colour is now Soviet.
* Changed Red Alert's trigger action "Destroy attached building" to a more accurate "Destroy attached object", seeing as it even works from celltrigger to kill units.
* Bibs are now shown as part of the building boundaries.
* Bibs boundary checking can be disabled with a global setting.
* The overlap detection in the map loading now correctly scans the full footprint of buildings and terrain objects, and will now correctly report the blocking object.
* Sounds lists in RA triggers now have descriptions.
* If you try to save an opened file but the folder it was loaded from is deleted, it will no longer give an error, but revert to "Save As" behaviour.
* The chosen preview image in the Steam upload dialog will now also be used as in-game preview for the map.
* In TD maps, a building that is set to be rebuilt but is not built from the start will now show as House "None".
* The Interior tiles "wall0002" to "wall0022" are now grouped per type into three dummy-entries "wallgroup1", "wallgroup2" and "wallgroup3", to simplify random placement of these tiles and to remove clutter from the tiles list.

v1.4.1.1:

* The Red Alert teamtype order "Guard Area" now correctly has 'time' as argument type, rather than a waypoint.
* Added a system to detect singleplayer missions from the original games and automatically mark them as singleplayer if they conform to the classic naming scheme for singleplayer missions, and contain a Lose and Win trigger.
* Functions asking to save unsaved changes (New/Open/Publish/Close) will now actually abort if you choose to save the unsaved opened map but it doesn't pass the basic waypoints validation, rather than giving the validation fail message and then continuing anyway.
* The title of the window will now show "Untitled.ini" or "Untitled.mpr" when you have a new but unsaved map opened.
* The title of the window will now show an asterisk behind the filename to indicate that the current file has unsaved changes.
* Maps loaded from file are now seen as 'modified' if any issues were detected that made the editor change or discard data during the loading process.
* The triggers check feedback (TD only) now also uses the large window used for showing the map load errors.

v1.4.2.0:

* Menu items related to map actions will now be disabled if no map is loaded.
* Fixed "Destroy attached object" trigger in RA not being seen as valid on units.
* Added power balance evaluation tool.
* If a map is saved without a map name set in the map settings, the filename will be filled in as name.
* Expanded the "View" menu to enable and disable drawing for all possible map elements and indicators on the map.
* Vehicle previews are now shown in a more dynamic south-west facing.
* When a map is opened, the editor will load theater-specific icons into the toolstrip.
* Resource placement is now disabled in Interior theater.
* Map loading now checks if map objects exist in the specified theater.
* An image export function has been added. This will mirror the current items enabled in the "View" menu. Its size is determined by the "ExportScale" setting in "CnCTDRAMapEditor.exe.config".
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
* The possible multiplayer waypoints to place on a map now go from 0 to 15.
* If the mission is marked as single player scenario, the first waypoints are no longer indicated as player start positions with a "P" prefix.
* Mods will now only be loaded for maps targeted at their respective game, meaning common assets can be overridden differently by TD and RA mods.

### Possible future features

Some ideas that might get implemented in the future:

* Add Tiberian Dawn Megamap support. (Sole Survivor map type)
* Show bibs placed as the 'smudge' type as their full size.
