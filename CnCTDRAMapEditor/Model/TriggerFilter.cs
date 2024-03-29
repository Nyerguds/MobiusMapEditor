﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
        private GameType gameType;
        private Map map;

        public bool FilterHouse { get; set; }
        public bool FilterPersistenceType { get; set; }
        public bool FilterEventControl { get; set; }
        public bool FilterEventType { get; set; }
        public bool FilterActionType { get; set; }
        public bool FilterTeamType { get; set; }
        public bool FilterWaypoint { get; set; }
        public bool FilterGlobal { get; set; }
        public bool FilterTrigger { get; set; }
        public string House { get; set; }
        public TriggerPersistentType PersistenceType { get; set; } = TriggerPersistentType.Volatile;
        public TriggerMultiStyleType EventControl { get; set; } = TriggerMultiStyleType.Only;
        public string EventType { get; set; } = TriggerEvent.None;
        public string ActionType { get; set; } = TriggerAction.None;
        public string TeamType { get; set; } = Model.TeamType.None;
        public string Trigger { get; set; } = Model.Trigger.None;
        public int Waypoint { get; set; }
        public int Global { get; set; }

        public bool IsEmpty
        {
            get
            {
                return !this.FilterHouse &&
                       !this.FilterPersistenceType &&
                       !this.FilterEventControl &&
                       !this.FilterEventType &&
                       !this.FilterActionType &&
                       !this.FilterTeamType &&
                       !this.FilterTrigger &&
                       !this.FilterWaypoint &&
                       !this.FilterGlobal;
            }
        }

        public TriggerFilter(IGamePlugin plugin)
        {
            this.gameType = plugin.GameInfo.GameType;
            this.map = plugin.Map;
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
                House house = map.Houses.Where(h => String.Equals(h.Type.Name, this.House, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
            if (isRA && FilterEventControl)
            {
                bool filterMatch = trigger.EventControl == EventControl;
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
                    if (IsTeamMatch(trigger.Action1.Team, TeamType))
                        filterMatch = true;
                }
                else
                {
                    if (IsRATeamEvent(trigger.Event1.EventType) && IsTeamMatch(trigger.Event1.Team, this.TeamType))
                        filterMatch = true;
                    if (trigger.EventControl != TriggerMultiStyleType.Only && IsRATeamEvent(trigger.Event2.EventType) && IsTeamMatch(trigger.Event2.Team, this.TeamType))
                        filterMatch = true;
                    if (IsRATeamAction(trigger.Action1.ActionType) && IsTeamMatch(trigger.Action1.Team, this.TeamType))
                        filterMatch = true;
                    if (IsRATeamAction(trigger.Action2.ActionType) && IsTeamMatch(trigger.Action2.Team, this.TeamType))
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
                    if (String.Equals(trigger.Name, Trigger, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsTDTriggerAction(trigger.Action1.ActionType, out string affectedTrigger)
                        && String.Equals(affectedTrigger, Trigger, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (!filterMatch)
                    {
                        return false;
                    }
                }
                else
                {
                    bool filterMatch = false;
                    if (String.Equals(trigger.Name, Trigger, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsRATriggerAction(trigger.Action1.ActionType)
                        && String.Equals(trigger.Action1.Trigger, Trigger, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (IsRATriggerAction(trigger.Action2.ActionType)
                        && String.Equals(trigger.Action2.Trigger, Trigger, StringComparison.InvariantCultureIgnoreCase))
                        filterMatch = true;
                    if (!filterMatch)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private Boolean IsTeamMatch(String trigTeam, String filterTeam)
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
                    return !Globals.Ignore106Scripting;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_VVVV:
                    affectedTrigger = "VVVV";
                    return !Globals.Ignore106Scripting;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_WWWW:
                    affectedTrigger = "WWWW";
                    return !Globals.Ignore106Scripting;
            }
            return false;
        }

        private bool IsRAHouseEvent(String eventType)
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

        private Boolean IsRAHouseAction(String actionType)
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

        private Boolean IsRATeamEvent(String eventType)
        {
            return eventType == RedAlert.EventTypes.TEVENT_LEAVES_MAP;
        }

        private Boolean IsRATeamAction(String actionType)
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
        private Boolean IsRAGlobalEvent(String eventType)
        {
            switch (eventType)
            {
                case RedAlert.EventTypes.TEVENT_GLOBAL_SET:
                case RedAlert.EventTypes.TEVENT_GLOBAL_CLEAR:
                    return true;
            }
            return false;
        }

        private Boolean IsRAGlobalAction(String actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_SET_GLOBAL :
                case RedAlert.ActionTypes.TACTION_CLEAR_GLOBAL:
                    return true;
            }
            return false;
        }

        private Boolean IsRAWaypointAction(String actionType)
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

        private Boolean IsRATriggerAction(String actionType)
        {
            switch (actionType)
            {
                case RedAlert.ActionTypes.TACTION_FORCE_TRIGGER:
                case RedAlert.ActionTypes.TACTION_DESTROY_TRIGGER:
                    return true;
            }
            return false;
        }

        public override String ToString()
        {
            return ToString('P', null, null);
        }

        public String ToString(char persistenceLabel, string[] persistenceNames, string[] eventControlNames)
        {
            List<string> sb = new List<string>();
            if (this.FilterHouse)
            {
                sb.Add("Hh:" + this.House);
            }
            if (this.FilterPersistenceType)
            {
                sb.Add(persistenceLabel + ":" + (persistenceNames == null ? this.PersistenceType.ToString() : persistenceNames[(int)this.PersistenceType]));
            }
            if (this.FilterEventControl)
            {
                sb.Add("Ex:" + (eventControlNames == null ? this.EventControl.ToString() : eventControlNames[(int)this.EventControl]));
            }
            if (this.FilterEventType)
            {
                sb.Add("Ev:" + this.EventType);
            }
            if (this.FilterActionType)
            {
                sb.Add("Ac:" + this.ActionType);
            }
            if (this.FilterTeamType)
            {
                sb.Add("Tm:" + this.TeamType);
            }
            if (this.FilterTrigger)
            {
                sb.Add("Tr:" + this.Trigger);
            }
            if (this.FilterWaypoint)
            {
                sb.Add("Wp:" + this.Waypoint);
            }
            if (this.FilterGlobal)
            {
                sb.Add("Gl:" + this.Global);
            }
            return String.Join(", ", sb.ToArray());

        }

    }
}
