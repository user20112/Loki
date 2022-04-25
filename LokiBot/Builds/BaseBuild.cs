using SC2APIProtocol;
using Sharky;
using Sharky.Builds.Terran;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LokiBot.Builds
{
    public abstract class BaseBuild : TerranSharkyBuild
    {
        public BaseBuild(Sharky.LokiBot.BaseLokiBot bot) : base(bot)
        {
        }

        public void BuildAddonsWithCallback(UnitTypes AddonToBuild, int count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredAddOnCounts[AddonToBuild];
            MacroData.DesiredAddOnCounts[AddonToBuild] += count;
            int Desired = MacroData.DesiredAddOnCounts[AddonToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(AddonToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(AddonToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildAddonWithCallback(UnitTypes AddonToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredAddOnCounts[AddonToBuild]++;
            int Desired = MacroData.DesiredAddOnCounts[AddonToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(AddonToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(AddonToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildDefensiveStructuresWithCallbacks(UnitTypes StructureToBuild, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredDefensiveBuildingsAtDefensivePoint[StructureToBuild];
            MacroData.DesiredDefensiveBuildingsAtDefensivePoint[StructureToBuild] += Count;
            int Desired = MacroData.DesiredDefensiveBuildingsAtDefensivePoint[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildDefensiveStructureWithCallbacks(UnitTypes StructureToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredDefensiveBuildingsAtDefensivePoint[StructureToBuild]++;
            int Desired = MacroData.DesiredDefensiveBuildingsAtDefensivePoint[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildGasesWithCallback(int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredGases;
            MacroData.DesiredGases += Count;
            int Desired = MacroData.DesiredGases;
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitTypes.TERRAN_REFINERY) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitTypes.TERRAN_REFINERY) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildGasWithCallback(Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredGases++;
            int Desired = MacroData.DesiredGases;
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitTypes.TERRAN_REFINERY) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitTypes.TERRAN_REFINERY) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildProductionStructuresWithCallbacks(UnitTypes StructureToBuild, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredProductionCounts[StructureToBuild];
            MacroData.DesiredProductionCounts[StructureToBuild] += Count;
            int Desired = MacroData.DesiredProductionCounts[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildProductionStructureWithCallbacks(UnitTypes StructureToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredProductionCounts[StructureToBuild]++;
            int Desired = MacroData.DesiredProductionCounts[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildStructure(UnitTypes StructureToBuild)
        {
            BuildStructureWithCallbacks(StructureToBuild);
        }

        public void BuildStructures(UnitTypes StructureToBuild, int Count)
        {
            BuildStructuresWithCallbacks(StructureToBuild, Count);
        }

        public void BuildStructuresWithCallbacks(UnitTypes StructureToBuild, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            switch (StructureToBuild)
            {
                case UnitTypes.TERRAN_COMMANDCENTER:
                case UnitTypes.TERRAN_STARPORT:
                case UnitTypes.TERRAN_FACTORY:
                case UnitTypes.TERRAN_BARRACKS:
                    BuildProductionStructuresWithCallbacks(StructureToBuild, Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_MISSILETURRET:
                case UnitTypes.TERRAN_SENSORTOWER:
                case UnitTypes.TERRAN_BUNKER:
                    BuildDefensiveStructuresWithCallbacks(StructureToBuild, Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_ARMORY:
                case UnitTypes.TERRAN_FUSIONCORE:
                case UnitTypes.TERRAN_ENGINEERINGBAY:
                    BuildTechStructuresWithCallbacks(StructureToBuild, Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_GHOSTACADEMY:
                    break;

                case UnitTypes.TERRAN_PLANETARYFORTRESS:
                case UnitTypes.TERRAN_ORBITALCOMMAND:
                    MorphStructuresWithCallbacks(StructureToBuild, Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_REFINERY:
                    BuildGasesWithCallback(Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_FACTORYTECHLAB:
                case UnitTypes.TERRAN_FACTORYREACTOR:
                case UnitTypes.TERRAN_BARRACKSTECHLAB:
                case UnitTypes.TERRAN_BARRACKSREACTOR:
                case UnitTypes.TERRAN_STARPORTTECHLAB:
                case UnitTypes.TERRAN_STARPORTREACTOR:
                    BuildAddonsWithCallback(StructureToBuild, Count, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_SUPPLYDEPOT:
                    BuildSupplyDepotsWithCallbacks(Count, BuiltCallBack, StartedCallBack);
                    break;
            }
        }

        public void BuildStructureWithCallbacks(UnitTypes StructureToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            switch (StructureToBuild)
            {
                case UnitTypes.TERRAN_COMMANDCENTER:
                case UnitTypes.TERRAN_STARPORT:
                case UnitTypes.TERRAN_FACTORY:
                case UnitTypes.TERRAN_BARRACKS:
                    BuildProductionStructureWithCallbacks(StructureToBuild, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_SENSORTOWER:
                case UnitTypes.TERRAN_BUNKER:
                    BuildDefensiveStructureWithCallbacks(StructureToBuild, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_ARMORY:
                case UnitTypes.TERRAN_FUSIONCORE:
                case UnitTypes.TERRAN_ENGINEERINGBAY:
                    BuildTechStructureWithCallbacks(StructureToBuild, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_GHOSTACADEMY:
                    break;

                case UnitTypes.TERRAN_MISSILETURRET:
                    break;

                case UnitTypes.TERRAN_ORBITALCOMMAND:
                    break;

                case UnitTypes.TERRAN_PLANETARYFORTRESS:
                    break;

                case UnitTypes.TERRAN_REFINERY:
                    BuildGasWithCallback(BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_FACTORYTECHLAB:
                case UnitTypes.TERRAN_FACTORYREACTOR:
                case UnitTypes.TERRAN_BARRACKSTECHLAB:
                case UnitTypes.TERRAN_BARRACKSREACTOR:
                case UnitTypes.TERRAN_STARPORTTECHLAB:
                case UnitTypes.TERRAN_STARPORTREACTOR:
                    BuildAddonWithCallback(StructureToBuild, BuiltCallBack, StartedCallBack);
                    break;

                case UnitTypes.TERRAN_SUPPLYDEPOT:
                    BuildSupplyDepotWithCallbacks(BuiltCallBack, StartedCallBack);
                    break;
            }
        }

        public void BuildTechStructuresWithCallbacks(UnitTypes StructureToBuild, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredTechCounts[StructureToBuild];
            MacroData.DesiredTechCounts[StructureToBuild] += Count;
            int Desired = MacroData.DesiredTechCounts[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildTechStructureWithCallbacks(UnitTypes StructureToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredTechCounts[StructureToBuild]++;
            int Desired = MacroData.DesiredTechCounts[StructureToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildUnit(UnitTypes UnitToBuild)
        {
            BuildUnitWithCallback(UnitToBuild);
        }

        public void BuildUnits(UnitTypes UnitToBuild, int Count)
        {
            BuildUnitsWithCallback(UnitToBuild, Count);
        }

        public void BuildUnitsWithCallback(UnitTypes UnitToBuild, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredUnitCounts[UnitToBuild];
            MacroData.DesiredUnitCounts[UnitToBuild] += Count;
            int Desired = MacroData.DesiredUnitCounts[UnitToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildUnitWithCallback(UnitTypes UnitToBuild, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredUnitCounts[UnitToBuild]++;
            int Desired = MacroData.DesiredUnitCounts[UnitToBuild];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitToBuild) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitToBuild) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void BuildUpgrade(Upgrades UpgradeTobuild)
        {
            BuildUpgradeWithCallback(UpgradeTobuild);
        }

        public void BuildUpgradeWithCallback(Upgrades Upgrade, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            MacroData.DesiredUpgrades[Upgrade] = true;
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (!UnitCountService.UpgradeInProgress(Upgrade))
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (!UnitCountService.UpgradeDone(Upgrade))
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public override List<string> CounterTransition(int frame)
        {
            return base.CounterTransition(frame);
        }

        public override void EndBuild(int frame)
        {
            base.EndBuild(frame);
        }

        public void MorphStructuresWithCallbacks(UnitTypes StructureToMorph, int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredMorphCounts[StructureToMorph];
            MacroData.DesiredMorphCounts[StructureToMorph] += Count;
            int Desired = MacroData.DesiredMorphCounts[StructureToMorph];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToMorph) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToMorph) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public void MorphStructureWithCallbacks(UnitTypes StructureToMorph, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredMorphCounts[StructureToMorph]++;
            int Desired = MacroData.DesiredMorphCounts[StructureToMorph];
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(StructureToMorph) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(StructureToMorph) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        public override void OnFrame(ResponseObservation observation)
        {
            base.OnFrame(observation);
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);
            MicroTaskData.MicroTasks["DefenseSquadTask"].Disable();
            MicroTaskData.MicroTasks["WorkerScoutGasStealTask"].Disable();
            MicroTaskData.MicroTasks["WorkerScoutTask"].Disable();
            MicroTaskData.MicroTasks["ReaperScoutTask"].Disable();
            MicroTaskData.MicroTasks["FindHiddenBaseTask"].Disable();
            MicroTaskData.MicroTasks["ProxyScoutTask"].Disable();
            MicroTaskData.MicroTasks["MiningTask"].Disable();
            MicroTaskData.MicroTasks["AttackTask"].Disable();
            MicroTaskData.MicroTasks["ReaperWorkerHarassTask"].Disable();
            MicroTaskData.MicroTasks["BansheeHarassTask"].Disable();
            MicroTaskData.MicroTasks["WallOffTask"].Disable();
            MicroTaskData.MicroTasks["PermanentWallOffTask"].Disable();
            MicroTaskData.MicroTasks["DestroyWallOffTask"].Disable();
            MicroTaskData.MicroTasks["PrePositionBuilderTask"].Disable();
            MicroTaskData.MicroTasks["RepairTask"].Disable();
            MicroTaskData.MicroTasks["SaveLiftableBuildingTask"].Disable();
            MicroTaskData.MicroTasks["HellbatMorphTask"].Disable();
            MicroTaskData.MicroTasks["ReaperMiningDefenseTask"].Disable();
        }

        public override bool Transition(int frame)
        {
            return base.Transition(frame);
        }

        private void BuildSupplyDepotsWithCallbacks(int Count, Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredSupplyDepots;
            MacroData.DesiredSupplyDepots += Count;
            int Desired = MacroData.DesiredSupplyDepots;
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitTypes.TERRAN_SUPPLYDEPOT) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitTypes.TERRAN_SUPPLYDEPOT) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }

        private void BuildSupplyDepotWithCallbacks(Action<object> BuiltCallBack = null, Action<object> StartedCallBack = null)
        {
            int Current = MacroData.DesiredSupplyDepots++;
            int Desired = MacroData.DesiredSupplyDepots;
            if (BuiltCallBack == null && StartedCallBack == null)
                return;
            Task.Run(() =>
            {
                while (UnitCountService.BuildingsDoneAndInProgressCount(UnitTypes.TERRAN_SUPPLYDEPOT) + UnitCountService.BuildingsDoneAndInProgressCount(UnitTypes.TERRAN_SUPPLYDEPOTLOWERED) != Desired)
                    Thread.Sleep(50);
                StartedCallBack?.Invoke(null);
                while (UnitCountService.Completed(UnitTypes.TERRAN_SUPPLYDEPOT) + UnitCountService.Completed(UnitTypes.TERRAN_SUPPLYDEPOTLOWERED) != Desired)
                    Thread.Sleep(50);
                BuiltCallBack?.Invoke(null);
            });
        }
    }
}