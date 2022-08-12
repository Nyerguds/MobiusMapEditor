## C&C Tiberian Dawn and Red Alert Map Editor

An enhanced version of the C&C Tiberian Dawn and Red Alert Map Editor based on the source code released by Electronic Arts.
The goal of the project is simply to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base,
enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though, but post them as issues.

---

## Change log

### Features added by Rampastring:

* Downsized menu graphics by a user-configurable factor so you can see more placeable object types at once on sub-4K monitors.
* Improved zoom levels.
* Fixed a couple of crashes.
* Made tool windows remember their previous position, size and other settings upon closing and re-opening them.
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks.

### Features and fixes by Nyerguds (so far):

v1.4.0.0:

* Fixed overlay height overflow bug in Rampa's new UI.
* Fixed tiles list duplicating every time the "Map" tool window is opened in Rampa's version.
* Split off internal overlay type "decoration", used for pavements and civilian buildings.
* Added CONC and ROAD pavement. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in teamtypes.
* Removed the Aircraft from the placeable units in TD.
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc.)
* Fixed case sensitivity related crashes in TD teamtypes.
* TD triggers without a teamtype will now automatically get "None" filled in as teamtype, fixing the malfunctioning of their repeat status.
* Added Ctrl-N, Ctrl+O, Ctrl+S etc shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tool window to adjust crater stage.
* Fixed terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
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

v1.4.0.4: [WIP]

* Fixed dimensions of RA's ore mine, Snow theater ice floes and Interior theater boxes.
* Added \*.ini to the list of possible extensions for opening RA maps. Apparently before I only added it for saving.
* The editor will now accept nonstandard extensions from drag & drop without any issues. For TD maps, it will need to find the accompanying bin or ini file with the correct extension.
* Files opened from filenames with nonstandard extensions will not change these extensions when saving the file. This also means RA maps opened from a .ini file will no longer change the extension to .mpr when saving.
* Terrain objects will now only pop up a poperties box for setting a trigger in TD mode.
* Optimised loading so the editor will skip loading objects from different theaters.
* Fixed user settings loading, so it can port over the settings (game folder, invite warning, and the dialog locations) from previous versions. (It's a hacky system, but it works.)
* Added support for loading mod xml info and graphics through the "ModsToLoad" setting in "CnCTDRAMapEditor.exe.config". The syntax is a semicolon-separated list, with each entry either a Steam workshop ID, or a folder under "Documents\CnCRemastered\Mods\". As folder, the path must contain the "Tiberian_Dawn" or "Red_Alert" part at the start. That prefix folder will also be used as consistency check for the mod type as defined inside "ccmod.json". Mods given by folder name will also be looked up in the Steam workshop folders, with the prefix folder used only for the consistency check. Mods do NOT have to be enabled in the game to work in the editor.
* Added support for the unique pattern of TD's CONC pavement. You will need the "ConcretePavementTD" mod to actually see that, though. This mod is enabled by default in the editor's settings, so it will automatically be used if found.
* Fixed loading and saving of the videos set in the map options dialog, so no more errors pop up there.
* Made video names freely editable for TD missions. Any mod-added video in TD is playable from missions.
* The preview selection in the Steam publish dialog will now open in the correct folder.
* The preview rendered for singleplayer maps for the Steam publish (which is not used by default) will show all map contents.
* Removed crater types CR2 to CR6; they don't work correctly in either game and will just show the smallest size of CR1. Any craters of other types encountered on map load will now be converted to CR1.
* The teamtypes dialog no longer uses data grids for its teams and orders.
* The controls of the orders now correctly adapt to the types of each order, giving dropdowns for special choices lists and for waypoints.
* The waypoints that can be selected for an RA teamtype now correctly start from -1 as "(none)".
* Fixed colour of "Special" in RA to have the same colour as Spain.
* Trigger Events and Actions retained their argument data when changing their type, meaning the UI would pick the equivalent data on whatever list or control popped up for the new type. This has been fixed.
* RA triggers now show human-readable data for the Event and Action arguments.
* The editor no longer locks up when the triggers dialog shows an empty list of teamtypes or triggers because none were made yet.
* Removed Aircraft section handling from TD.
* Like walls, overlay placement and removing can now be dragged to affect multiple cells.
* All waypoint will now be shown with their coordinates.
* Added "Jump to..." button on the waypoints tool. This will only have any effect when zoomed in.
* Clicking overlapping waypoints multiple times will cycle to the next one in the list on each click. Right-clicking will cycle backwards.
* When deleting overlapping waypoints, if the currently selected waypoint is one of them, that one will be deleted first.
* Map indicators will now be painted in this order: map boundaries, celltriggers, waypoints, object triggers. The later ones will be on top and thus most visible.
* Map indicators for the type you are currently editing are now always drawn last, and thus the most visible. (e.g. overlapping celltriggers and waypoints)
* Unit/building/infantry tools now paint the object trigger labels last, so they no longer get painted over by the occupied cell indicators.
* TGA files that are not inside a .zip file can now also load their positioning metadata from accompanying .meta files.
* Factory doors will no longer be seen as semitransparent on the placement preview.
* "Quality" factor in the config file (indicating a downscaling factor of the graphics) now accepts negative values to enable smooth scaling.
* Object previews will now obey the "Quality" factor set in the config file. However, they will always be displayed using pixel scaling, because smooth scaling looks awful on the zoomed-in preview panels.
* Fixed incorrect cleanup when switching between tools, which could cause odd bugs like two selected cells being visible on the tileset tool.
* Terrain and structure editing mode will now always draw the full green bounds underneath the red occupied cells.
* Fixed incorrect footprint for desert-theater terrain element "rock2".
* Optimised all calculations related to centering objects in their bounding box and drawing them on the map.
* Infantry are now positioned more accurately.
* The terrain tool now uses a list box like all the other tools, instead of the awkward dropdown list.
* The smudge tool now allows setting the size in the preview window, and picking craters with a different size from the map.
* Previews in tool windows will now use higher quality graphics than the map by default. This can be adjusted in the CnCTDRAMapEditor.exe.config file, by changing the "MapScaleFactor" and "PreviewScaleFactor".
