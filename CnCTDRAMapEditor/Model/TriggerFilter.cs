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
using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    public class TriggerFilter
    {
        private readonly string[] persistenceNames = Trigger.PersistenceNamesShort.ToArray();
        private readonly string[] multiStyleNames = Trigger.MultiStyleNamesShort.ToArray();

        private readonly GameType gameType;
        private readonly Map map;

        public bool FilterHouse { get; set; }
        public bool FilterPersistenceType { get; set; }
        public bool FilterMultiStyle { get; set; }
        public bool FilterEventType { get; set; }
        public bool FilterActionType { get; set; }
        public bool FilterTeamType { get; set; }
        public bool FilterWaypoint { get; set; }
        public bool FilterGlobal { get; set; }
        public bool FilterTrigger { get; set; }
        public string House { get; set; }
        public TriggerPersistentType PersistenceType { get; set; } = TriggerPersistentType.Volatile;
        public TriggerMultiStyleType MultiStyle { get; set; } = TriggerMultiStyleType.Only;
        public string EventType { get; set; } = TriggerEvent.None;
        public string ActionType { get; set; } = TriggerAction.None;
        public string TeamTypeArg { get; set; } = TeamType.None;
        public string TriggerArg { get; set; } = Trigger.None;
        public int Waypoint { get; set; }
        public int Global { get; set; }

        public bool IsEmpty
        {
            get
            {
                return !FilterHouse &&
                       !FilterPersistenceType &&
                       !FilterMultiStyle &&
                       !FilterEventType &&
                       !FilterActionType &&
                       !FilterTeamType &&
                       !FilterTrigger &&
                       !FilterWaypoint &&
                       !FilterGlobal;
            }
        }

        public TriggerFilter(IGamePlugin plugin)
        {
            gameType = plugin.GameInfo.GameType;
            map = plugin.Map;
        }

        public TriggerFilter(GameType gameType, Map map)
        {
            this.gameType = gameType;
            this.map = map;
        }

        public bool MatchesFilter(Trigger trigger)
        {
            bool isRA = gameType == GameType.RedAlert;
            if (FilterHouse)
            {
                House house = map.Houses.Where(h => String.Equals(h.Type.Name, House, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                int houseId = house == null ? -1 : house.Type.ID;
                bool filterMatch = trigger.House == House;
                if (isRA && !filterMatch)
                {
                    if (IsRAHouseEvent(trigger.Event1.EventType) && trigger.Event1.Data == houseId)
                        filterMatch = true;
                    if (trigger.EventControl != TriggerMultiStyleType.Only && IsRAHouseEvent(trigger.Event2.EventType) && trigger.Event2.Data == houseId)
                        filterMatch = true;
                    if (IsRAHouseAction(trigger.Action1.ActionType) && trigger.Action1.Data == houseId)
                        filterMatch = true;
                    if (IsRAHouseAction(trigger.Action2.ActionType) && trigger.Action2.Data == houseId)
                        filterMatch = true;
                }
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterPersistenceType)
            {
                bool filterMatch = trigger.PersistentType == PersistenceType;
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (isRA && FilterMultiStyle)
            {
                bool filterMatch = trigger.EventControl == MultiStyle;
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterEventType)
            {
                bool isEmpty = TriggerEvent.IsEmpty(EventType);
                bool filterMatch = trigger.Event1.EventType == EventType || isEmpty && TriggerEvent.IsEmpty(trigger.Event1.EventType)
                    || (isRA && trigger.EventControl != TriggerMultiStyleType.Only &&
                        ((isEmpty && TriggerEvent.IsEmpty(trigger.Event1.EventType)) || trigger.Event2.EventType == EventType));
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterActionType)
            {
                bool isEmpty = TriggerAction.IsEmpty(ActionType);
                bool filterMatch = trigger.Action1.ActionType == ActionType || isEmpty && TriggerAction.IsEmpty(trigger.Action1.ActionType)
                    || (isRA && ((isEmpty && TriggerAction.IsEmpty(trigger.Action2.ActionType)) || trigger.Action2.ActionType == ActionType));
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterTeamType)
            {
                bool filterMatch = false;
                if (!isRA)
                {
                    if (IsTeamMatch(trigger.Action1.Team, TeamTypeArg))
                        filterMatch = true;
                }
                else
                {
                    if (IsRATeamEvent(trigger.Event1.EventType) && IsTeamMatch(trigger.Event1.Team, TeamTypeArg))
                        filterMatch = true;
                    if (trigger.EventControl != TriggerMultiStyleType.Only && IsRATeamEvent(trigger.Event2.EventType) && IsTeamMatch(trigger.Event2.Team, TeamTypeArg))
                        filterMatch = true;
                    if (IsRATeamAction(trigger.Action1.ActionType) && IsTeamMatch(trigger.Action1.Team, TeamTypeArg))
                        filterMatch = true;
                    if (IsRATeamAction(trigger.Action2.ActionType) && IsTeamMatch(trigger.Action2.Team, TeamTypeArg))
                        filterMatch = true;
                }
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterWaypoint && isRA)
            {
                bool filterMatch = false;
                if (IsRAWaypointAction(trigger.Action1.ActionType) && trigger.Action1.Data == Waypoint)
                    filterMatch = true;
                if (IsRAWaypointAction(trigger.Action2.ActionType) && trigger.Action2.Data == Waypoint)
                    filterMatch = true;
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterGlobal && isRA)
            {
                bool filterMatch = false;
                if (IsRAGlobalEvent(trigger.Event1.EventType) && trigger.Event1.Data == Global)
                    filterMatch = true;
                if (trigger.EventControl != TriggerMultiStyleType.Only && IsRAGlobalEvent(trigger.Event2.EventType) && trigger.Event2.Data == Global)
                    filterMatch = true;
                if (IsRAGlobalAction(trigger.Action1.ActionType) && trigger.Action1.Data == Global)
                    filterMatch = true;
                if (IsRAGlobalAction(trigger.Action2.ActionType) && trigger.Action2.Data == Global)
                    filterMatch = true;
                if (!filterMatch)
                {
                    return false;
                }
            }
            if (FilterTrigger)
            {
                if (!isRA)
                {
                    bool filterMatch = false;
                    if (String.Equals(trigger.Name, TriggerArg, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsTDTriggerAction(trigger.Action1.ActionType, out string affectedTrigger)
                        && String.Equals(affectedTrigger, TriggerArg, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (!filterMatch)
                    {
                        return false;
                    }
                }
                else
                {
                    bool filterMatch = false;
                    if (String.Equals(trigger.Name, TriggerArg, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsRATriggerAction(trigger.Action1.ActionType)
                        && String.Equals(trigger.Action1.Trigger, TriggerArg, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsRATriggerAction(trigger.Action2.ActionType)
                        && String.Equals(trigger.Action2.Trigger, TriggerArg, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (!filterMatch)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsTeamMatch(string trigTeam, string filterTeam)
        {
            return (Model.TeamType.IsEmpty(filterTeam) && Model.TeamType.IsEmpty(trigTeam)) || trigTeam == filterTeam;
        }

        private bool IsTDTriggerAction(string actionType, out string affectedTrigger)
        {
            affectedTrigger = null;
            switch (actionType)
            {
                case TiberianDawn.ActionTypes.ACTION_DESTROY_XXXX:
                    affectedTrigger = "XXXX";
                    return true;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_YYYY:
                    affectedTrigger = "YYYY";
                    return true;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_ZZZZ:
                    affectedTrigger = "ZZZZ";
                    return true;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_UUUU:
                    affectedTrigger = "UUUU";
                    return Globals.EnableTd106Scripting;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_VVVV:
                    affectedTrigger = "VVVV";
                    return Globals.EnableTd106Scripting;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_WWWW:
                    affectedTrigger = "WWWW";
                    return Globals.EnableTd106Scripting;
            }
            return false;
        }

        private bool IsRAHouseEvent(string eventType)
        {
            switch (eventType)
            {
                case RedAlert.EventTypes.TEVENT_PLAYER_ENTERED:
                case RedAlert.EventTypes.TEVENT_CROSS_HORIZONTAL:
                case RedAlert.EventTypes.TEVENT_CROSS_VERTICAL:
                case RedAlert.EventTypes.TEVENT_ENTERS_ZONE:
                case RedAlert.EventTypes.TEVENT_LOW_POWER:
                case RedAlert.EventTypes.TEVENT_THIEVED:
                case RedAlert.EventTypes.TEVENT_HOUSE_DISCOVERED:
                case RedAlert.EventTypes.TEVENT_BUILDINGS_DESTROYED:
                case RedAlert.EventTypes.TEVENT_UNITS_DESTROYED:
                case RedAlert.EventTypes.TEVENT_ALL_DESTROYED:
                    return true;
            }
            return false;
        }

        private bool IsRAHouseAction(string actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_WIN:
                case RedAlert.ActionTypes.TACTION_LOSE:
                case RedAlert.ActionTypes.TACTION_BEGIN_PRODUCTION:
                case RedAlert.ActionTypes.TACTION_FIRE_SALE:
                case RedAlert.ActionTypes.TACTION_AUTOCREATE:
                case RedAlert.ActionTypes.TACTION_ALL_HUNT:
                    return true;
            }
            return false;
        }

        private bool IsRATeamEvent(string eventType)
        {
            return eventType == RedAlert.EventTypes.TEVENT_LEAVES_MAP;
        }

        private bool IsRATeamAction(string actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_CREATE_TEAM:
                case RedAlert.ActionTypes.TACTION_DESTROY_TEAM:
                case RedAlert.ActionTypes.TACTION_REINFORCEMENTS:
                    return true;
            }
            return false;
        }
        private bool IsRAGlobalEvent(string eventType)
        {
            switch (eventType)
            {
                case RedAlert.EventTypes.TEVENT_GLOBAL_SET:
                case RedAlert.EventTypes.TEVENT_GLOBAL_CLEAR:
                    return true;
            }
            return false;
        }

        private bool IsRAGlobalAction(string actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_SET_GLOBAL :
                case RedAlert.ActionTypes.TACTION_CLEAR_GLOBAL:
                    return true;
            }
            return false;
        }

        private bool IsRAWaypointAction(string actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_DZ:
                case RedAlert.ActionTypes.TACTION_REVEAL_SOME:
                case RedAlert.ActionTypes.TACTION_REVEAL_ZONE:
                    return true;
            }
            return false;
        }

        private bool IsRATriggerAction(string actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_FORCE_TRIGGER:
                case RedAlert.ActionTypes.TACTION_DESTROY_TRIGGER:
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            List<string> sb = new List<string>();
            if (FilterHouse)
            {
                sb.Add("Hs:" + House);
            }
            if (FilterPersistenceType)
            {
                int persistence = (int)PersistenceType;
                string persistenceName = persistence >= 0 && persistence < persistenceNames.Length ?
                        persistenceNames[persistence] : PersistenceType.ToString();
                sb.Add("Ex:" + persistenceName);
            }
            if (FilterMultiStyle)
            {
                int multiStyle = (int)MultiStyle;
                string multiStyleName = multiStyle >= 0 && multiStyle < multiStyleNames.Length ?
                        multiStyleNames[multiStyle] : MultiStyle.ToString();
                sb.Add("Tp:" + multiStyleName);
            }
            if (FilterEventType)
            {
                sb.Add("Ev:" + TrimArg(EventType));
            }
            if (FilterActionType)
            {
                sb.Add("Ac:" + TrimArg(ActionType));
            }
            if (FilterTeamType)
            {
                sb.Add("Tm:" + TeamTypeArg);
            }
            if (FilterTrigger)
            {
                sb.Add("Tr:" + TriggerArg);
            }
            if (FilterWaypoint)
            {
                sb.Add("Wp:" + Waypoint);
            }
            if (FilterGlobal)
            {
                sb.Add("Gl:" + Global);
            }
            return String.Join(", ", sb.ToArray());
        }

        private String TrimArg(string arg)
        {
            if (String.IsNullOrEmpty(arg))
            {
                return String.Empty;
            }
            int bracketIndex = arg.IndexOf('(');
            if (bracketIndex > 0)
            {
                arg = arg.Substring(0, bracketIndex);
            }
            arg = arg.TrimEnd('.');
            return arg;
        }

    }
}
