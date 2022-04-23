using LokiBot.Builds;
using LokiBot.MicroTasks;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.MicroControllers;
using System.Collections.Generic;

namespace LokiBot
{
    public class TerranBuildChoices
    {
        public TerranBuildChoices(Sharky.LokiBot.LokiBot lokiBot)
        {
            var hellionRush = new HellionRush(lokiBot);
            var massVikings = new MassVikings(lokiBot);
            var bansheesAndMarines = new BansheesAndMarines(lokiBot);
            var adaptiveOpening = new AdaptiveOpening(lokiBot);
            var vikingDrops = new VikingDrops(lokiBot);
            var MarineMarauderTankTiming = new VikingDrops(lokiBot);

            var scvMicroController = new IndividualMicroController(lokiBot, lokiBot.SharkyAdvancedPathFinder, MicroPriority.JustLive, false);
            var reaperCheese = new ReaperCheese(lokiBot, scvMicroController);

            var builds = new Dictionary<string, ISharkyBuild>
            {
                [hellionRush.Name()] = hellionRush,
                [massVikings.Name()] = massVikings,
                [bansheesAndMarines.Name()] = bansheesAndMarines,
                [adaptiveOpening.Name()] = adaptiveOpening,
                [vikingDrops.Name()] = vikingDrops,
                [reaperCheese.Name()] = reaperCheese
            };
            var versusTerran = new List<List<string>>
            {
                new List<string> { hellionRush.Name() },
                new List<string> { reaperCheese.Name() },
                new List<string> { vikingDrops.Name() }
            };
            var versusEverything = new List<List<string>>
            {
                new List<string> { adaptiveOpening.Name() },
                new List<string> { hellionRush.Name() },
                new List<string> { massVikings.Name() }
            };
            var transitions = new List<List<string>>
            {
                new List<string> { bansheesAndMarines.Name() },
            };

            var buildSequences = new Dictionary<string, List<List<string>>>
            {
                [Race.Terran.ToString()] = versusTerran,
                [Race.Zerg.ToString()] = versusEverything,
                [Race.Protoss.ToString()] = versusEverything,
                [Race.Random.ToString()] = versusEverything,
                ["Transition"] = transitions,
            };

            BuildChoices = new BuildChoices { Builds = builds, BuildSequences = buildSequences };

            AddTerranTasks(lokiBot);
        }

        public BuildChoices BuildChoices { get; private set; }

        private void AddTerranTasks(Sharky.LokiBot.LokiBot lokiBot)
        {
            var vikingDropTask = new VikingDropTask(lokiBot, .5f, false);
            lokiBot.MicroTaskData.MicroTasks[vikingDropTask.GetType().Name] = vikingDropTask;
        }
    }
}