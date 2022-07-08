## C&C Tiberian Dawn and Red Alert Map Editor

An enhanced version of the C&C Tiberian Dawn and Red Alert Map Editor based on the source code released by Electronic Arts.
The goal of the project is simply to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base,
enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Features added by Rampastring:

* Downsized menu graphics by an user-configurable factor so you can see more placeable object types at once on sub-4K monitors
* Improved zoom levels
* Fixed a couple of crashes
* Made tool windows remember their previous position, size and other settings upon closing and re-opening them
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks 

### Features and fixes by Nyerguds (so far):

* Fixed Overlay height overflow bug in Rampa's new UI.
* Fixed tiles list duplicating every time the "Map" tool window is opened.
* Split off internal Overlay type "decoration", used for pavements and civilian buildings.
* Added CONC and ROAD pavement. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in teamtypes.
* Removed the Aircraft from the placeable units in TD.
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc.)
* Fixed case sensitivity related crashes in TD teamtypes.
* Added Ctrl-N, Ctrl+O, Ctrl+S etc shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tooltip window to adjust crater stage.
* Fixed Terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
* Red Alert "Spied by..." trigger event now shows the House to select.
* Added "Add" buttons in triggers and teamtypes dialogs.
* Fixed tab order in triggers and teamtypes dialogs.
* Fixed crash in "already exists" messages for triggers and teams.
* Randomised tiberium on save, like the original WW editor does.
* [EXPERIMENTAL] Added ability to place bibs. They won't show their full size in the editor at the moment, though.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though.
