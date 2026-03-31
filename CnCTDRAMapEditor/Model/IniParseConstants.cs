namespace MobiusEditor.Model
{
    public static class IniParseConstants
    {
        // Movie identification suffixes
        public const string MovieRemarkOld = " (Classic only)";
        public const string MovieRemarkNew = " (Remaster only)";

        // Special warning messages
        public const string SoloMissionDetected = "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.";
        public const string KeyLengthWarning = "{0} '{1}' has a name that is longer than {2} characters." +
                            " This will not be corrected by the loading process, but should be addressed," +
                            " since it can make the {0} fail to link correctly to other map objects and" +
                            " scripting.";

        // Special skip messages.
        public const string SectionDisabled = "{0} section is disabled.";
        public const string EntrySkipped = "{0} {1} entry was skipped.";
        public const string EntriesSkipped = "{0} {1} entries were skipped.";
        public const string TypeSkipped = "{0} '{1}' is disabled in the editor.";
        public const string SectionPropError = "{0} section [{1}], entry '{2}' cannot be parsed: {3}";

        // Related to settings from .config file
        public const string ConsultManual = "For more information, please consult the manual's explanation of the \"{0}\" setting.";
        public const string SettingNoAirUnits = "DisableAirUnits";
        public const string SettingNoWallBuildings = "OverlayWallsOnly";
        public const string SettingNoSquishMark = "DisableSquishMark";
        public const string SettingBadCraters = "ConvertCraters";
        public const string SettingNoOwnedObjSole = "NoOwnedObjectsInSole";

        // Infantry cell and sub cell combining
        public const string ParseInfantryCellSubPos = "{0}, sub-position {1}";
        // Token checks
        public const string ParseTokensBadNr = "{0} entry '{1}', value \"{2}\", has wrong number of tokens (has {3}, expecting {4}); skipping.";
        // Type checks
        public const string ParseTypeUnknown = "{0} entry '{1}' references unknown {2} '{3}'; skipping.";
        public const string ParseTypeUnknownCell = "{0} entry '{1}' on cell {2} references unknown {3} '{4}'; skipping.";
        // Theater checks
        public const string ParseTheaterBad = "{0} entry '{1}': {2} type {3} is not available in the set theater; skipping.";
        public const string ParseTheaterBadCell = "{0} entry '{1}': {2} type {3} on cell {4} is not available in the set theater; skipping.";
        // Strength checks
        public const string ParseStrengthBad = "{0} entry '{1}': {2} has Strength value '{3}' which cannot be parsed as number; skipping.";
        public const string ParseStrengthIllegal = "{0} entry '{1}': {2} on cell {3} has illegal Strength value {4}; corrected to {5}.";
        // Location checks
        public const string ParseCellBad = "{0} entry '{1}': {2} has Cell value '{3}' which cannot be parsed as number; skipping.";
        public const string ParseCellIllegal = "{0} entry '{1}': {2} has Cell value {3} which is not inside the map; skipping.";
        public const string ParseCellKeyBad = "{0} entry key '{1}' (value \"{2}\") cannot be parsed as Cell number; skipping.";
        public const string ParseCellKeyIllegal = "{0} entry '{1}' has Cell value '{2}' which is not inside the map; skipping.";
        public const string ParseCoordsBad = "{0} entry '{1}': {2} has Coordinates value '{3}' which cannot be parsed as number; skipping.";
        public const string ParseCoordsIllegal = "{0} entry '{1}': {2} has Coordinates [{3},{4}] which are outside the map; skipping.";
        // Infantry sub-position checks
        public const string ParseSubPosBad = "{0} entry '{1}': {2} on cell {3} has sub-position value '{4}' which cannot be parsed as number; skipping.";
        public const string ParseSubPosIllegal = "{0} entry '{1}': {2} on cell {3} has illegal sub-position value {4}; skipping.";
        // Direction checks
        public const string ParseDirectionBad = "{0} entry '{1}': {2} on cell {3} has Direction value '{4}' which cannot be parsed as number; reverting to 0.";
        public const string ParseDirectionIllegal = "{0} entry '{1}': {2} on cell {3} has Direction value {4}";
        public const string ParseDirectionUnknown = " which cannot be matched to a known value";
        public const string ParseDirectionClosest = "; taking closest match value {0} ({1}).";
        public const string ParseDirectionNotSupported = " and {0} type {1} does not support a Direction; reverting to 0.";
        // House checks
        public const string ParseHouseUnknownObj = "{0} entry '{1}': {2} on cell {3} references unknown house '{4}'; reverting to '{5}'.";
        public const string ParseHouseObsoleteObj = "{0} entry '{1}': {2} on cell {3} references obsolete house '{4}'; substituting with '{5}'.";
        public const string ParseHouseBaseUnknown = "Base section has unknown house '{0}'; reverting to '{1}'.";
        public const string ParseHouseBaseObsolete = "Base section has obsolete house '{0}'; substituting with '{1}'.";
        // Orders checks
        public const string ParseOrdersUnknown = "{0} entry '{1}': {2} on cell {3} references unknown orders '{4}'; changing to '{5}'.";
        public const string ParseOrdersUnsupported = "{0} entry '{1}': {2} on cell {3} references unsupported orders '{4}'; changing to '{5}'.";
        // Blocker checks
        public const string ParseBlocker = "{0} entry '{1}': {2} on cell {3} overlaps {4}; skipping.";
        public const string ParseBlockerMulticell = "{0} entry '{1}': {2} placed on cell {3} overlaps {4} in cell {5}; skipping.";
        public const string ParseBlockerMulticellArg = "{0} {1} placed on cell {2}";
        public const string ParseBlockerUnknown = "unknown techno";
        public const string ParseBlockerInfGroup = "{0} entry '{1}': {2} on cell {3} overlaps {0} {4} at sub-position {5}; skipping.";
        // Trigger checks
        public const string ParseTriggerUnknown = "{0} entry '{1}' links to unknown Trigger '{3}'; clearing Trigger.";
        public const string ParseTriggerIllegal = "{0} entry '{1}' links to Trigger '{2}' which does not contain an Event or Action applicable to {0} entries; clearing Trigger.";
        public const string ParseTriggerUnknownObj = "{0} entry '{1}': {2} on cell {3} links to unknown Trigger '{4}'; clearing Trigger.";
        public const string ParseTriggerIllegalObj = "{0} entry '{1}': {2} on cell {3} links to Trigger '{4}' which does not contain an Event or Action applicable to {0} entries; clearing Trigger.";

        // Base section specific
        public const string ParseBaseCountBad = "{0} \"Count\" value '{0}' cannot be parsed as number.";
        public const string ParseBaseCountExceeded = "{0} entry '{1}' exceeds \"Count\" value {2}; skipping.";
        public const string ParseBaseEntryMissing = "{0} entry '{1}' is missing. Missing entries will cause the game to crash.";
        public const string ParseBaseDuplicate = "{0} entry '{1}': duplicate entry for {2} {3} on cell '{4}'; skipping.";
        // Overlay section specific
        public const string ParseOverlayTopBottom = "{0} entry '{1}': {2} on cell {3}: {0} can not be placed on the first and or last lines of the map; skipping.";
        // Waypoints
        public const string ParseIntKeyBad = "{0} entry '{1}' cannot be parsed as number; skipping.";
        public const string ParseIntKeyPadded = "{0} entry {1} is zero-padded and will never be read by the game; skipping.";
        public const string ParseIntKeyRange = "{0} entry {0} is out of range: expecting between {1} and {2}; skipping";
        // Houses
        public const string ParseEdgeIllegal = "{0} {1} has an unknown edge value '{2}'; reverting to {3}";
        // Triggers
        public const string ParseTypeUnknownDef = "{0} entry {1} references unknown {2} '{3}'; reverting to '{4}'.";
        public const string ParseDataBad = "{0} entry {1}: {2} value '{3}' cannot be parsed as number; defaulting to {4}";
        public const string ParseListIndexIllegal = "{0} entry {1}: {2} value {3} cannot be found in list; defaulting to {4}";

        // Special handling; usually related to config options.
        public const string ParseHandleWallStruct = "{0} entry '{1}': {2} type {3} on cell {4} is a wall type; it will be treated as wall, not as {4}.";
        public const string ParseHandleWallSkip = "{0} entry '{1}': {2} type {3} is a wall type; skipping.";
        public const string ParseHandleWallSkipCell = "{0} entry '{1}': {2} type {3} on cell {4} is a wall type; skipping.";

        public const string ParseHandleCrater = "{0} entry '{1}': {2} on cell {3} does not function correctly in maps. Correcting to '{4}'.";
        public const string ParseHandleExpansion = "Expansion {0} '{1}' encountered, but expansion units are not enabled; enabling expansion units.";
        public const string ParseHandleSoleObjects = "Owned objects in Sole Survivor are disabled.";
        public const string ParseHandleConcPairs = "Added extra concrete cells to fill up pairs on cells: {0}.";


        public static string OverlayTypeDescription(OverlayType ovlt)
        {
            return (ovlt.IsWall ? "Wall" : "Solid overlay") + " " + ovlt.Name.ToUpperInvariant();
        }
    }
}
