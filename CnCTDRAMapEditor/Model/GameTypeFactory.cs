using MobiusEditor.Interface;
using MobiusEditor.RedAlert;
using MobiusEditor.SoleSurvivor;
using MobiusEditor.TiberianDawn;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    public abstract class GameInfo
    {
        public abstract GameType GameType { get; }
        public abstract string Name { get; }
        public abstract string ModFolder { get; }
        public abstract string ModIdentifier { get; }
        public abstract string ModsToLoad { get; }
        public abstract string ModsToLoadSetting { get; }
        public abstract string WorkshopTypeId { get; }
        public abstract string ClassicFolder { get; }
        public abstract string ClassicFolderRemaster { get; }
        public abstract string ClassicFolderDefault { get; }
        public abstract string ClassicFolderSetting { get; }
        public abstract string ClassicStringsFile { get; }
        public abstract TheaterType[] AllTheaters { get; }
        public abstract TheaterType[] AvailableTheaters { get; }
        public abstract bool MegamapSupport { get; }
        public abstract bool MegamapOptional { get; }
        public abstract bool MegamapDefault { get; }
        public abstract bool MegamapOfficial { get; }
        public abstract bool HasSinglePlayer { get; }
        public abstract int MaxTriggers { get; }
        public abstract int MaxTeams { get; }
        public abstract int HitPointsGreenMinimum { get; }
        public abstract int HitPointsYellowMinimum { get; }
        public abstract IGamePlugin CreatePlugin(bool mapImage, bool megaMap);
        public abstract void InitializePlugin(IGamePlugin plugin);
        public abstract void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster);
        public abstract string GetClassicOpposingPlayer(string player);
        public abstract bool SupportsMapLayer(MapLayerFlag mlf);

        public override string ToString()
        {
            return this.Name;
        }

    }

    public enum GameType
    {
        None = -1,
        TiberianDawn = 0,
        RedAlert = 1,
        SoleSurvivor = 2
    }

    public static class GameTypeFactory
    {
        private static Type[] gameTypes = new[]
        {
            typeof(GameInfoTibDawn),
            typeof(GameInfoRedAlert),
            typeof(GameInfoSole)
        };

        public static GameType[] GetGameTypes()
        {
            return Enum.GetValues(typeof(GameType)).Cast<GameType>().Where(gt => (int)gt >= 0).OrderBy(gt => (int)gt).ToArray();
        }

        public static GameInfo[] GetGameInfos()
        {
            GameType[] enumTypes = GetGameTypes();
            GameInfo[] types = new GameInfo[enumTypes.Max(gt => (int)gt) + 1];
            foreach (Type gType in gameTypes)
            {
                try
                {
                    GameInfo gameTypeObj = (GameInfo)Activator.CreateInstance(gType);
                    if (gameTypeObj != null)
                    {
                        types[(int)gameTypeObj.GameType] = gameTypeObj;
                    }
                }
                catch { /* ignore */ }
            }
            return types.ToArray();
        }

        public static Dictionary<GameType, GameInfo> GetGameInfosByType()
        {
            GameInfo[] types = GetGameInfos();
            return types.ToDictionary(tp => tp.GameType, tp => tp);
        }

        public static GameInfo GetGameInfo(GameType gameType)
        {
            foreach (Type gType in gameTypes)
            {
                GameInfo gameTypeObj;
                try
                {
                    gameTypeObj = (GameInfo)Activator.CreateInstance(gType);
                    if (gameTypeObj != null && gameTypeObj.GameType == gameType)
                    {
                        return gameTypeObj;
                    }
                }
                catch { /* ignore */ }
            }
            return null;
        }
    }
}
