//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System.Drawing;
using System.Linq;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.TiberianDawn;
using MobiusEditor.Utility;

namespace MobiusEditor.SoleSurvivor
{
    public class GameInfoSole : GameInfoTibDawn
    {
        public override GameType GameType => GameType.SoleSurvivor;
        public override string ShortName => "SS";
        public override string Name => "Sole Survivor";
        public override string IniName => "SoleSurvivor";
        public override string SteamId => null;
        public override bool PublishedMapsUseMirrorServer => false;
        public override string SteamGameName => null;
        public override string SteamFileExtensionSolo => null;
        public override string SteamFileExtensionMulti => null;
        public override FileType SteamFileType => FileType.None;
        public override string[] SteamDefaultTags => null;
        public override string[] SteamSoloTags => null;
        public override string[] SteamMultiTags => null;
        public override string[] SteamSoloExtraTags => null;
        public override string[] SteamMultiExtraTags => null;
        public override FileTypeInfo[] SupportedFileTypes => new FileTypeInfo[] {
            // BIN is added separately so it gets accepted as valid resave type, but its data is never actually used.
            new FileTypeInfo(FileType.INI, "Sole Surivor map (ini+bin)", new string[] { "ini", "bin" }, new string[] { "ini", "bin" },
                new FileType[] { FileType.INI, FileType.BIN }, new FileType[] { FileType.INI, FileType.BIN }),
            new FileTypeInfo(FileType.BIN, "Sole Surivor map (ini+bin)", FileTypeFlags.HideFromList, new string[] { "bin", "ini" }, new string[] { "bin", "ini" },
                new FileType[] { FileType.BIN, FileType.INI }, new FileType[] { FileType.BIN, FileType.INI })
        };
        public override FileType DefaultSaveTypeFromPgm => FileType.None;
        public override string[] RemasterMegFiles => new string[] { "CONFIG.MEG", "TEXTURES_COMMON_SRGB.MEG", "TEXTURES_SRGB.MEG", "TEXTURES_TD_SRGB.MEG" };
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathSS;
        public override string ClassicFolderDefault => "Classic\\TD\\";
        public override string ClassicFolderSetting => "ClassicPathSS";
        public override string ClassicStringsFile => "conquer.eng";
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapIsSupported => true;
        public override bool MegamapIsOptional => true;
        public override bool MegamapIsDefault => true;
        public override bool MegamapIsOfficial => true;
        public override bool HasSinglePlayer => false;
        public override bool CanUseNewMixFormat => false;
        public override int MaxAircraft => Constants.MaxAircraftClassic;
        public override int MaxVessels => 0;
        public override int MaxBuildings => Constants.MaxBuildingsClassic;
        public override int MaxInfantry => Constants.MaxInfantryClassic;
        public override int MaxTerrain => Constants.MaxTerrainClassic;
        public override int MaxUnits => Constants.MaxUnitsClassic;
        public override int MaxTriggers => Constants.MaxTriggersClassic;
        public override int MaxTeams => Constants.MaxTeamsClassic;
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.FlagPlace;
        public override Bitmap WorkshopPreviewGeneric => null;
        public override Bitmap WorkshopPreviewGenericGame => null;

        public override FileType IdentifyMap(INI iniContents, byte[] binContents, bool contentWasSwapped, out bool isMegaMap, out string theater)
        {
            isMegaMap = true;
            theater = null;
            bool iniMatch = IsCnCIni(iniContents) &&
                GamePluginSS.CheckForSSmap(iniContents) &&
                !RedAlert.GamePluginRA.CheckForRAMap(iniContents);
            if (!iniMatch)
            {
                return FileType.None;
            }
            int maxTemplate = TemplateTypes.GetTypes().Max(tp => tp.ID);
            isMegaMap = GamePluginTD.CheckForMegamap(iniContents);
            if (contentWasSwapped
                && ((isMegaMap && !GamePluginTD.CheckMegaMapFormat(binContents, MapSizeMega, maxTemplate))
                || !isMegaMap && !GamePluginTD.CheckNormalMapFormat(binContents, MapSize, maxTemplate, -1, out _)))
            {
                // Primary read file is just some unsupported file that happens to have the same
                // name as a valid ini file in the same folder. Reject the original loaded file.
                return FileType.None;
            }
            theater = GetTheater(iniContents);
            return FileType.INI;
        }

        public override IGamePlugin CreatePlugin(bool mapImage, bool megaMap) => new GamePluginSS(mapImage, megaMap);

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            MapLayerFlag badLayers = MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius;
            if (Globals.NoOwnedObjectsInSole)
            {
                badLayers |= MapLayerFlag.Buildings | MapLayerFlag.Units | MapLayerFlag.Infantry | MapLayerFlag.BuildingRebuild;
            }
            return mlf == (mlf & ~badLayers);
        }

        public override string EvaluateBriefing(string briefing)
        {
            return null;
        }

        public override string GetSteamWorkshopFileName(IGamePlugin plugin)
        {
            return "-";
        }
    }
}
