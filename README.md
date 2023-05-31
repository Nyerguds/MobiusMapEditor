# Mobius Map Editor

## Project

Mobius Map Editor is an enhanced version of the map editor of the Command & Conquer Remastered Collection, based on the source code released by Electronic Arts.

The editor can edit maps for Command & Conquer Tiberian Dawn, Sole Survivor and Red Alert. The goal of the project is to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base, enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though, but post them as issues.

## Installation

**DO NOT unpack this in the C&C Remastered Collection's install folder.** It is absolutely unnecessary to overwrite any files of the installed game.

Simply unpack the editor into a new folder on your disk somewhere. On first startup, it will automatically try to detect the folder in which the game is installed, and if it can't find it, it will show a popup asking you to locate it. Note that this autodetect only works on Steam installations of the game.

It is advised to install the `GraphicsFixesTD` and `GraphicsFixesRA` mods, to fix errors and add missing bits in the Remastered graphics. The editor will use the mods automatically when they are installed, even if they are not enabled inside the game. You can find them on the Steam workshop ([GraphicsFixesTD](https://steamcommunity.com/sharedfiles/filedetails/?id=2844969675), [GraphicsFixesRA](https://steamcommunity.com/sharedfiles/filedetails/?id=2978875641)) and on ModDB ([GraphicsFixesTD](https://www.moddb.com/games/command-conquer-remastered/addons/graphicsfixestd), [GraphicsFixesRA](https://www.moddb.com/games/cc-red-alert-remastered/addons/graphicsfixesra)).

**The GraphicsFixesRA mod is not currently configured in the settings file to load automatically; this will be amended in the next release. To add it manually, edit `CnCTDRAMapEditor.exe.config` and replace the `<value />` under the "ModsToLoadRA" setting with `<value>2978875641;GraphicsFixesRA</value>`**

For usage and configuration, see [MANUAL.md](MANUAL.md).

## Features

A brief overview of the improvements and added features:

* Multithreading for loading and saving, preventing the application from freezing during these operations.
* Keyboard shortcuts for most functions in the editor.
* Drag & drop support.
* Creating a map starting from an image containing the rough design and symmetry.
* Flood fill function for map tiles, to easily fill large areas in water-based and Interior maps.
* Image export function, with adjustable scale, and choice of shown object types.
* An ini rules section in the map settings, with support for adding keys even in sections like [Basic] that are managed by the editor. The editor will automatically adapt to rule changes in RA maps.
* Sole Survivor and Tiberian Dawn megamap support.
* Different size support for craters.
* A placement grid is shown while placing down or moving objects.
* Mod loading support to allow applying graphics fixes, and filling in graphics for objects that were not remastered.
* Missing objects added: unused decorations and pavements in Overlay and Smudge, and farmer fields and haystacks in Buildings.
* The ability to enable/disable the added units of Red Alert's Aftermath expansion pack in missions.
* Vastly improved triggers and teamtypes editors, with clear descriptions and tooltips for all options, and a trigger analysis function to detect possible issues.
* Improved problem detection in map loading, which will always give a full list of the found issues, rather than refusing to open the map.
* Expanded Undo/Redo functionality to include all actions in the editor.
* Map objects overview, including analysis of unused scripting objects (triggers, globals, teams, waypoints).
* Power balance and silo level tools, which account for buildings scripted to be built later, and for rule tweaks in RA maps.
* Tile randomisation in Red Alert interior maps, allowing the use of previously unused alternate graphics that exist for almost all tiles.
* Options in the editor's .config file can tweak specific editor behaviour, like disallowing overlapping bibs or theater-illegal objects.

The full change log can be viewed in [CHANGELOG.md](CHANGELOG.md)

### Possible future features

Some ideas that might get implemented in the future:

* A function to place random equivalent tiles.
* The ability to change a map's theater.
* A copy & paste function.
* Use classic graphics, making it independent from the Remaster.
