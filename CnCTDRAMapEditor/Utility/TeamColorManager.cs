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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class TeamColorManager
    {
        private readonly Dictionary<string, TeamColor> teamColors = new Dictionary<string, TeamColor>();

        private readonly MegafileManager megafileManager;
        private string[] expandModPaths = null;

        public TeamColor this[string key] => !string.IsNullOrEmpty(key) ? teamColors[key] : null;

        public TeamColorManager(MegafileManager megafileManager, string[] expandModPaths)
        {
            this.megafileManager = megafileManager;
            this.expandModPaths = expandModPaths;
        }

        public void Reset()
        {
            teamColors.Clear();
        }

        public void Load(string xmlPath)
        {
            XmlDocument xmlDoc = null;
            if (expandModPaths != null && expandModPaths.Length > 0)
            {
                for (int i = 0; i < expandModPaths.Length; ++i)
                {
                    string modXmlPath = Path.Combine(expandModPaths[i], xmlPath);
                    if (modXmlPath != null && File.Exists(modXmlPath))
                    {
                        xmlDoc = new XmlDocument();
                        xmlDoc.Load(modXmlPath);
                        break;
                    }
                }
            }
            if (xmlDoc == null)
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(megafileManager.Open(xmlPath));
            }
            // Add default black for unowned.
            var teamColorNone = new TeamColor(this);
            teamColorNone.Load("NONE", "BASE_TEAM",
                Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                new Vector3(0.30f, -1.00f, 0.00f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 0.1f),
                new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(61, 61, 59));
            teamColors[teamColorNone.Name] = teamColorNone;
            foreach (XmlNode teamColorNode in xmlDoc.SelectNodes("/*/TeamColorTypeClass"))
            {
                var teamColor = new TeamColor(this);
                teamColor.Load(teamColorNode.OuterXml);
                teamColors[teamColor.Name] = teamColor;
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
                    edges.Add((this[node.Variant], node));
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
