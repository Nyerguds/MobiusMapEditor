//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;

namespace MobiusEditor.Model
{
    public enum TriggerPersistentType
    {
        Volatile = 0,
        SemiPersistent = 1,
        Persistent = 2
    }

    public enum TriggerMultiStyleType
    {
        Only = 0,
        And = 1,
        Or = 2,
        Linked = 3
    }

    public class TriggerEvent : ICloneable, IEquatable<TriggerEvent>
    {
        public static readonly string None = "None";

        public string EventType { get; set; } = None;

        public string Team { get; set; }

        public long Data { get; set; }

        public TriggerEvent Clone()
        {
            return new TriggerEvent()
            {
                EventType = EventType,
                Team = Team,
                Data = Data
            };
        }
        public void FillDataFrom(TriggerEvent other)
        {
            Team = other.Team;
            Data = other.Data;
        }

        public bool Equals(TriggerEvent other)
        {
            return
                this.EventType.EqualsOrDefaultIgnoreCase(other.EventType, None)
                && this.Team.EqualsOrDefaultIgnoreCase(other.Team, Trigger.None)
                && this.Team == other.Team
                && this.Data == other.Data;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class TriggerAction : ICloneable, IEquatable<TriggerAction>
    {
        public static readonly string None = "None";

        public string ActionType { get; set; } = None;

        public string Trigger { get; set; }

        public string Team { get; set; }

        public long Data { get; set; }

        public TriggerAction Clone()
        {
            return new TriggerAction()
            {
                ActionType = ActionType,
                Trigger = Trigger,
                Team = Team,
                Data = Data
            };
        }
        public void FillDataFrom(TriggerAction other)
        {
            Trigger = Trigger;
            Team = other.Team;
            Data = other.Data;
        }

        public bool Equals(TriggerAction other)
        {
            return
                this.ActionType.EqualsOrDefaultIgnoreCase(other.ActionType, None)
                && this.Trigger.EqualsOrDefaultIgnoreCase(other.Trigger, Model.Trigger.None)
                && this.Team == other.Team
                && this.Data == other.Data;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class Trigger : INamedType, ICloneable, IEquatable<Trigger>
    {
        public static readonly string None = "None";

        public string Name { get; set; }

        public TriggerPersistentType PersistentType { get; set; } = TriggerPersistentType.Volatile;

        public string House { get; set; }

        public TriggerMultiStyleType EventControl { get; set; } = TriggerMultiStyleType.Only;

        public TriggerEvent Event1 { get; private set; } = new TriggerEvent { EventType = TriggerEvent.None };

        public TriggerEvent Event2 { get; private set; } = new TriggerEvent { EventType = TriggerEvent.None };

        public TriggerAction Action1 { get; private set; } = new TriggerAction { ActionType = TriggerAction.None };

        public TriggerAction Action2 { get; private set; } = new TriggerAction { ActionType = TriggerAction.None };

        public Trigger Clone()
        {
            return new Trigger()
            {
                Name = Name,
                PersistentType = PersistentType,
                House = House,
                EventControl = EventControl,
                Event1 = Event1.Clone(),
                Event2 = Event2.Clone(),
                Action1 = Action1.Clone(),
                Action2 = Action2.Clone()
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Trigger trig)
            {
                return this.Equals(trig);
            }
            else if (obj is string str)
            {
                return string.Equals(Name, str, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(obj);
        }

        public Boolean Equals(Trigger other)
        {
            return ReferenceEquals(this, other)
                || (other != null
                && this.Name == other.Name
                && this.PersistentType == other.PersistentType
                && this.House.EqualsOrDefaultIgnoreCase(other.House, Model.House.None)
                && this.EventControl == other.EventControl
                && this.Event1.Equals(other.Event1)
                && this.Event2.Equals(other.Event2)
                && this.Action1.Equals(other.Action1)
                && this.Action2.Equals(other.Action2));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }


        public static Boolean CheckForChanges(List<Trigger> list1, List<Trigger> list2)
        {
            // Might need to migrate this to the map.
            if (list1.Count != list2.Count)
                return true;
            HashSet<string> found = new HashSet<string>();
            foreach (Trigger trig in list1)
            {
                Trigger oldTrig = list2.Find(t => t.Name.Equals(trig.Name));
                if (oldTrig == null)
                {
                    return true;
                }
                found.Add(trig.Name);
                if (!trig.Equals(oldTrig))
                {
                    return true;
                }
            }
            foreach (Trigger trig in list2)
            {
                if (!found.Contains(trig.Name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
