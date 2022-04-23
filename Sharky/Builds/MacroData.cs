using SC2APIProtocol;
using Sharky.Builds;
using System.Collections.Generic;

namespace Sharky
{
    public class MacroData
    {
        public List<UnitTypes> AddOns;
        public List<UnitTypes> BarracksUnits;
        public Dictionary<UnitTypes, bool> BuildAddOns;
        public Dictionary<UnitTypes, bool> BuildDefensiveBuildings;
        public bool BuildGas;
        public bool BuildOverlord;
        public Dictionary<UnitTypes, bool> BuildProduction;
        public bool BuildPylon;
        public bool BuildSupplyDepot;
        public Dictionary<UnitTypes, bool> BuildTech;
        public Dictionary<UnitTypes, bool> BuildUnits;
        public List<UnitTypes> CommandCenterUnits;
        public List<UnitTypes> DefensiveBuildings;
        public Dictionary<UnitTypes, int> DesiredAddOnCounts;
        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsAtDefensivePoint;
        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsAtEveryBase;
        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsAtEveryMineralLine;
        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsAtNextBase;
        public Dictionary<UnitTypes, int> DesiredDefensiveBuildingsCounts;
        public int DesiredExtraBaseSimCityBatteries;
        public int DesiredExtraBaseSimCityCannons;
        public int DesiredExtraBaseSimCityPylons;
        public int DesiredGases;
        public int DesiredMacroCommandCenters;
        public Dictionary<UnitTypes, int> DesiredMorphCounts;
        public int DesiredOverlords;
        public Dictionary<UnitTypes, int> DesiredProductionCounts;
        public int DesiredPylons;
        public int DesiredPylonsAtDefensivePoint;
        public int DesiredPylonsAtEveryBase;
        public int DesiredPylonsAtEveryMineralLine;
        public int DesiredPylonsAtNextBase;
        public int DesiredSupplyDepots;
        public Dictionary<UnitTypes, int> DesiredTechCounts;
        public Dictionary<UnitTypes, int> DesiredUnitCounts;
        public Dictionary<Upgrades, bool> DesiredUpgrades;
        public List<UnitTypes> FactoryUnits;
        public List<UnitTypes> GatewayUnits;
        public List<UnitTypes> HatcheryUnits;
        public List<UnitTypes> LarvaUnits;
        public Dictionary<UnitTypes, bool> Morph;
        public List<UnitTypes> Morphs;
        public List<UnitTypes> NexusUnits;
        public List<UnitTypes> Production;
        public Race Race;
        public List<UnitTypes> RoboticsFacilityUnits;
        public List<UnitTypes> StargateUnits;
        public List<UnitTypes> StarportUnits;
        public List<UnitTypes> Tech;
        public List<UnitTypes> Units;
        public Dictionary<string, AddOnSwap> AddOnSwaps { get; set; }
        public float DefensiveBuildingMaximumDistance { get; set; }
        public float DefensiveBuildingMineralLineMaximumDistance { get; set; }
        public int FoodArmy { get; set; }
        public int FoodLeft { get; set; }
        public int FoodUsed { get; set; }
        public int Frame { get; set; }
        public int Minerals { get; set; }
        public Dictionary<string, ProxyData> Proxies { get; set; }
        public int VespeneGas { get; set; }
    }
}