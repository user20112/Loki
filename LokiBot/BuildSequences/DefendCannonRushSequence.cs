using LokiBot.Builds;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using System.Collections.Generic;

namespace LokiBot.BuildSequences
{
    internal class DefendCannonRushSequence : BaseBuild
    {
        private bool SequenceDone = false;

        public DefendCannonRushSequence(Sharky.LokiBot.BaseLokiBot bot) : base(bot)
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
        }

        public override bool Transition(int frame)
        {
            return SequenceDone;
        }
    }
}