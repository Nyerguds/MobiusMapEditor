//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MobiusEditor.Model
{
    [DebuggerDisplay("{Type}: {Count}")]
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
        MapCell,
        MissionNumber,
        GlobalNumber,
        Tarcom,
    }

    [DebuggerDisplay("{Mission} ({ArgType})")]
    public class TeamMission : ICloneable, IEquatable<TeamMission>
    {
        public int ID { get; private set; }
        public string Mission { get; private set; }
        public TeamMissionArgType ArgType { get; private set; }
        public string Tooltip { get; private set; }
        public (int Value, string Label)[] DropdownOptions { get; private set; }

        public TeamMission(int id, string mission, TeamMissionArgType argType, string tooltip, params (int, string)[] dropdownOptions)
        {
            this.ID = id;
            this.Mission = mission;
            this.Tooltip = String.IsNullOrEmpty(tooltip) ? null : tooltip;
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

        public TeamMission(int id, string mission, TeamMissionArgType argType, params (int, string)[] dropdownOptions)
            : this(id, mission, argType, null, dropdownOptions)
        {
        }

        public bool Equals(TeamMission other)
        {
            return Mission == other.Mission;
        }

        public TeamMission Clone()
        {
            return new TeamMission(this.ID, this.Mission, this.ArgType, this.Tooltip, this.DropdownOptions.Select(ddo => (ddo.Value, ddo.Label)).ToArray());
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

        public string GetFormattedArgument()
        {
            if (Mission != null && Mission.ArgType == TeamMissionArgType.OptionsList && Mission.DropdownOptions != null)
            {
                IEnumerable<(int Value ,string Label)> options = Mission.DropdownOptions.Where(ddo => ddo.Value == Argument);
                if (options.Count() > 0)
                {
                    return options.First().Label;
                }
            }
            return Argument.ToString();
        }
    }

    [DebuggerDisplay("{Name}: {House}, {Classes}, {Missions}")]
    public class TeamType : INamedType, ICloneable
    {
        public static readonly string None = "None";

        public static bool IsEmpty(string teamtype)
        {
            return teamtype == null || teamtype.Equals(None, StringComparison.OrdinalIgnoreCase);
        }

        public static TeamType GetTeamType(string name, Map map)
        {
            if (String.IsNullOrEmpty(name) || None.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return map?.TeamTypes?.FirstOrDefault(t => name.Equals(t.Name, StringComparison.OrdinalIgnoreCase));
        }

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

        public int Origin { get; set; } = -1;

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

        public bool IsEmpty() {
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
                && Origin == -1
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
            else if (obj is string str)
            {
                return String.Equals(Name, str, StringComparison.OrdinalIgnoreCase);
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

        public string GetSummaryLabel(bool withLineBreaks)
        {
            string[] classes = Classes.Where(cl => cl.Count > 0).Select(cl => String.Format("{0}: {1}", cl.Type.Name, cl.Count)).ToArray();
            string[] missions = Missions.Select(ms => String.Format("{0}: {1}", ms.Mission.Mission?.TrimEnd('.'), ms.GetFormattedArgument())).ToArray();
            if (!withLineBreaks)
            {
                return House.Name
                    + ": " + (classes.Length == 0 ? "<none>" : String.Join(", ", classes))
                    + " → " + (missions.Length == 0 ? "<none>" : String.Join(", ", missions));
            }
            const int BREAKLEN = 50;
            List<string> lines = new List<string>();
            StringBuilder line = new StringBuilder(House.Name);
            line.Append(": ");
            if (classes.Length == 0)
            {
                line.Append("<none>");
            }
            for (int i = 0; i < classes.Length; ++i)
            {
                if (line.Length > BREAKLEN)
                {
                    line.Append(",");
                    lines.Add(line.ToString());
                    line.Clear();
                    line.Append(" ");
                }
                else if (i > 0)
                {
                    line.Append(", ");
                }
                line.Append(classes[i]);
            }
            if (line.Length > 0)
            {
                lines.Add(line.ToString());
                line.Clear();
            }
            line.Append(" → ");
            if (missions.Length == 0)
            {
                line.Append("<none>");
            }
            for (int i = 0; i < missions.Length; ++i)
            {
                if (line.Length > BREAKLEN)
                {
                    line.Append(",");
                    lines.Add(line.ToString());
                    line.Clear();
                    line.Append("   ");
                }
                else if (i > 0)
                {
                    line.Append(", ");
                }
                line.Append(missions[i]);
            }
            if (line.Length > 0)
            {
                lines.Add(line.ToString());
                line.Clear();
            }
            return String.Join("\n", lines.ToArray());
        }

    }
}
