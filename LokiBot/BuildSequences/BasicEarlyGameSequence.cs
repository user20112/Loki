using LokiBot.Builds;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using System.Collections.Generic;
using System.Linq;

namespace LokiBot.BuildSequences
{
    internal class BasicEarlyGameSequence : BaseBuild
    {
        private bool BuiltAttackForce = false;
        private bool SequenceDone = false;

        public BasicEarlyGameSequence(Sharky.LokiBot.BaseLokiBot bot) : base(bot)
        {
        }

        public override BuildSegment Segment => BuildSegment.EarlyGame;

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
            if (BuiltAttackForce)
            {
                foreach (var unit in ActiveUnitData.Commanders.Where(c => !c.Value.UnitCalculation.Attributes.Contains(SC2APIProtocol.Attribute.Structure)))
                {
                    if (unit.Value.UnitCalculation.UnitClassifications.Contains(UnitClassification.ArmyUnit))
                    {
                        unit.Value.UnitRole = UnitRole.Attack;
                    }
                }
            }
            else
            {
                foreach (var unit in ActiveUnitData.Commanders.Where(c => !c.Value.UnitCalculation.Attributes.Contains(SC2APIProtocol.Attribute.Structure)))
                {
                    if (unit.Value.UnitCalculation.UnitClassifications.Contains(UnitClassification.ArmyUnit))
                    {
                        unit.Value.UnitRole = UnitRole.Defend;
                    }
                }
            }
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
            BuildStructureWithCallbacks(UnitTypes.TERRAN_SUPPLYDEPOT, null, (object obj) =>
            {
                BuildUnit(UnitTypes.TERRAN_MARINE);
                BuildStructureWithCallbacks(UnitTypes.TERRAN_FACTORY, null, (object obj) =>
                {
                    BuildStructure(UnitTypes.TERRAN_BARRACKSREACTOR);
                    BuildStructureWithCallbacks(UnitTypes.TERRAN_BARRACKS, null, (object obj) =>
                    {
                        BuildStructure(UnitTypes.TERRAN_REFINERY);
                        BuildStructure(UnitTypes.TERRAN_BARRACKSTECHLAB);
                        BuildStructureWithCallbacks(UnitTypes.TERRAN_FACTORYTECHLAB, null, (object obj) =>
                        {
                            BuildOptions.StrictSupplyCount = false;
                            BuildStructures(UnitTypes.TERRAN_BUNKER, 2);
                            BuildStructureWithCallbacks(UnitTypes.TERRAN_STARPORT, (object obj) =>
                            {
                                BuildStructure(UnitTypes.TERRAN_STARPORTREACTOR);
                                BuildUnits(UnitTypes.TERRAN_MEDIVAC, 2);
                            });
                            BuildStructureWithCallbacks(UnitTypes.TERRAN_FACTORY, (object obj) =>
                            {
                                BuildStructure(UnitTypes.TERRAN_FACTORYTECHLAB);
                            });
                            BuildStructuresWithCallbacks(UnitTypes.TERRAN_BARRACKS, 2, (object obj) =>
                            {
                                BuildStructure(UnitTypes.TERRAN_BARRACKSREACTOR);
                                BuildStructure(UnitTypes.TERRAN_BARRACKSTECHLAB);
                            });
                            BuildUnits(UnitTypes.TERRAN_MARINE, 70);
                            BuildUnits(UnitTypes.TERRAN_MARAUDER, 20);
                            BuildUnitsWithCallback(UnitTypes.TERRAN_SIEGETANK, 4, (obj) => { BuiltAttackForce = true; });
                            BuildUnitsWithCallback(UnitTypes.TERRAN_SIEGETANK, 2, SiegeTanksBuilt);
                            BuildUpgrade(Upgrades.STIMPACK);
                            BuildUpgrade(Upgrades.SHIELDWALL);
                            BuildStructure(UnitTypes.TERRAN_COMMANDCENTER);
                            BuildStructure(UnitTypes.TERRAN_ORBITALCOMMAND);
                            BuildOptions.StrictWorkerCount = false;
                        });
                    });
                });
            });
        }

        public override bool Transition(int frame)
        {
            return SequenceDone;
        }

        private void SiegeTanksBuilt(object obj)
        {
            SequenceDone = true;
        }
    }
}