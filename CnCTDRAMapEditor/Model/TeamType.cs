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
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    public class TeamTypeClass : ICloneable, IEquatable<TeamTypeClass>
    {
        public ITechnoType Type { get; set; }

        public byte Count { get; set; }

        public TeamTypeClass Clone()
        {
            return new TeamTypeClass()
            {
                Type = Type,
                Count = Count
            };
        }

        public bool Equals(TeamTypeClass other)
        {
            return ReferenceEquals(this, other)
                || (other != null
                && this.Type == other.Type
                && this.Count == other.Count);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public enum TeamMissionArgType
    {
        None,
        Number,
        Time,
        Waypoint,
        OptionsList,
        Tarcom,
    }

    public class TeamMission : ICloneable, IEquatable<TeamMission>
    {
        public string Mission { get; private set; }
        public TeamMissionArgType ArgType { get; private set; }
        public (int,string)[] DropdownOptions { get; private set; }

        public TeamMission(String mission, TeamMissionArgType argType, params (int, string)[] dropdownOptions)
        {
            this.Mission = mission;
            this.ArgType = argType;
            if (dropdownOptions == null)
            {
                this.DropdownOptions = new (int, string)[0];
            }
            else
            {
                this.DropdownOptions = dropdownOptions.ToArray();
            }
        }

        public Boolean Equals(TeamMission other)
        {
            return Mission == other.Mission;
        }

        public TeamMission Clone()
        {
            return new TeamMission(this.Mission, this.ArgType, this.DropdownOptions.Select( ddo => (ddo.Item1, ddo.Item2)).ToArray());
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class TeamTypeMission : ICloneable, IEquatable<TeamTypeMission>
    {
        public TeamMission Mission { get; set; }

        public int Argument { get; set; }

        public TeamTypeMission Clone()
        {
            return new TeamTypeMission()
            {
                Mission = Mission,
                Argument = Argument
            };
        }

        public bool Equals(TeamTypeMission other)
        {
            return ReferenceEquals(this, other)
                || (other != null
                && this.Mission == other.Mission
                && this.Argument == other.Argument);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class TeamType : INamedType, ICloneable
    {
        public static readonly string None = "None";

        public string Name { get; set; }

        public HouseType House { get; set; }

        public bool IsRoundAbout { get; set; }

        public bool IsLearning { get; set; }

        public bool IsSuicide { get; set; }

        public bool IsAutocreate { get; set; }

        public bool IsMercenary { get; set; }

        public int RecruitPriority { get; set; }

        public byte MaxAllowed { get; set; }

        public byte InitNum { get; set; }

        public byte Fear { get; set; }

        public bool IsReinforcable { get; set; }

        public bool IsPrebuilt { get; set; }

        public int Origin { get; set; }

        public string Trigger { get; set; } = Model.Trigger.None;

        public List<TeamTypeClass> Classes { get; } = new List<TeamTypeClass>();

        public List<TeamTypeMission> Missions { get; } = new List<TeamTypeMission>();

        public TeamType Clone()
        {
            var teamType = new TeamType()
            {
                Name = Name,
                House = House,
                IsRoundAbout = IsRoundAbout,
                IsLearning = IsLearning,
                IsSuicide = IsSuicide,
                IsAutocreate = IsAutocreate,
                IsMercenary = IsMercenary,
                RecruitPriority = RecruitPriority,
                MaxAllowed = MaxAllowed,
                InitNum = InitNum,
                Fear = Fear,
                IsReinforcable = IsReinforcable,
                IsPrebuilt = IsPrebuilt,
                Origin = Origin,
                Trigger = Trigger
            };

            teamType.Classes.AddRange(Classes.Select(c => c.Clone()));
            teamType.Missions.AddRange(Missions.Select(m => m.Clone()));

            return teamType;
        }

        public Boolean IsEmpty() {
            // true if nothing is filled in besides a default selected house.
            return
                IsRoundAbout == false
                && IsLearning == false
                && IsSuicide == false
                && IsAutocreate == false
                && IsMercenary == false
                && RecruitPriority == 0
                && MaxAllowed == 0
                && InitNum == 0
                && Fear == 0
                && IsReinforcable == false
                && IsPrebuilt == false
                && Origin == 0
                && Trigger == null
                && Classes.Count == 0
                && Missions.Count == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is TeamType)
            {
                return this == obj;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public bool EqualsOther(TeamType other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (this.Name == other.Name
                && this.House == other.House
                && this.IsRoundAbout == other.IsRoundAbout
                && this.IsLearning == other.IsLearning
                && this.IsSuicide == other.IsSuicide
                && this.IsAutocreate == other.IsAutocreate
                && this.IsMercenary == other.IsMercenary
                && this.RecruitPriority == other.RecruitPriority
                && this.MaxAllowed == other.MaxAllowed
                && this.InitNum == other.InitNum
                && this.Fear == other.Fear
                && this.IsReinforcable == other.IsReinforcable
                && this.IsPrebuilt == other.IsPrebuilt
                && this.Origin == other.Origin
                && this.Trigger == other.Trigger
                && ((this.Classes == null && other.Classes== null) || this.Classes.Count == other.Classes.Count)
                && ((this.Missions == null && other.Missions == null) || this.Missions.Count == other.Missions.Count))
            {
                if (this.Classes != null)
                {
                    for (int i = 0; i < this.Classes.Count; ++i)
                    {
                        if (!this.Classes[i].Equals(other.Classes[i]))
                            return false;
                    }
                }
                if (this.Missions != null)
                {
                    for (int i = 0; i < this.Missions.Count; ++i)
                    {
                        if (!this.Missions[i].Equals(other.Missions[i]))
                            return false;
                    }
                }
                return true;
            }
            return false;
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
    }
}
