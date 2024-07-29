# Mobius Map Editor - Manual

## Installation

**DO NOT unpack this in the C&C Remastered Collection's install folder.** It is absolutely unnecessary to overwrite any files of the installed game.

Simply unpack the editor into a new folder on your disk somewhere. On first startup, it will automatically try to detect the folder in which the game is installed, and if it can't find it, it will show a popup asking you to locate it. Note that this autodetect only works on Steam installations of the game.

If the C&C Remastered Collection is not installed on your PC, a warning will be shown telling you it will start in Classic Files mode instead, using the assets of the original 90's games. To suppress this warning, you can edit the config file and enable the option to always use classic graphics. (See the "Configuration" section below.)

---

## Usage

The creators of the map editor have chosen to build a manual into the editor, but it might not be immediately obvious. Look at the bottom bar, and it will tell you for the currently selected editing type what your mouse buttons will do, and which modifier keys will change to different editing modes. Once you hold down such a key, the bottom bar text will change, further explaining what your mouse buttons will do in this specific mode. Several types of objects can also be dragged around, and will change the bottom bar text accordingly when the mouse button is pressed down on an object.

Besides that, the scroll wheel will allow zooming in and out, and holding down either the middle mouse button or the space bar will allow you to quickly drag-scroll around the map.

Specific options about the map and the scripting elements can be found in the "Settings" menu:

* "Map" will allow you to set the map's name, indicate whether it is a single player mission, set videos for mission startup, win, and lose, and configure the player's House (for singleplayer) and set alliances and other settings for all the Houses used on the map.
* "Teamtypes" will allow you to define teams that can be used in the map's scripting, or that will simply be created randomly as attack teams by the AI Houses.
* "Triggers" will allow you to make scripts that execute on the map. Note that scripting is mostly a singleplayer thing, and is severely limited in multiplayer, especially in Tiberian Dawn.

The triggers dialog contains a "Check" button that will check if any configurations in the triggers might not work, or might even cause game crashes. For TD, these checks are based on [the TD triggers overview guide I wrote on Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=2824756756). Note that this is not a scripting guide; it is an overview of what each trigger event and action will accept as inputs, and produce as output, highlighting potential issues and some workarounds.

### Hotkeys

You can switch between the different editing modes using the six first letters on the top two rows on your keyboard; Q-W-E-R-T-Y and A-S-D-F-G on classic a US qwerty keyboard. Note that these keys are interpreted positionally on the keyboard, meaning that they will work in the intended logical way on different-region keyboard, like the German 'qwertz' and French 'azerty'.

Besides those, PageUp and PageDown have been universally implemented to let you switch to the next / previous item on the current editing tool's selection list, with Home and End going to the start and end of the list. This also works for increasing/decreasing the resource placement size in Resources mode.

Some editing modes will have their own specific shortcuts, like the ones to select specific special waypoints in Waypoints mode. Those will be indicated in the bottom bar along with the mouse function modifiers.

Note that these hotkeys only work when the main window is selected; if you click on the tool window to select it, all keys will work as expected inside the selected controls.

### Sole Survivor and Megamaps

Some of you might remember Sole Survivor; the rather obscure death match arena spinoff of Command & Conquer 1. It's a game in which you control a single unit, collect crates to upgrade that unit, and kill your enemies. The game engine is a trimmed version of the Tiberian Dawn one with the base building and harvesting parts disabled, but it has one interesting upgrade that the original game didn't have: its battle arena maps are 128x128; the same size as Red Alert's maps.

