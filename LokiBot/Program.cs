using SC2APIProtocol;
using Sharky;
using System;
using System.Threading.Tasks;

namespace LokiBot
{
    internal class Program
    {
        private static bool Testing = true;

        public static async Task RunMultiPlayer(GameConnection BotConnection, GameConnection PlayerConnection, SharkyBot bot, Race BotRace, Race PlayerRace, string map, SharkyBot Bot2 = null)
        {
            PlayerConnection.readSettings();
            BotConnection.readSettings();
            PlayerConnection.StartSC2Instance(8165);
            BotConnection.StartSC2Instance(8170);
            await PlayerConnection.Connect(8165);
            await BotConnection.Connect(8170);
            await PlayerConnection.CreateGame(map, BotRace, PlayerRace);
            Task.WaitAll(Task.Run(() =>
           {
               PlayerConnection.JoinGame(PlayerRace, true).Wait();
           }),
            Task.Run(() =>
              {
                  var BottaskId = BotConnection.JoinGame(BotRace, false);
                  BottaskId.Wait();
                  BotConnection.Run(bot, BottaskId.Result, "Test").Wait();
              }));
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Loki Bot");
            var BotGameConnection = new GameConnection();
            var PlayerGameConnection = new GameConnection();
            var lokiBot = new LokiBot.BotCode.LokiBot(BotGameConnection);
            var terranBuildChoices = new MyBuildChoices(lokiBot);
            lokiBot.BuildChoices[Race.Terran] = terranBuildChoices.BuildChoices;
            var sharkyExampleBot = lokiBot.CreateBot(lokiBot.Managers, lokiBot.DebugService);
            if (!Testing)
                RunMultiPlayer(BotGameConnection, PlayerGameConnection, sharkyExampleBot, Race.Terran, Race.Protoss, @"LightshadeLE.SC2Map").Wait();
            else
                BotGameConnection.RunSinglePlayer(sharkyExampleBot, @"LightshadeLE.SC2Map", Race.Terran, Race.Protoss, Difficulty.VeryHard, AIBuild.RandomBuild).Wait();
        }
    }
}