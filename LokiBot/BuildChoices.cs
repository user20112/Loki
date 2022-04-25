using LokiBot.Builds;
using LokiBot.BuildSequences;
using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.MicroControllers;
using System.Collections.Generic;

namespace LokiBot
{
    internal class MyBuildChoices
    {
        public MyBuildChoices(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            var BasicOpener = new BasicOpener(lokiBot);
            var BattlecruiserRushSequence = new BattlecruiserRushSequence(lokiBot);
            var FastExpandProtossResponseSequence = new FastExpandProtossResponseSequence(lokiBot);
            var HighEarlyAggressionSequence = new HighEarlyAgressionSequence(lokiBot);
            var BasicEarlyGameSequence = new BasicEarlyGameSequence(lokiBot);
            var BasicMidGameSequence = new BasicMidGameSequence(lokiBot);
            var DefaultLateGameSequence = new DefaultLateGameSequence(lokiBot);
            var DefendAirRushSequence = new DefendAirRushSequence(lokiBot);
            var DefendCannonRushSequence = new DefendCannonRushSequence(lokiBot);
            var DefendGroundMeleeRushSequence = new DefendGroundMeleeRushSequence(lokiBot);

            var scvMicroController = new IndividualMicroController(lokiBot, lokiBot.SharkyAdvancedPathFinder, MicroPriority.JustLive, false);

            var builds = new Dictionary<string, ISharkyBuild>
            {
                [BasicOpener.Name()] = BasicOpener,
                [BattlecruiserRushSequence.Name()] = BattlecruiserRushSequence,
                [FastExpandProtossResponseSequence.Name()] = FastExpandProtossResponseSequence,
                [HighEarlyAggressionSequence.Name()] = HighEarlyAggressionSequence,
                [BasicEarlyGameSequence.Name()] = BasicEarlyGameSequence,
                [BasicMidGameSequence.Name()] = BasicMidGameSequence,
                [DefaultLateGameSequence.Name()] = DefaultLateGameSequence,
                [DefendAirRushSequence.Name()] = DefendAirRushSequence,
                [DefendCannonRushSequence.Name()] = DefendCannonRushSequence,
                [DefendGroundMeleeRushSequence.Name()] = DefendGroundMeleeRushSequence
            };
            var versusEverything = new List<List<string>>();
            versusEverything.Add(new List<string>());
            foreach (var build in builds)
            {
                versusEverything[0].Add(build.Value.Name());
            }
            var buildSequences = new Dictionary<string, List<List<string>>>
            {
                [Race.Terran.ToString()] = versusEverything,
                [Race.Zerg.ToString()] = versusEverything,
                [Race.Protoss.ToString()] = versusEverything,
                [Race.Random.ToString()] = versusEverything,
                ["Transition"] = versusEverything,
            };

            BuildChoices = new BuildChoices { Builds = builds, BuildSequences = buildSequences };
        }

        public BuildChoices BuildChoices { get; private set; }
    }
}