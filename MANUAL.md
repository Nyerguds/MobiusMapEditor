# Mobius Map Editor - Manual

## Installation

**DO NOT unpack this in the C&C Remastered Collection's install folder.** It is absolutely unnecessary to overwrite any files of the installed game.

Simply unpack the editor into a new folder on your disk somewhere. On first startup, it will automatically try to detect the folder in which the game is installed, and if it can't find it, it will show a popup asking you to locate it. Note that this autodetect only works on Steam installations of the game.

---

## Usage

The creators of the map editor have chosen to build a manual into the editor, but it might not be immediately obvious. Look at the bottom bar, and it will tell you for the currently selected editing type what your mouse buttons will do, and which modifier keys will change to different editing modes. Once you hold down such a key, the bottom bar text will change, further explaining what your mouse buttons will do in this specific mode. Several types of objects can also be dragged around, and will change the bottom bar text accordingly when the mouse button is pressed down on an object.

Besides that, the scroll wheel will allow zooming in and out, and holding down either the middle mouse button or the space bar will allow you to quickly drag-scroll around the map.

Specific options about the map and the scripting elements can be found in the "Settings" menu:

* "Map" will allow you to set the map's name, indicate whether it is a single player mission, set videos for mission startup, win, and lose, and configure the player's House (for singleplayer) and set alliances and other settings for all the Houses used on the map.
* "Teamtypes" will allow you to define teams that can be used in the map's scripting, or that will simply be created randomly as attack teams by the AI Houses.
* "Triggers" will allow you to make scripts that execute on the map. Note that scripting is mostly a singleplayer thing, and is severely limited in multiplayer, especially in Tiberian Dawn.

The triggers dialog contains a "Check" button that will check if any configurations in the triggers might not work, or might even cause game crashes. For TD, these checks are based on [the TD triggers overview guide I wrote on Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=2824756756). Note that this is not a scripting guide; it is an overview of what each trigger event and action will accept as inputs, and produce as output, highlighting potential issues and some workarounds.

### The "New from image" function

The editor contains a function to create a new map starting from an image, also usable with the drag-and-drop feature. This is not some magical conversion tool, however; it needs to be used in a very specific way.

The function is meant to allow map makers to plan out the layout of their map in an image editor, with more tools available in terms of symmetry, copy-pasting, drawing straight lines, drawing curves, etc. Each pixel on the image represents one cell, so the image should be the size of a full map; 64x64 for Tiberian Dawn, and 128x128 for Red Alert. If the image is smaller, it will be expanded with black. If it is larger, only the upper left corner will be used.

After selecting the map type and the image, a dialog will be shown where colors can be mapped to a specific tileset icon type. This function only handles distinct colors, so avoid using tools that use smooth color transitions; the final image should probably only contain some 3-10 distinct colors. Note that the alpha factor of the colors will be ignored.

The result of this is obviously not an immediately usable map. It will produce a rough layout which will then need to be overlayed with actual cliffs, rivers and shores to turn it into an actual map. For Red Alert Interior theater, you can probably do a lot more preparation work in the image editor; all corridor sizes can be laid out exactly, and you can even pre-place areas where wall shadows will come, so they can be taken care of with a few quick flood fill commands in the editor.

---

## Configuration

The file "CnCTDRAMapEditor.exe.config" contains settings to customise the editor. This is what they do:

### Mods:

* **ModsToLoadTD** / **ModsToLoadRA** / **ModsToLoadSS**: semicolon (or comma) separated list of mod entries for each supported game.

A mod entry can either be a Steam workshop ID, or a folder name. The paths will initially be looked up in the mods folder of the respective game in the CnCRemastered\mods\ folder under your Documents folder, but the loading system will also check the Steam workshop files for a matching mod. Sole Survivor will use Tiberian Dawn mods. Note that mods can only apply graphical changes from the tileset and house color xml files; the editor can't read any data from compiled dll files. This mods system is mostly meant to apply graphical fixes to the editor.

The **ModsToLoadTD** and **ModsToLoadSS** settings will have the `GraphicsFixesTD` mod set by default, to complete the incomplete TD Remastered graphics set, meaning the mod will automatically be loaded if found. Note that the editor has no way to check whether mods are enabled in the game, so that makes no difference.

