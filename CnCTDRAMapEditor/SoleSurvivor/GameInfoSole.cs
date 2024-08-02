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

namespace MobiusEditor.SoleSurvivor
{
    public class GameInfoSole : GameInfoTibDawn
    {
        public override GameType GameType => GameType.SoleSurvivor;
        public override string ShortName => "SS";
        public override string Name => "Sole Survivor";
        public override string IniName => "SoleSurvivor";
        public override string WorkshopTypeId => null;
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
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.FlagPlace;

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

        public override Bitmap GetWaypointIcon()
        {
            return GetTile("beacon", 0, "mouse", 12);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return GetTile("mine", 3, "mine.shp", 3);
        }

        public override string EvaluateBriefing(string briefing)
        {
            return null;
        }
    }
}
