﻿using SC2APIProtocol;

namespace Sharky.Builds.Terran
{
    public class BattleCruisers : TerranSharkyBuild
    {
        public BattleCruisers(Sharky.LokiBot.BaseLokiBot lokiBot) : base(lokiBot)
        {
        }

        public override void OnFrame(ResponseObservation observation)
        {
            if (MacroData.FoodUsed >= 15)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 1)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 1;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_BARRACKS) > 0)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_FACTORY] < 1)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_FACTORY] = 1;
                }

                if (MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] < 1)
                {
                    MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] = 1;
                }

                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] < 4)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] = 4;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_FACTORY) > 0)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] < 2)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] = 2;
                }

                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_HELLION] < 4)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_HELLION] = 4;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_STARPORT) > 0)
            {
                if (MacroData.DesiredTechCounts[UnitTypes.TERRAN_FUSIONCORE] < 1)
                {
                    MacroData.DesiredTechCounts[UnitTypes.TERRAN_FUSIONCORE] = 1;
                }

                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] < 2)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] = 2;
                }

                MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] = 0;
                MacroData.DesiredUnitCounts[UnitTypes.TERRAN_HELLION] = 0;
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_FUSIONCORE) > 0)
            {
                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_BATTLECRUISER] < 10)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_BATTLECRUISER] = 10;
                }
            }

            if (MacroData.FoodUsed >= 50)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] < 2)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] = 2;
                }
                if (MacroData.DesiredTechCounts[UnitTypes.TERRAN_ARMORY] < 1)
                {
                    MacroData.DesiredTechCounts[UnitTypes.TERRAN_ARMORY] = 1;
                }
            }
            if (MacroData.FoodUsed >= 100)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] < 3)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] = 3;
                }
                if (MacroData.DesiredTechCounts[UnitTypes.TERRAN_ARMORY] < 2)
                {
                    MacroData.DesiredTechCounts[UnitTypes.TERRAN_ARMORY] = 2;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_ARMORY) > 0)
            {
                MacroData.DesiredUpgrades[Upgrades.TERRANSHIPWEAPONSLEVEL1] = true;
                MacroData.DesiredUpgrades[Upgrades.TERRANSHIPWEAPONSLEVEL2] = true;
                MacroData.DesiredUpgrades[Upgrades.TERRANSHIPWEAPONSLEVEL3] = true;
            }
            if (UnitCountService.Completed(UnitTypes.TERRAN_ARMORY) > 1)
            {
                MacroData.DesiredUpgrades[Upgrades.TERRANVEHICLEANDSHIPARMORSLEVEL1] = true;
                MacroData.DesiredUpgrades[Upgrades.TERRANVEHICLEANDSHIPARMORSLEVEL2] = true;
                MacroData.DesiredUpgrades[Upgrades.TERRANVEHICLEANDSHIPARMORSLEVEL3] = true;
            }
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);
        }
    }
}