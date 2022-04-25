﻿using SC2APIProtocol;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.Macro
{
    public class UpgradeResearcher
    {
        private ActiveUnitData ActiveUnitData;
        private MacroData MacroData;
        private SharkyUnitData SharkyUnitData;

        public UpgradeResearcher(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            MacroData = lokiBot.MacroData;
            ActiveUnitData = lokiBot.ActiveUnitData;
            SharkyUnitData = lokiBot.SharkyUnitData;
        }

        public List<Action> ResearchUpgrades()
        {
            var commands = new List<Action>();

            foreach (var upgrade in MacroData.DesiredUpgrades)
            {
                if (upgrade.Value && !SharkyUnitData.ResearchedUpgrades.Contains((uint)upgrade.Key))
                {
                    var upgradeData = SharkyUnitData.UpgradeData[upgrade.Key];

                    if (!ActiveUnitData.Commanders.Any(c => upgradeData.ProducingUnits.Contains((UnitTypes)c.Value.UnitCalculation.Unit.UnitType) && c.Value.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (int)upgradeData.Ability)))
                    {
                        var building = ActiveUnitData.Commanders.Where(c => upgradeData.ProducingUnits.Contains((UnitTypes)c.Value.UnitCalculation.Unit.UnitType) && !c.Value.UnitCalculation.Unit.IsActive && c.Value.UnitCalculation.Unit.BuildProgress == 1 && c.Value.LastOrderFrame != MacroData.Frame);
                        if (building.Count() > 0)
                        {
                            if (upgradeData.Minerals <= MacroData.Minerals && upgradeData.Gas <= MacroData.VespeneGas)
                            {
                                commands.AddRange(building.First().Value.Order(MacroData.Frame, upgradeData.Ability));
                            }
                        }
                    }
                }
            }

            return commands;
        }
    }
}