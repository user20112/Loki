﻿using LokiBot.Builds.BuildServices;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds.Terran;
using Sharky.MicroTasks;

namespace LokiBot.Builds
{
    public class BansheesAndMarines : TerranSharkyBuild
    {
        private ExpandForever ExpandForever;
        private ProxyScoutTask ProxyScoutTask;
        private bool Scouted;
        private WorkerScoutTask WorkerScoutTask;

        public BansheesAndMarines(Sharky.LokiBot.LokiBot lokiBot) : base(lokiBot)
        {
            ExpandForever = new ExpandForever(lokiBot);
        }

        public override void OnFrame(ResponseObservation observation)
        {
            Opening();
            TechUp();
            LateGame();
            ExpandProduction();
            ExpandForever.OnFrame();
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            BuildOptions.StrictGasCount = true;

            if (MicroTaskData.MicroTasks.ContainsKey("WorkerScoutTask"))
            {
                WorkerScoutTask = (WorkerScoutTask)MicroTaskData.MicroTasks["WorkerScoutTask"];
            }
            if (MicroTaskData.MicroTasks.ContainsKey("ProxyScoutTask"))
            {
                ProxyScoutTask = (ProxyScoutTask)MicroTaskData.MicroTasks["ProxyScoutTask"];
            }
        }

        private void ExpandProduction()
        {
            if (UnitCountService.EquivalentTypeCompleted(UnitTypes.TERRAN_COMMANDCENTER) > 2)
            {
                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_RAVEN] < 1)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_RAVEN] = 1;
                }

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] < 4)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] = 4;
                }
                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] < 4)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] = 4;
                }

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 4)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 4;
                }
                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSREACTOR] < 3)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSREACTOR] = 3;
                }
            }
        }

        private void LateGame()
        {
            if (UnitCountService.Count(UnitTypes.TERRAN_STARPORT) > 0)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 2)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 2;
                }
                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSREACTOR] < 1)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSREACTOR] = 1;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_STARPORT) > 0)
            {
                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] < 2)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_STARPORTTECHLAB] = 2;
                }
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_STARPORTTECHLAB) > 0)
            {
                MacroData.DesiredUpgrades[Upgrades.BANSHEECLOAK] = true;
                MacroData.DesiredUpgrades[Upgrades.BANSHEESPEED] = true;

                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_BANSHEE] < 25)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_BANSHEE] = 25;
                }
                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] < 50)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] = 50;
                }
            }
        }

        private void Opening()
        {
            if (MacroData.FoodUsed >= 15)
            {
                Scout();

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 1)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 1;
                }
            }

            if (UnitCountService.Count(UnitTypes.TERRAN_BARRACKS) > 0)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] < 2)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] = 2;
                }

                MacroData.DesiredGases = 1;

                if (MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] < 20)
                {
                    MacroData.DesiredUnitCounts[UnitTypes.TERRAN_MARINE] = 20;
                }
            }
        }

        private void Scout()
        {
            if (!Scouted)
            {
                if (WorkerScoutTask != null)
                {
                    WorkerScoutTask.Enable();
                }
                if (ProxyScoutTask != null)
                {
                    ProxyScoutTask.Enable();
                }
                Scouted = true;
            }
        }

        private void TechUp()
        {
            if (UnitCountService.Completed(UnitTypes.TERRAN_BARRACKS) > 0 && UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER) >= 2)
            {
                BuildOptions.StrictGasCount = false;

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_FACTORY] < 1)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_FACTORY] = 1;
                }

                if (MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] < 1)
                {
                    MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] = 1;
                }

                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] < 1)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_BARRACKS] = 1;
                }

                if (MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSTECHLAB] < 1)
                {
                    MacroData.DesiredAddOnCounts[UnitTypes.TERRAN_BARRACKSTECHLAB] = 1;
                }

                MacroData.DesiredUpgrades[Upgrades.SHIELDWALL] = true;
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_BARRACKSTECHLAB) > 0)
            {
                MacroData.DesiredUpgrades[Upgrades.STIMPACK] = true;
            }

            if (UnitCountService.Completed(UnitTypes.TERRAN_FACTORY) > 0)
            {
                if (MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] < 2)
                {
                    MacroData.DesiredProductionCounts[UnitTypes.TERRAN_STARPORT] = 2;
                }
            }
        }
    }
}