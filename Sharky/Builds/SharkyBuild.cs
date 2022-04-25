using SC2APIProtocol;
using Sharky.Chat;
using Sharky.MicroTasks.Macro;
using System;
using System.Collections.Generic;

namespace Sharky.Builds
{
    public abstract class SharkyBuild : ISharkyBuild
    {
        protected ActiveUnitData ActiveUnitData;
        protected AttackData AttackData;
        protected BuildOptions BuildOptions;
        protected ChatService ChatService;
        protected FrameToTimeConverter FrameToTimeConverter;
        protected MacroData MacroData;
        protected MicroTaskData MicroTaskData;
        protected PrePositionBuilderTask PrePositionBuilderTask;
        protected SharkyOptions SharkyOptions;
        protected bool Started;
        protected int StartFrame;
        protected UnitCountService UnitCountService;

        public SharkyBuild(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            BuildOptions = lokiBot.BuildOptions;
            SharkyOptions = lokiBot.SharkyOptions;
            MacroData = lokiBot.MacroData;
            ActiveUnitData = lokiBot.ActiveUnitData;
            AttackData = lokiBot.AttackData;
            ChatService = lokiBot.ChatService;
            UnitCountService = lokiBot.UnitCountService;
            MicroTaskData = lokiBot.MicroTaskData;
            FrameToTimeConverter = lokiBot.FrameToTimeConverter;

            if (lokiBot.MicroTaskData.MicroTasks.ContainsKey("PrePositionBuilderTask"))
            {
                PrePositionBuilderTask = (PrePositionBuilderTask)lokiBot.MicroTaskData.MicroTasks["PrePositionBuilderTask"];
            }

            Started = false;
        }

        public SharkyBuild(BuildOptions buildOptions, SharkyOptions sharkyOptions, MacroData macroData, ActiveUnitData activeUnitData, AttackData attackData, MicroTaskData microTaskData,
            ChatService chatService, UnitCountService unitCountService,
            FrameToTimeConverter frameToTimeConverter)
        {
            BuildOptions = buildOptions;
            SharkyOptions = sharkyOptions;
            MacroData = macroData;
            ActiveUnitData = activeUnitData;
            AttackData = attackData;
            ChatService = chatService;
            UnitCountService = unitCountService;
            MicroTaskData = microTaskData;
            FrameToTimeConverter = frameToTimeConverter;

            if (microTaskData.MicroTasks.ContainsKey("PrePositionBuilderTask"))
            {
                PrePositionBuilderTask = (PrePositionBuilderTask)microTaskData.MicroTasks["PrePositionBuilderTask"];
            }
        }

        public virtual BuildSegment Segment { get; }

        public virtual List<string> CounterTransition(int frame)
        {
            return null;
        }

        public virtual void EndBuild(int frame)
        {
        }

        public string Name()
        {
            return GetType().Name;
        }

        public virtual void OnFrame(ResponseObservation observation)
        {
        }

        public virtual void StartBuild(int frame)
        {
            Console.WriteLine($"{frame} {FrameToTimeConverter.GetTime(frame)} Build: {Name()}");
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
            BuildOptions.StrictGasCount = false;
            BuildOptions.StrictSupplyCount = false;
            BuildOptions.StrictWorkerCount = false;
            BuildOptions.StrictWorkersPerGas = false;
            BuildOptions.StrictWorkersPerGasCount = 3;
            BuildOptions.MaxActiveGasCount = 8;

            AttackData.UseAttackDataManager = true;
            AttackData.AttackTrigger = 1.5f;
            AttackData.RetreatTrigger = 1f;

            foreach (var u in MacroData.Units)
            {
                MacroData.DesiredUnitCounts[u] = 0;
            }
            foreach (var u in MacroData.Production)
            {
                MacroData.DesiredProductionCounts[u] = 0;
            }
            foreach (var u in MacroData.Tech)
            {
                MacroData.DesiredTechCounts[u] = 0;
            }
            foreach (var u in MacroData.DefensiveBuildings)
            {
                MacroData.DesiredDefensiveBuildingsCounts[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtDefensivePoint[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryBase[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtNextBase[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryMineralLine[u] = 0;
            }

            if (MacroData.Race == SC2APIProtocol.Race.Protoss)
            {
                MacroData.DesiredProductionCounts[UnitTypes.PROTOSS_NEXUS] = 1;
            }
            else if (MacroData.Race == SC2APIProtocol.Race.Terran)
            {
                MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] = 1;
            }
            else if (MacroData.Race == SC2APIProtocol.Race.Zerg)
            {
                MacroData.DesiredProductionCounts[UnitTypes.ZERG_HATCHERY] = 1;
            }

            if (MicroTaskData.MicroTasks.ContainsKey("AttackTask"))
            {
                MicroTaskData.MicroTasks["AttackTask"].Enable();
            }
        }

        public virtual bool Transition(int frame)
        {
            return false;
        }
    }
}