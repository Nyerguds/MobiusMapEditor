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
using MobiusEditor.RedAlert;
using MobiusEditor.SoleSurvivor;
using MobiusEditor.TiberianDawn;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
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
            return GetGameInfos(false);
        }

        public static GameInfo[] GetAvailableGameInfosOrdered()
        {
            GameInfo[] types = GetGameInfos(false);
            Dictionary<string, int> order = Globals.EnabledGamesOrder;
            return types.Where(gi => gi != null).OrderBy(gi => order[gi.ShortName]).ToArray();
        }

        public static GameInfo[] GetGameInfos(bool unfiltered)
        {
            HashSet<string> enabledGames = unfiltered ? null : Globals.EnabledGames;
            GameType[] enumTypes = GetGameTypes();
            GameInfo[] types = new GameInfo[enumTypes.Max(gt => (int)gt) + 1];
            foreach (Type gType in gameTypes)
            {
                try
                {
                    GameInfo gameInfo = (GameInfo)Activator.CreateInstance(gType);
                    if (gameInfo != null && (enabledGames == null || enabledGames.Contains(gameInfo.ShortName)))
                    {
                        types[(int)gameInfo.GameType] = gameInfo;
                    }
                }
                catch { /* ignore */ }
            }
            return types.ToArray();
        }

        public static Dictionary<GameType, GameInfo> GetGameInfosByType()
        {
            return GetGameInfosByType(false);
        }

        public static Dictionary<GameType, GameInfo> GetGameInfosByType(bool unfiltered)
        {
            GameInfo[] types = GetGameInfos(unfiltered);
            return types.Where(tp => tp != null).ToDictionary(tp => tp.GameType, tp => tp);
        }
    }
}