Some mod makers found that an interesting feature, since it means there is an official Tiberian Dawn megamap format, and so, some mods on the C&C Remastered workshop, like [CFE Patch Redux](https://steamcommunity.com/sharedfiles/filedetails/?id=2239875646), and [john_drak's updated branch of it](https://steamcommunity.com/sharedfiles/filedetails/?id=3002363531), support this format. The [Vanilla Conquer](https://github.com/TheAssemblyArmada/Vanilla-Conquer) project, which reconstructed the original Tiberian Dawn source code from the remaster code, also supports it. And so, I decided to support it in this editor too, to make larger maps and missions that are playable on these.

In recent years, quite some progress has been made in getting Sole Survivor's server-side infrastructure running again, and so, the game might soon actually be playable again. So for that reason, actual support for Sole Survivor was added as well, including its own special waypoints and map options.

### The "New from image" function

The editor contains a function to create a new map starting from an image, also usable with the drag-and-drop feature. This is not some magical conversion tool, however; it needs to be used in a very specific way.

The function is meant to allow map makers to plan out the layout of their map in an image editor, with more tools available in terms of symmetry, copy-pasting, drawing straight lines, drawing curves, etc. Each pixel on the image represents one cell, so the image should be the size of a full map; 64x64 for Tiberian Dawn, and 128x128 for Red Alert and Sole Survivor (and Tiberian Dawn megamaps). If the image is smaller, it will be expanded with black. If it is larger, only the upper left corner will be used.

After selecting the map type and the image, a dialog will be shown where colors can be mapped to a specific tileset icon type. This function only handles distinct colors, so avoid using tools that use smooth color transitions; the final image should probably only contain some 3-10 distinct colors. Note that the alpha factor of the colors will be ignored.

The result of this is obviously not an immediately usable map. It will produce a rough layout which will then need to be overlayed with actual cliffs, rivers and shores to turn it into an actual map. For Red Alert Interior theater, you can probably do a lot more preparation work in the image editor; all corridor sizes can be laid out exactly, and you can even pre-place areas where wall shadows will come, so they can be taken care of with a few quick flood fill commands in the editor.

### Randomizable tiles

In Red Alert's Interior theater, the editor unlocks access to an unused game feature, namely, the ability to use the random alternates that exist in most of the Interior theater's 1x1 tile types. None of the original game maps ever use these alternates, but both the original game and the remaster can perfectly show them if they are present in maps.

On the preview window, such tiles will be indicated with a light blue grid drawn over them. When you have such a tile type selected, placing down a tile will randomly place one of the available tiles. Of course, just as with any other tile, you can right-click on the map (or left-click on the preview window) to select a specific tile to place, which will disable the randomizing feature. Right-clicking in the preview window will remove the specifically-selected cell and re-enable the randomization.

The randomizability feature is also applied the other way around: to remove clutter, three ranges of equivalent 1x1 wall tiles were packed together into three randomizable tileset groups. These are "wallgroup1", "wallgroup2" and "wallgroup3". If you place down any of these walls on the map and hover your mouse over the tile, you will see the actual tiles are identified as wall0002 to wall0022.

The Tools menu has a specific "Re-randomize tiles" option to automatically apply this randomization to existing maps. This will include randomizing the walls. Note that even though the specific corner pieces in wall0023 to wall0049 are pretty much equivalent to combinations of these randomizable wall tiles, they are not affected by this operation.

### The "Publish" function

The "Publish" option in the menu will allow you to upload your map to the Steam Workshop. It only works if you have the C&C Remastered game installed through Steam.

If the editor is running in Classic mode (see below), the editor will never ask for your C&C Remaster game folder, however, if it hasn't already been set, it *will* attempt to auto-detect the Steam game folder, and if it succeeds, the Publish function will be available as usual.

If, for some reason, the Steam game folder detection fails, you can get the prompt to manually select your game folder by disabling classic mode and restarting the editor.

### Map passability

Passability is an option that can be enabled under View → Extra indicators, or simply by pressing F3. It indicates where stuff can move, and which areas can't be built on. Once enabled, it will be shown both on the map and on the map tile placement preview. Note that this only evaluates the map terrain tiles, and does not account for the things placed on top of them. You will need to check the footprints of these objects in their own editing mode to see which cells they occupy.

The different passability states are:

* No hashing: Passable for land units.
* Red: Land impassable for all units.
* Yellow: Passable, but can't be built on (rough terrain or beach)
* White: Passable for ships only.
* Blue: Water impassable for all units.

For Red Alert, these can be adjusted by adding modified rules.ini land type definitions into the mission (or in a loaded mod). If so, note that these indicators might change in unpredictable ways. Terrain is considered "passable for land units" by the editor if either foot, track or wheel units have a movement speed of more than 0 on it.

Tiberian Dawn and Sole Survivor only have one water type, which is all passable to water units, no matter how bizarre it looks. Though in practice this is only important for TD hovercraft reinforcements, which need unobstructed water to travel through.

---

## Local settings storage

The editor has two kinds of settings; global settings used on every run, and modifiable settings that can get changed during the program run. The global settings are those detailed in the "Configuration" section below. The modifiable settings include things like the game path and window positions, and will automatically get stored under the user-folder. If, for any reason, you would want to clear these settings and start the editor with a clean slate, simply open the File Explorer, paste the following into the address bar and press [enter]:

**\%localappdata\%\\Nyerguds\\**

This should make you end up in the "AppData\Local\Nyerguds" folder under your Windows user folder. Removing this folder will clear all of the editor's user settings.

## Configuration

The file "CnCTDRAMapEditor.exe.config" contains settings to customise the editor. This is what they do.

Note that this listing is updated as I develop. If options mentioned in this list do not exist in the settings file, chances are they refer to features that are already implemented in the source code on GitHub, but not yet released in a new version to download.

### Using classic files:

* **UseClassicFiles**: Disabled by default, so the editor can ask you for your C&C Remastered game folder, but if you don't own the Remaster, or prefer the classic graphics, simply set this to "True".
* **ClassicPathTD** / **ClassicPathRA** / **ClassicPathSS**: Path to load the classic files from for each of the game types when running in Classic Files mode. If the directory entered in this cannot be found, this reverts to predefined subfolders under the editor's location; "Classic\TD" for Tiberian Dawn and Sole Survivor, and "Classic\RA" for Red Alert. If these folders are not found either, the editor will check if it is ran from the C&C Remastered folder, by checking for the existence of the classic folders inside the CNCDATA folder. If the data is not present at the given location, the editor will refuse to launch in classic mode.
* **ClassicNoRemasterLogic**: Defaults to False. When enabled, using classic mode will make it stop doing remaster-specific checks (such as briefing screen constraints in RA) or use the Remaster's specific folders under Documents.
* **ClassicProducesNoMetaFiles**: Defaults to False. Suppresses the creation of xml and thumbnail files for multiplayer maps when in Classic Files mode.

Using classic files will not only use the classic graphics, but will also load the classic game text from the respective game's 'CONQUER.ENG' file, and the Red Alert house colours from 'PALETTE.CPS'.

The default "Classic\TD" and "Classic\RA" folders are supplied along with the editor, so it is immediately usable in classic mode. The contents of these folders were taken from the official freeware releases of the games, supplemented with some files from the Red Alert expansion packs. For the exact expected contents of the folders, see the "Classic files listing" section below.

### General editor options

* **EnableDpiAwareness**: On some machines with high dpi monitors, people might have odd issues where the positioning of the indicators on the map doesn't correctly match the map tiles. If this happens, enabling this option might fix the problem.
* **EditorLanguage**: This option can change the language the editor loads for the remastered game text to name the objects in the editor by specifying a culture code in a format such as "EN-US". This only works for languages that are supported by the C&C Remaster as in-game language. When set to "Auto", it will attempt to use the system language, or the nearest supported one that matches it. To force the default English language, you can leave the setting empty, or use "Default" or "None". The supported languages of the game are: EN-US, DE-DE, ES-ES, FR-FR, KO-KR, PL-PL, RU-RU, ZH-CN, ZH-TW.
* **CheckUpdatesOnStartup**: Enabled by default. Will make the editor notify users if a new version is available.

### Mods:

* **ModsToLoadTD** / **ModsToLoadRA** / **ModsToLoadSS**: semicolon (or comma) separated list of mod entries for each supported game.

A mod entry can either be a Steam workshop ID, or a folder name. The paths will initially be looked up in the mods folder of the respective game in the CnCRemastered\mods\ folder under your Documents folder, but the loading system will also check the Steam workshop files for a matching mod. Sole Survivor will use Tiberian Dawn mods. Note that mods can only apply graphical changes from the tileset and house color xml files; the editor can't read any data from compiled dll files. This mods system is mostly meant to apply graphical fixes to the editor.

The **ModsToLoadTD** and **ModsToLoadSS** settings will have the `GraphicsFixesTD` mod set by default, to complete the incomplete TD Remastered graphics set, meaning the mod will automatically be loaded if found. Similarly, the **ModsToLoadRA** setting will have the `GraphicsFixesRA` mod set. Note that the editor has no way to check whether mods are enabled in the game, so that makes no difference.

You can find these mods on the Steam workshop ([GraphicsFixesTD](https://steamcommunity.com/sharedfiles/filedetails/?id=2844969675), [GraphicsFixesRA](https://steamcommunity.com/sharedfiles/filedetails/?id=2978875641)) and on ModDB ([GraphicsFixesTD](https://www.moddb.com/games/command-conquer-remastered/addons/graphicsfixestd), [GraphicsFixesRA](https://www.moddb.com/games/cc-red-alert-remastered/addons/graphicsfixesra)).

In classic graphics mode, the editor can still use mods, if they contain classic files in a "ccdata" folder. The 'GraphicsFixesRA' mod has such a classic component, to fix the classic graphics of Einstein and the ant buildings.

### Defaults:

* **DefaultBoundsObstructFill**: Default for the option "Tools" → "Options" → "Flood fill is obstructed by map bounds".  When enabled, and filling map tiles with [Ctrl]+[Shift]+[Click], the map boundary acts as border blocking the fill spread. This applies both inside and outside the border.
* **DefaultTileDragProtect**: Default for the option "Tools" → "Options" → "Drag-place map tiles without smearing". When placing tiles in map mode, and dragging around the mouse, this option will make sure a new tileset block is only placed after fully leaving the previously-placed blocks inside that one mouse action.
* **DefaultTileDragRandomize**: Default for the option "Tools" → "Options" → "Randomize drag-placed map tiles". When placing a tile and holding down the mouse to drag more, this will make the subsequently placed tiles randomize between equivalents of the same size.
* **DefaultShowPlacementGrid**: Default for the option "Tools" → "Options" → "Show grid when placing / moving". This option enables showing the map grid when in placement mode (and/or holding down [Shift]) or when dragging a placed down object to a different location.
* **DefaultOutlineAllCrates**: Default for the option "Tools" → "Options" → "Crate outline indicators show on all crates". When enabled, the crate indicators from the "View" → "Indicators" → "Outlines on overlapped crates" option will show on all crates instead of just those underneath objects or graphics.
* **DefaultCratesOnTop**: Default for the option "Tools" → "Options" → "Show crates on top of other objects".
* **DefaultExportScale**: Default scaling multiplier for the size at which an exported image will be generated through "Tools" → "Export As Image". A negative values will set it to smooth scaling. Defaults to -0.5.
* **DefaultExportScaleClassic**: Default scaling multiplier for exporting images in when using classic graphics. Defaults to 1.0.

### Editor fine tuning:

* **ZoomToBoundsOnLoad**: Defaults to True. When enabled, causes the editor to zoom in to the map bounds when loading an existing map.
* **RememberToolData**: Defaults to False. When enabled, the item selections and options on the tool windows will be remembered when opening a different or new map for the same game.
* **MapGridColor**: Color for drawing the map grid, as "A,R,G,B". This includes the alpha value, because the grid is semitransparent by default.
* **MapBackColor**: Background color for the map screen, as "R,G,B". This defaults to dark grey, so users can see the actual map outline on Red Alert Interior maps.
* **MapScale**: Scaling multiplier for the size at which assets are rendered on the map. Scaling down the rendered map size will make the UI more responsive. Negative values will enable smooth scaling, which gives nicer graphics but will make the UI noticeable _less_ responsive. Defaults to 0.5.
* **MapScaleClassic**: Scaling multiplier when using classic graphics. Defaults to 1.0.
* **PreviewScale**: Scaling multiplier for the size at which assets are rendered on the preview tools. Negative values will enable smooth scaling, but this usually doesn't look good on the upscaled preview graphics. Defaults to 1. This value is automatically adjusted for Classic mode by multiplying it with 128/24.
* **ObjectToolItemSizeMultiplier**: Floating-point multiplication factor for downsizing the item icons on the selection lists on the tool windows.
* **TemplateToolTextureSizeMultiplier**: Floating-point multiplication factor for the size of tiles shown on the Map tool. This scaling is somehow done relative to the screen size; not sure.
* **MaxMapTileTextureSize**: Maximum for the size of the tiles shown on the Map tool. Leave on 0 to disable.
* **UndoRedoStackSize**: The amount of undo/redo actions stored in memory. Defaults to 100.
* **MinimumClampSize**: Minimum size of the tool window that will automatically be forced to remain in the screen area. If set to 0,0, this will default to the size of the entire tool window.

### Editor behavior tweaks:

These options are all enabled by default, but can be disabled if you wish. Use these at your own risk. Some of these (air units, craters, harvesting) are related to bugs in the games, so they could be disabled when making maps for a mod in which these issues are fixed.

* **ReportMissionDetection**: When detecting that a file is a classic single player mission file because it matches the classic "SCG01EA"-like name pattern and contains win and lose scripts, a note about it is shown in the mission load analysis. When disabled, this will only be shown if it is not the only remark in the list.
* **EnforceObjectMaximums**: Don't allow saving a map if any of the the object amounts exceed the normal internal maximums of the game. Can be disabled in case a mission is specifically meant to be played on a modded game that increases these limits.
* **Ignore106Scripting**: Don't support the extended scripting added by the C&C95 v1.06 patch. If this option is disabled, additional triggers named UUUU, VVVV and WWWW can also be destroyed with "Dstry Trig" actions.
* **ConvertRaObsoleteClear**: Automatically clear tiles with ID 255 on RA Temperate/Snow maps, or on Interior maps if more than 80% of the area outside the map bounds is filled with it, to fix the fact old versions of RA saved that as Clear terrain. This can be disabled to research changes on old maps.
* **BlockingBibs**: Bibs block the placement of other structures. Note that if you disable this, you should be careful not to block the build plan of rebuildable AI structures. Also, the games might have issues with walls overlaying building bibs.
* **DisableAirUnits**: Air unit reading from maps was a disabled feature in the original games. Even though the Remaster re-enabled this, it is buggy and unpredictable, so the editor disables air units by default. Air units put on maps will not appear on the specified cell; they will spawn in the air above it, will either fly off the map or find a nearby building of their House to land at, and (in TD) will usually leave behind an impassable cell on the map under the place where they spawned. Note that any "preplaced" Chinook helicopters that might appear in missions are actually flown in by scripts at the start of the mission.
* **ConvertCraters**: Any craters of the types CR2-CR6 placed in maps are bugged in the games, and revert to the smallest size of CR1. This filters them out and converts them to CR1 craters of the specified size, and removes the other crater types from the Smudge choices list.
* **FilterTheaterObjects**: Filter out objects that don't belong in the current map's theater. This affects both map loading, and the items visible in the placement lists. Do not turn this off unless you really know what you're doing; having theater-illegal objects on maps may cause situations that crash the game.
* **WriteClassicBriefing**: In addition to the single-line "Text=" briefing used by the Remaster, also write classic-style briefings into the ini file as "1=", "2=", etc. lines split at human-readable length. This includes the C&C95 v1.06 line break system using ## characters at the end of a line.
* **ApplyHarvestBug**: The game has a bug where the final harvested stage of a cell yields nothing (or only 3/4th for RA gems). Assume this bug is unfixed when doing the resource calculations.
* **DontAllowWallsAsBuildings**: if this option is set to False, buildings will show up in the Buildings list as well, from where they can be placed down as player-owned buildings. This allows selling them, but it is generally not advised since it bloats the ini file.
* **NoOwnedObjectsInSole**: Sole Survivor maps normally don't include placed down units, structures or infantry, so loading and saving them is disabled by default. But it seems some official maps do contain decorative civilian buildings, and old betas of the game could read those, so this option can be disabled for research purposes.
* **RestrictSoleLimits**: When analysing / saving Sole Survivor maps, use the original object amount limits of the game, rather than the Remaster's larger values.

### Graphical tweaks:

These don't affect any real behaviour, but change some graphics to look more correct in the editor:

* **FixClassicEinstein**: While the Win95 and remastered versions of Red Alert have Einstein's in-game sprite coloured to match how he appears in the briefings, the DOS version (which the editor and the game use) looks identical to Dr. Mobius in Tiberian Dawn. This option makes the editor shuffle around some colours in the classic DOS sprite so it matches that same colour scheme. Note that the **GraphicsFixesRA** mod also fixes this.
* **FixConcretePavement**: The connection logic of the "CONC" pavement in Tiberian Dawn is seriously bugged in-game. The editor contains a fixed logic, showing the concrete how it was intended to be, filling in side gaps with filler cells. However, be advised that this new logic does not match the actual game. For this reason, it is disabled by default.
* **DrawSoleTeleports**: On Sole Survivor maps, draw a black area with blue border over the loaded ROAD graphics to emulate the look of the in-game teleporters.

## Classic files listing:

The following files can be read from the configured classic data folders, for running the editor in classic mode. They can also be loaded from mod folders. They will be loaded in the listed order, from any available sources. The basic rule in the game is that each file name can only be loaded once, and the first-loaded files have priority, so this also shows which archives can override the contents of which other archives.

Files marked with <sup>(*)</sup> are required, though they may not be visible inside the folder if they are embedded inside another archive.

Files marked with <sup>(1)</sup> (the sc*.mix archives) are add-ons. Anything matching the pattern will be read, but they still obey the rule that each archive name is only read from one location.

Files marked with <sup>(2)</sup> (RA only) can be embedded inside the ''redalert.mix'' or ''main.mix'' archive.

Note that there is no support for running the editor for one specific game only, while not having the files for the other game(s) available. All data needs to be present to make the editor start up.

### Tiberian Dawn and Sole Survivor

These are read from the "Classic\TD" subfolder by default. Note that the editor has not been specifically tested on an actual Sole Survivor folder.

* cclocal.mix (or local.mix) <sup>(*)</sup>
* sc*.mix <sup>(1)</sup>
* conquer.mix <sup>(*)</sup>
* desert.mix <sup>(*)</sup>
* temperat.mix <sup>(*)</sup>
* winter.mix <sup>(*)</sup>

### Red Alert

These are read from the "Classic\RA" subfolder by default.

* expand2.mix
* expand.mix
* redalert.mix
* main.mix
* local.mix <sup>(*)</sup> <sup>(2)</sup>
* sc*.mix <sup>(1)</sup>
* general.mix <sup>(2)</sup>
* conquer.mix <sup>(*)</sup> <sup>(2)</sup>
* lores.mix <sup>(*)</sup> <sup>(2)</sup>
* lores1.mix <sup>(2)</sup>
* interior.mix <sup>(*)</sup> <sup>(2)</sup>
* snow.mix <sup>(*)</sup> <sup>(2)</sup>
* temperat.mix <sup>(*)</sup> <sup>(2)</sup>

The ''hires.mix'' and ''hires1.mix'' archives are not used; like the Red Alert Remaster itself, the editor uses the DOS versions of the infantry.

The expansions data is not strictly required. If given a folder whithout those files, dummy graphics will be shown for the missing objects.
