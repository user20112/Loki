using LokiBot.Builds;
using SC2APIProtocol;
using Sharky;
using System.Collections.Generic;

namespace LokiBot.BuildSequences
{
    public class BasicMidSequence : BaseBuild
    {
        private bool SequenceDone = false;

        public BasicMidSequence(Sharky.LokiBot.LokiBot bot) : base(bot)
        {
        }

        public override List<string> CounterTransition(int frame)
        {
            return base.CounterTransition(frame);
        }

        public override void EndBuild(int frame)
        {
            base.EndBuild(frame);
        }

        public void EvaluateAttack()
        {
        }

        public void EvaluateDefense()
        {
        }

        public void EvaluateScout()
        {
        }

        public override void OnFrame(ResponseObservation observation)
        {
            base.OnFrame(observation);
        }

        public override void StartBuild(int frame)
        {
            MicroTaskData.MicroTasks["DefenseSquadTask"].Enable();
            MicroTaskData.MicroTasks["WorkerScoutGasStealTask"].Disable();
            MicroTaskData.MicroTasks["WorkerScoutTask"].Enable();
            MicroTaskData.MicroTasks["ReaperScoutTask"].Disable();
            MicroTaskData.MicroTasks["FindHiddenBaseTask"].Disable();
            MicroTaskData.MicroTasks["ProxyScoutTask"].Disable();
            MicroTaskData.MicroTasks["MiningTask"].Enable();
            MicroTaskData.MicroTasks["AttackTask"].Disable();
            MicroTaskData.MicroTasks["ReaperWorkerHarassTask"].Disable();
            MicroTaskData.MicroTasks["BansheeHarassTask"].Disable();
            MicroTaskData.MicroTasks["WallOffTask"].Disable();
            MicroTaskData.MicroTasks["PermanentWallOffTask"].Disable();
            MicroTaskData.MicroTasks["DestroyWallOffTask"].Disable();
            MicroTaskData.MicroTasks["PrePositionBuilderTask"].Enable();
            MicroTaskData.MicroTasks["RepairTask"].Enable();
            MicroTaskData.MicroTasks["SaveLiftableBuildingTask"].Enable();
            MicroTaskData.MicroTasks["HellbatMorphTask"].Disable();
            MicroTaskData.MicroTasks["ReaperMiningDefenseTask"].Enable();
            StartFrame = frame;
            if (!Started)
            {
                if (SharkyOptions.TagsEnabled && SharkyOptions.BuildTagsEnabled)
                {
                    ChatService.SendAllyChatMessage($"Tag:Build-{Name()}", true);
                }
                Started = true;
            }
            BuildOptions.AllowBlockWall = false;
            BuildOptions.StrictGasCount = true;
            BuildOptions.StrictSupplyCount = true;
            BuildOptions.StrictWorkerCount = true;
            BuildOptions.StrictWorkersPerGas = true;
            BuildOptions.StrictWorkersPerGasCount = 3;
            BuildOptions.MaxActiveGasCount = 8;
            AttackData.UseAttackDataManager = true;

            foreach (Sharky.UnitTypes Unit in MacroData.Units)
            {
                MacroData.DesiredUnitCounts[Unit] = 0;
            }
            foreach (Sharky.UnitTypes ProductionBuilding in MacroData.Production)
            {
                MacroData.DesiredProductionCounts[ProductionBuilding] = 0;
            }
            foreach (Sharky.UnitTypes TechBuilding in MacroData.Tech)
            {
                MacroData.DesiredTechCounts[TechBuilding] = 0;
            }
            foreach (Sharky.UnitTypes DefensiveBuildings in MacroData.DefensiveBuildings)
            {
                MacroData.DesiredDefensiveBuildingsCounts[DefensiveBuildings] = 0;
                MacroData.DesiredDefensiveBuildingsAtDefensivePoint[DefensiveBuildings] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryBase[DefensiveBuildings] = 0;
                MacroData.DesiredDefensiveBuildingsAtNextBase[DefensiveBuildings] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryMineralLine[DefensiveBuildings] = 0;
            }
            BuildStructure(UnitTypes.TERRAN_COMMANDCENTER);
            BuildUnits(UnitTypes.TERRAN_SCV, 12);

            BuildUnits(UnitTypes.TERRAN_SCV, 3);
            BuildStructureWithCallbacks(Sharky.UnitTypes.TERRAN_SUPPLYDEPOT, FirstDepotBuilt, FirstDepotStarted);
        }

        public override bool Transition(int frame)
        {
            return SequenceDone;
        }

        private void FirstBaracksFinished(object obj)
        {
            BuildStructuresWithCallbacks(UnitTypes.TERRAN_ORBITALCOMMAND, 1, null, (object obj) =>
            {
                BuildUnits(UnitTypes.TERRAN_SCV, 7);
            });
            BuildStructuresWithCallbacks(UnitTypes.TERRAN_ORBITALCOMMAND, 1, null, (object obj) =>
            {
                BuildUnits(UnitTypes.TERRAN_SCV, 10);
            });
        }

        private void FirstBaracksStarted(object obj)
        {
            BuildStructureWithCallbacks(Sharky.UnitTypes.TERRAN_COMMANDCENTER, null, SecondStarted);
        }

        private void FirstDepotBuilt(object obj)
        {
            BuildStructureWithCallbacks(Sharky.UnitTypes.TERRAN_BARRACKS, FirstBaracksFinished, FirstBaracksStarted);
        }

        private void FirstDepotStarted(object obj)
        {
            BuildStructureWithCallbacks(Sharky.UnitTypes.TERRAN_REFINERY, null, ((object obj) => { BuildUnits(UnitTypes.TERRAN_SCV, 4); }));
        }

        private void SecondStarted(object obj)
        {
            SequenceDone = true;
        }
    }
}