You can find the mod [on the Steam workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2844969675) and [on ModDB](https://www.moddb.com/games/command-conquer-remastered/addons/graphicsfixestd).

### Defaults:

* **DefaultBoundsObstructFill**: Default for the option "Tools" → "Options" → "Flood fill is obstructed by map bounds".  When enabled, and filling map tiles with [Ctrl]+[Shift]+[Click], the map boundary acts as border blocking the fill spread. This applies both inside and outside the border.
* **DefaultTileDragProtect**: Default for the option "Tools" → "Options" → "Drag-place map tiles without smearing". When placing tiles in map mode, and dragging around the mouse, this option will make sure a new tileset block is only placed after fully leaving the previously-placed blocks inside that one mouse action.
* **DefaultTileDragRandomize**: Default for the option "Tools" → "Options" → "Randomize drag-placed map tiles". When placing a tile and holding down the mouse to drag more, this will make the subsequently placed tiles randomise between equivalents of the same size.
* **DefaultShowPlacementGrid**: Default for the option "Tools" → "Options" → "Show grid when placing / moving". This option enables showing the map grid when in placement mode (and/or holding down [Shift]) or when dragging a placed down object to a different location.
* **DefaultOutlineAllCrates**: Default for the option "Tools" → "Options" → "Crate outline indicators show on all crates". When enabled, the crate indicators from the "View" → "Indicators" → "Outlines on overlapped crates" option will show on all crates instead of just those underneath objects or graphics.
* **DefaultCratesOnTop**: Default for the option "Tools" → "Options" → "Show crates on top of other objects".
* **DefaultShowMapGrid**: Default for the option "View" → "Extra Indicators" → "Map grid". When enabled, this always shows the map grid, regardless of the "Show grid when placing / moving" option.
* **DefaultExportScale**: Default scaling multiplier for the size at which an exported image will be generated through "Tools" → "Export As Image". A negative values will set it to smooth scaling. Defaults to -0.5.

### Editor fine tuning:

* **MapGridColor**: Color for drawing the map grid, as "A,R,G,B". This includes the alpha value, because the grid is semitransparent by default.
* **MapBackColor**: Background color for the map screen, as "R,G,B". This defaults to dark grey, so users can see the actual map outline on Red Alert Interior maps.
* **MapScale**: Scaling multiplier for the size at which assets are rendered on the map. Scaling down the rendered map size will make the UI more responsive. Negative values will enable smooth scaling, which gives nicer graphics but will make the UI noticeable _less_ responsive. Defaults to 0.5.
* **PreviewScale**: Scaling multiplier for the size at which assets are rendered on the preview tools. Negative values will enable smooth scaling, but this usually doesn't look good on the upscaled preview graphics. Defaults to 1.
* **ObjectToolItemSizeMultiplier**: Floating-point multiplication factor for downsizing the item icons on the selection lists on the tool windows.
* **TemplateToolTextureSizeMultiplier**: Floating-point multiplication factor for the size of tiles shown on the Map tool. This scaling is somehow done relative to the screen size; not sure.
* **MaxMapTileTextureSize**: Maximum for the size of the tiles shown on the Map tool. Leave on 0 to disable.
* **UndoRedoStackSize**: The amount of undo/redo actions stored in memory. Defaults to 50.
* **MinimumClampSize**: Minimum size of the tool window that will automatically be forced to remain in the screen area. If set to 0,0, this will default to the size of the entire tool window.

### Editor behavior tweaks:

These options are all enabled by default, but can be disabled if you wish. Use these at your own risk.

* **Ignore106Scripting**: Don't support the extended scripting added by the C&C95 v1.06 patch. If this option is disabled, additional triggers named UUUU, VVVV and WWWW can also be destroyed with "Dstry Trig" actions.
* **NoMetaFilesForSinglePlay**: Suppresses the generation of .tga and .json files for single player maps saved to disc, since they are useless clutter and unused by the game. This does not affect Steam uploads. Note that json files for single player maps will now only contain the Home waypoint.
* **ConvertRaObsoleteClear**: Automatically clear tiles with ID 255 on RA Temperate/Snow maps, or on Interior maps if more than 80% of the area outside the map bounds is filled with it, to fix the fact old versions of RA saved that as Clear terrain. This can be disabled to research changes on old maps.
* **BlockingBibs**: Bibs block the placement of other structures. Note that if you disable this, you should be careful not to block the build plan of rebuildable AI structures. Also, the games might have issues with walls overlaying building bibs.
* **DisableAirUnits**: Air unit reading from maps was a disabled feature in the original games. Even though the Remaster re-enabled this, it is buggy and unpredictable, so the editor disables air units by default. Air units put on maps will not appear on the specified cell; they will spawn in the air above it, will either fly off the map or find a nearby building of their House to land at, and (in TD) will usually leave behind an impassable cell on the map under the place where they spawned.
* **ConvertCraters**: Any craters of the types CR2-CR6 placed in missions are bugged in the games, and revert to the smallest size of CR1. This filters them out and converts them to CR1 craters of the specified size, and removes the other crater types from the Smudge choices list.
* **FilterTheaterObjects**: Filter out objects that don't belong in the current map's theater. This affects both map loading, and the items visible in the placement lists. Do not turn this off unless you really know what you're doing; having theater-illegal objects on maps may cause situations that crash the game.
* **WriteClassicBriefing**: In addition to the single-line "Text=" briefing used by the Remaster, also write classic-style briefings into the ini file as "1=", "2=", etc. lines split at human-readable length. This includes the C&C95 v1.06 line break system using ## characters at the end of a line.
* **ApplyHarvestBug**: The game has a bug where the final harvested stage of a cell yields nothing (or only 3/4th for RA gems). Assume this bug is unfixed when doing the resource calculations.
* **NoOwnedObjectsInSole**: Sole Survivor maps normally don't include placed down units, structures or infantry, so loading and saving them is disabled by default. But it seems some official maps do contain decorative civilian buildings, and old betas of the game could read those, so this option can be disabled for research purposes.
* **DrawSoleTeleports**: On Sole Survivor maps, draw a black area with blue border over the loaded ROAD graphics to emulate the look of the in-game teleporters.
* **RestrictSoleLimits**: When analysing / saving Sole Survivor maps, use the original object amount limits of the game, rather than the Remaster's larger values.
