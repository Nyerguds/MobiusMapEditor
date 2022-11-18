# Mobius Map Editor

## Project

Mobius Map Editor is an enhanced version of the map editor of the Command & Conquer Remastered Collection, based on the source code released by Electronic Arts.

The editor can edit maps for Command & Conquer Tiberian Dawn, Sole Survivor and Red Alert. The goal of the project is to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base, enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though, but post them as issues.

## Installation

**DO NOT unpack this in the C&C Remastered Collection's install folder.** It is absolutely unnecessary to overwrite any files of the installed game.

Simply unpack the editor into a new folder on your disk somewhere. On first startup, it will automatically try to detect the folder in which the game is installed, and if it can't find it, it will show a popup asking you to locate it. Note that this autodetect only works on Steam installations of the game.

For usage and configuration, see [MANUAL.md](MANUAL.md).

## Features

A brief overview of the added features:

* Multithreading for loading and saving, preventing the application from freezing during these operations.
* Keyboard shortcuts for most functions in the editor.
* Drag & drop support.
* Flood fill function for map tiles, to easily fill large areas in water-based and Interior maps.
* Image export function, with adjustable scale, and choice of shown object types.
* Sole Survivor and Tiberian Dawn megamap support.
* Different size support for craters.
* Mod loading support to allow filling in graphics for objects that were not remastered.
* Missing objects added: bibs in Smudge, farmer fields and haystacks in buildings, and unused pavements in TD overlay.
* The ability to enable/disable Aftermath units.
* Vastly improved triggers and teamtypes editors, with clear descriptions and tooltips for all options.
* Improved problem detection in map loading, which will always give a full list of the found issues, rather than refusing to open the map.
* Expanded Undo/Redo functionality to all actions in the editor.
* Trigger analysis to detect possible issues.
* Map objects overview, including analysis of unused scripting objects (triggers, globals, teams, waypoints) in singleplayer maps.
* Power balance and silo level tools, which account for buildings scripted to be built later, and for rule tweaks in RA maps.
* Tile randomisation in Red Alert interior maps, allowing the use of previously unused alternates that exist for almost all tiles.
* Options in the main config file can disable specific editor behaviour, like disallowing overlapping bibs or theater-illegal objects.

The full change log can be viewed in [CHANGELOG.md](CHANGELOG.md)

### Possible future features

Some ideas that might get implemented in the future:

* Use classic graphics, making it independent from the Remaster.
* Change a map's theater.
