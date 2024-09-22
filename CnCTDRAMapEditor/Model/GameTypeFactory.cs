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
            GameType[] enumTypes = GetGameTypes();
            GameInfo[] types = new GameInfo[enumTypes.Max(gt => (int)gt) + 1];
            foreach (Type gType in gameTypes)
            {
                try
                {
                    GameInfo gameInfo = (GameInfo)Activator.CreateInstance(gType);
                    if (gameInfo != null)
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
