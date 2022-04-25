using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.Builds.BuildChoosing;
using Sharky.Chat;
using Sharky.EnemyPlayer;
using Sharky.EnemyStrategies;
using Sharky.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LokiBot.BotCode
{
    public class LokiBuildManager : SharkyManager
    {
        protected Race ActualRace;
        protected Dictionary<Race, BuildChoices> BuildChoices;
        protected List<string> BuildSequence;
        protected ChatHistory ChatHistory;
        protected ChatService ChatService;
        protected ISharkyBuild CurrentBuild;
        protected DebugService DebugService;
        protected EnemyPlayer EnemyPlayer;
        protected IEnemyPlayerService EnemyPlayerService;
        protected Race EnemyRace;
        protected Race EnemySelectedRace;
        protected EnemyStrategyHistory EnemyStrategyHistory;
        protected FrameToTimeConverter FrameToTimeConverter;
        protected IMacroBalancer MacroBalancer;
        protected string MapName;
        protected Race SelectedRace;
        protected SharkyOptions SharkyOptions;
        protected SimCityService SimCityService;
        private Sharky.LokiBot.BaseLokiBot LokiBot;

        public LokiBuildManager(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            LokiBot = lokiBot;
            BuildChoices = lokiBot.BuildChoices;
            DebugService = lokiBot.DebugService;
            MacroBalancer = lokiBot.MacroBalancer;
            BuildDecisionService = lokiBot.BuildDecisionService;
            EnemyPlayerService = lokiBot.EnemyPlayerService;
            ChatHistory = lokiBot.ChatHistory;
            EnemyStrategyHistory = lokiBot.EnemyStrategyHistory;
            FrameToTimeConverter = lokiBot.FrameToTimeConverter;
            SharkyOptions = lokiBot.SharkyOptions;
            ChatService = lokiBot.ChatService;
            SimCityService = lokiBot.SimCityService;
        }

        public LokiBuildManager(Dictionary<Race, BuildChoices> buildChoices, DebugService debugService, IMacroBalancer macroBalancer, IBuildDecisionService buildDecisionService, IEnemyPlayerService enemyPlayerService, ChatHistory chatHistory, EnemyStrategyHistory enemyStrategyHistory, FrameToTimeConverter frameToTimeConverter, SharkyOptions sharkyOptions, ChatService chatService, SimCityService simCityService)
        {
            BuildChoices = buildChoices;
            DebugService = debugService;
            MacroBalancer = macroBalancer;
            BuildDecisionService = buildDecisionService;
            EnemyPlayerService = enemyPlayerService;
            ChatHistory = chatHistory;
            EnemyStrategyHistory = enemyStrategyHistory;
            FrameToTimeConverter = frameToTimeConverter;
            SharkyOptions = sharkyOptions;
            ChatService = chatService;
            SimCityService = simCityService;
        }

        public IBuildDecisionService BuildDecisionService { get; set; }
        protected Dictionary<int, string> BuildHistory { get; set; }

        private string CurrentEnemyStrategy
        {
            get
            {
                if (EnemyStrategyHistory.History.Count == 0)
                    return "Unknown";
                return EnemyStrategyHistory.History[EnemyStrategyHistory.History.Count - 1];
            }
        }

        public override void OnEnd(ResponseObservation observation, Result result)
        {
            Console.WriteLine($"Build Sequence: {string.Join(" ", BuildHistory.Select(b => b.Value.ToString()))}");

            var game = GetGame(observation, result);
            EnemyPlayerService.SaveGame(game);
        }

        public override IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation)
        {
            DebugService.DrawText("Build: " + CurrentBuild.Name());
            DebugService.DrawText("Sequence: " + string.Join(", ", BuildSequence));

            var frame = (int)observation.Observation.GameLoop;
            if (CurrentBuild.Transition(frame))
            {
                var buildSequenceIndex = BuildSequence.FindIndex(b => b == CurrentBuild.Name());
                if (buildSequenceIndex != -1 && BuildSequence.Count() > buildSequenceIndex + 1)
                {
                    SwitchToNextSegment(BuildSequence[buildSequenceIndex], CurrentBuild.Segment, frame);
                }
            }

            CurrentBuild.OnFrame(observation);

            var actions = SimCityService.OnFrame();
            MacroBalance();

            return actions;
        }

        public override void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentId)
        {
            GetPlayerInfo(gameInfo, playerId, opponentId);

            if (EnemyPlayerService.Tournament.Enabled)
            {
                foreach (var buildSequence in EnemyPlayerService.Tournament.BuildSequences)
                {
                    foreach (var sequence in buildSequence.Value)
                    {
                        BuildChoices[(Race)Enum.Parse(typeof(Race), buildSequence.Key)].BuildSequences[sequence.Key] = sequence.Value;
                    }
                }
            }

            var buildSequences = BuildChoices[ActualRace].BuildSequences[EnemyRace.ToString()];
            if (!string.IsNullOrWhiteSpace(EnemyPlayer.Name) && BuildChoices[ActualRace].BuildSequences.ContainsKey(EnemyPlayer.Name))
            {
                buildSequences = BuildChoices[ActualRace].BuildSequences[EnemyPlayer.Name];
            }

            MapName = gameInfo.MapName;
            BuildSequence = BuildDecisionService.GetBestBuild(EnemyPlayer, buildSequences, MapName, EnemyPlayerService.Enemies, EnemyRace, ActualRace);

            BuildHistory = new Dictionary<int, string>();
            SwitchBuild(BuildSequence.First(), 0);
        }

        protected Game GetGame(ResponseObservation observation, Result result)
        {
            var length = 0;
            if (observation != null)
            {
                length = (int)observation.Observation.GameLoop;
            }
            return new Game { DateTime = DateTime.Now, EnemySelectedRace = EnemySelectedRace, MySelectedRace = SelectedRace, MyRace = ActualRace, EnemyRace = EnemyRace, Length = length, MapName = MapName, Result = (int)result, EnemyId = EnemyPlayer.Id, Builds = BuildHistory, EnemyStrategies = EnemyStrategyHistory.History, EnemyChat = ChatHistory.EnemyChatHistory, MyChat = ChatHistory.MyChatHistory };
        }

        protected void GetPlayerInfo(ResponseGameInfo gameInfo, uint playerId, string opponentId)
        {
            string enemyName = string.Empty;

            foreach (var playerInfo in gameInfo.PlayerInfo)
            {
                if (playerInfo.PlayerId == playerId)
                {
                    ActualRace = playerInfo.RaceActual;
                    SelectedRace = playerInfo.RaceRequested;
                    if (SharkyOptions.TagsEnabled && playerInfo.RaceRequested == Race.Random)
                    {
                        ChatService.SendAllyChatMessage($"Tag:SelfRandomRace-{playerInfo.RaceActual}", true);
                    }
                }
                else
                {
                    if (playerInfo.PlayerName != null)
                    {
                        enemyName = playerInfo.PlayerName;
                    }
                    EnemyRace = playerInfo.RaceRequested;
                    EnemySelectedRace = playerInfo.RaceRequested;
                }
            }

            EnemyPlayer = EnemyPlayerService.Enemies.FirstOrDefault(e => e.Id == opponentId);
            if (opponentId == "test" && EnemyPlayer == null)
            {
                EnemyPlayer = new EnemyPlayer { ChatMatches = new List<string>(), Games = new List<Game>(), Id = opponentId, Name = "test" };
            }
            if (EnemyPlayer == null)
            {
                EnemyPlayer = new EnemyPlayer { ChatMatches = new List<string>(), Games = new List<Game>(), Id = opponentId, Name = enemyName };
            }
        }

        protected void MacroBalance()
        {
            MacroBalancer.BalanceSupply();
            MacroBalancer.BalanceGases();
            MacroBalancer.BalanceTech();
            MacroBalancer.BalanceAddOns();
            MacroBalancer.BalanceDefensiveBuildings();
            MacroBalancer.BalanceMorphs();
            MacroBalancer.BalanceProduction();
            MacroBalancer.BalanceProductionBuildings();
            MacroBalancer.BalanceGasWorkers();
        }

        protected void SwitchBuild(string buildName, int frame)
        {
            BuildHistory[frame] = buildName;
            if (CurrentBuild != null)
            {
                CurrentBuild.EndBuild(frame);
            }
            CurrentBuild = BuildChoices[ActualRace].Builds[buildName];
            CurrentBuild.StartBuild(frame);
        }

        private void SwitchToNextSegment(string CurrentBuildSequence, BuildSegment CurrentBuildSegment, int frame)
        {
            switch (CurrentBuildSegment)
            {
                case BuildSegment.Opener:
                    //we just finished our opener, see if we have any enemy info, and react accordingly
                    switch (CurrentEnemyStrategy)
                    {
                        case "FourGate":
                        case "AdeptRush":
                        case "ProxyRobo":
                            SwitchBuild("HighEarlyAgressionSequence", frame);
                            break;

                        case "CannonRush":
                            SwitchBuild("DefendCannonRushSequence", frame);
                            break;

                        case "ProtossFastExpand":
                            SwitchBuild("FastExpandProtossResponseSequence", frame);
                            break;

                        case "ProxyStarport":
                            SwitchBuild("DefendAirRushSequence", frame);
                            break;

                        case "ZealotRush":
                            SwitchBuild("DefendGroundMeleeRushSequence", frame);
                            break;

                        default:
                            SwitchBuild("BasicEarlyGameSequence", frame);
                            break;
                    }
                    break;

                case BuildSegment.EarlyGame:
                    //we just finished our early game, we should verify we are still going the right direction, and maybe narrow down our strategy
                    SwitchBuild("BasicMidGameSequence", frame);
                    break;

                case BuildSegment.MidGame:
                    //just finished mid game, we hopefully countered and did some serious damage, we should now be checking for their transition and reacting accordingly.
                    if (CurrentEnemyStrategy == "Unknown")
                    {
                        // we dont know so just go into a standard late game
                    }
                    else
                    {
                    }
                    SwitchBuild("DefaultLateGameSequence", frame);
                    break;

                case BuildSegment.MidToLate:
                    //ok so we just finished the transition period from mid to late, make sure we actually countered them correctly and setup for the late game.
                    if (CurrentEnemyStrategy == "Unknown")
                    {
                        // we dont know so just go into a standard
                    }
                    else
                    {
                    }
                    SwitchBuild("DefaultLateGameSequence", frame);
                    break;

                case BuildSegment.Late:
                    // late never ends we shouldnt get here
                    break;
            }
        }
    }
}