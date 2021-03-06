using SC2APIProtocol;
using Sharky.Builds;
using System.Collections.Generic;

namespace Sharky.Macro
{
    public class TechBuilder
    {
        private IBuildingBuilder BuildingBuilder;
        private BuildOptions BuildOptions;
        private MacroData MacroData;
        private SharkyUnitData SharkyUnitData;
        private bool SkipTech;

        public TechBuilder(Sharky.LokiBot.BaseLokiBot lokiBot, IBuildingBuilder buildingBuilder)
        {
            MacroData = lokiBot.MacroData;
            SharkyUnitData = lokiBot.SharkyUnitData;
            BuildOptions = lokiBot.BuildOptions;

            BuildingBuilder = buildingBuilder;
        }

        public List<Action> BuildTechBuildings()
        {
            var commands = new List<Action>();
            if (SkipTech)
            {
                SkipTech = false;
                return commands;
            }
            var begin = System.DateTime.UtcNow;

            foreach (var unit in MacroData.BuildTech)
            {
                if (unit.Value)
                {
                    var unitData = SharkyUnitData.BuildingData[unit.Key];
                    var command = BuildingBuilder.BuildBuilding(MacroData, unit.Key, unitData, wallOffType: BuildOptions.WallOffType);
                    if (command != null)
                    {
                        commands.AddRange(command);
                        return commands;
                    }
                }
            }

            var endTime = (System.DateTime.UtcNow - begin).TotalMilliseconds;
            if (endTime > 1)
            {
                SkipTech = true;
            }

            return commands;
        }
    }
}