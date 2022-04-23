﻿using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky.Builds
{
    public class ProxyData
    {
        public Dictionary<UnitTypes, int> DesiredAddOnCounts;

        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsCounts;

        public Dictionary<UnitTypes, int> DesiredMorphCounts;

        public Dictionary<UnitTypes, int> DesiredProductionCounts;

        public int DesiredPylons;

        public Dictionary<UnitTypes, int> DesiredTechCounts;

        public ProxyData(Point2D location, MacroData macroData, bool enabled = false)
        {
            Location = location;
            Enabled = true;
            MaximumBuildingDistance = 15;
            DesiredPylons = 0;

            DesiredMorphCounts = new Dictionary<UnitTypes, int>();
            foreach (var productionType in macroData.Morphs)
            {
                DesiredMorphCounts[productionType] = 0;
            }

            DesiredProductionCounts = new Dictionary<UnitTypes, int>();
            foreach (var productionType in macroData.Production)
            {
                DesiredProductionCounts[productionType] = 0;
            }

            DesiredTechCounts = new Dictionary<UnitTypes, int>();
            foreach (var techType in macroData.Tech)
            {
                DesiredTechCounts[techType] = 0;
            }

            DesiredAddOnCounts = new Dictionary<UnitTypes, int>();
            foreach (var techType in macroData.AddOns)
            {
                DesiredAddOnCounts[techType] = 0;
            }

            DesiredDefensiveBuildingsCounts = new Dictionary<UnitTypes, int>();
            foreach (var defensiveBuildingsType in macroData.DefensiveBuildings)
            {
                DesiredDefensiveBuildingsCounts[defensiveBuildingsType] = 0;
            }

            Enabled = enabled;
        }

        public bool Enabled { get; set; }
        public Point2D Location { get; set; }
        public float MaximumBuildingDistance { get; set; }
    }
}