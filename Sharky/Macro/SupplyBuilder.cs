using SC2APIProtocol;
using Sharky.Builds;
using System.Collections.Generic;

namespace Sharky.Macro
{
    public class SupplyBuilder
    {
        private IBuildingBuilder BuildingBuilder;
        private BuildOptions BuildOptions;
        private MacroData MacroData;
        private SharkyUnitData SharkyUnitData;
        private bool SkipSupply;

        public SupplyBuilder(Sharky.LokiBot.LokiBot lokiBot, IBuildingBuilder buildingBuilder)
        {
            MacroData = lokiBot.MacroData;
            SharkyUnitData = lokiBot.SharkyUnitData;
            BuildOptions = lokiBot.BuildOptions;

            BuildingBuilder = buildingBuilder;
        }

        public List<Action> BuildSupply()
        {
            var commands = new List<Action>();
            if (SkipSupply)
            {
                SkipSupply = false;
                return commands;
            }

            var begin = System.DateTime.UtcNow;

            if (MacroData.BuildPylon)
            {
                var unitData = SharkyUnitData.BuildingData[UnitTypes.PROTOSS_PYLON];
                var command = BuildingBuilder.BuildBuilding(MacroData, UnitTypes.PROTOSS_PYLON, unitData, wallOffType: BuildOptions.WallOffType);
                if (command != null)
                {
                    commands.AddRange(command);
                    return commands;
                }
            }

            if (MacroData.BuildSupplyDepot)
            {
                var unitData = SharkyUnitData.BuildingData[UnitTypes.TERRAN_SUPPLYDEPOT];
                var command = BuildingBuilder.BuildBuilding(MacroData, UnitTypes.TERRAN_SUPPLYDEPOT, unitData, wallOffType: BuildOptions.WallOffType);
                if (command != null)
                {
                    commands.AddRange(command);
                    return commands;
                }
            }

            if (MacroData.BuildOverlord)
            {
                MacroData.BuildUnits[UnitTypes.ZERG_OVERLORD] = true;
            }

            var endTime = (System.DateTime.UtcNow - begin).TotalMilliseconds;
            if (endTime > 1)
            {
                SkipSupply = true;
            }

            return commands;
        }
    }
}