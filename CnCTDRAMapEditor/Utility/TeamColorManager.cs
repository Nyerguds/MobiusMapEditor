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
using MobiusEditor.Model;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class TeamColorManager : ITeamColorManager, ITeamColorManager<TeamColor>
    {
        private readonly Dictionary<string, TeamColor> teamColors = new Dictionary<string, TeamColor>();

        private readonly IArchiveManager megafileManager;
        public Color RemapBaseColor => Color.FromArgb(0x00, 0xE0, 0x00);

        public ITeamColor this[string key] => GetItem(key);

        public Color GetBaseColor(string key)
        {
            if (!string.IsNullOrEmpty(key) && teamColors.ContainsKey(key))
            {
                return teamColors[key].BaseColor;
            }
            return RemapBaseColor;
        }

        public TeamColor GetItem(string key) => !string.IsNullOrEmpty(key) && teamColors.ContainsKey(key) ? teamColors[key] : null;

        public void RemoveTeamColor(string col)
        {
            if (col != null && teamColors.ContainsKey(col))
            {
                teamColors.Remove(col);
            }
        }

        public void AddTeamColor(TeamColor col)
        {
            if (col != null && col.Name != null)
            {
                teamColors[col.Name] = col;
            }
        }

        public void RemoveTeamColor(TeamColor col)
        {
            if (col != null && col.Name != null && teamColors.ContainsKey(col.Name))
            {
                teamColors.Remove(col.Name);
            }
        }

        public TeamColorManager(IArchiveManager megafileManager)
        {
            this.megafileManager = megafileManager;
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            teamColors.Clear();
        }

        public void Load(string path)
        {
            XmlDocument xmlDoc = null;
            using (Stream archivedFile = megafileManager.OpenFile(path))
            {
                if (archivedFile != null)
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(archivedFile);
                }
            }
            if (xmlDoc != null)
            {
                teamColors.Clear();
                foreach (XmlNode teamColorNode in xmlDoc.SelectNodes("/*/TeamColorTypeClass"))
                {
                    var teamColor = new TeamColor(this);
                    teamColor.Load(teamColorNode.OuterXml);
                    teamColors[teamColor.Name] = teamColor;
                }
            }
            foreach (var teamColor in TopologicalSortTeamColors())
            {
                teamColor.Flatten();
            }
        }

        private IEnumerable<TeamColor> TopologicalSortTeamColors()
        {
            var nodes = teamColors.Values.ToList();
            HashSet<(TeamColor, TeamColor)> edges = new HashSet<(TeamColor, TeamColor)>();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.Variant))
                {
                    edges.Add((GetItem(node.Variant), node));
                }
            }
            var sorted = new List<TeamColor>();
            var openSet = new HashSet<TeamColor>(nodes.Where(n => edges.All(e => !e.Item2.Equals(n))));
            while (openSet.Count > 0)
            {
                var node = openSet.First();
                openSet.Remove(node);
                sorted.Add(node);
                foreach (var edge in edges.Where(e => e.Item1.Equals(node)).ToArray())
                {
                    var node2 = edge.Item2;
                    edges.Remove(edge);
                    if (edges.All(edge2 => !edge2.Item2.Equals(node2)))
                    {
                        openSet.Add(node2);
                    }
                }
            }
            return (edges.Count == 0) ? sorted : null;
        }
    }
}
