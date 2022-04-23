using LokiBot.Builds;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.MicroControllers;
using System.Collections.Generic;

namespace LokiBot
{
    internal class MyBuildChoices
    {
        public MyBuildChoices(Sharky.LokiBot.LokiBot lokiBot)
        {
            var MarineTankMedivacTvP = new MarineTankMedivacTvP(lokiBot);

            var scvMicroController = new IndividualMicroController(lokiBot, lokiBot.SharkyAdvancedPathFinder, MicroPriority.JustLive, false);

            var builds = new Dictionary<string, ISharkyBuild>
            {
                [MarineTankMedivacTvP.Name()] = MarineTankMedivacTvP
            };
            var versusTerran = new List<List<string>>
            {
                new List<string> { MarineTankMedivacTvP.Name() }
            };
            var versusEverything = new List<List<string>>
            {
                new List<string> { MarineTankMedivacTvP.Name() }
            };
            var transitions = new List<List<string>>
            {
                new List<string> { MarineTankMedivacTvP.Name() },
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
        }

        public BuildChoices BuildChoices { get; private set; }
    }
}