using SC2APIProtocol;
using Sharky.Builds;
using System.Collections.Generic;

namespace Sharky.Macro
{
    public class BuildingMorpher
    {
        private MacroData MacroData;
        private Morpher Morpher;
        private SharkyUnitData SharkyUnitData;

        public BuildingMorpher(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            MacroData = lokiBot.MacroData;
            SharkyUnitData = lokiBot.SharkyUnitData;
            Morpher = lokiBot.Morpher;
        }

        public List<Action> MorphBuildings()
        {
            var commands = new List<Action>();

            foreach (var unit in MacroData.Morph)
            {
                if (unit.Value)
                {
                    var unitData = SharkyUnitData.MorphData[unit.Key];
                    var command = Morpher.MorphBuilding(MacroData, unitData);
                    if (command != null)
                    {
                        commands.AddRange(command);
                        return commands;
                    }
                }
            }

            return commands;
        }
    }
}