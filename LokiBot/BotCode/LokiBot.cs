using Sharky;
using Sharky.LokiBot;

namespace LokiBot.BotCode
{
    public class LokiBot : BaseLokiBot
    {
        public LokiBot(GameConnection gameConnection) : base(gameConnection, false)
        {
            BuildManager = new LokiBuildManager(BuildChoices, DebugService, MacroBalancer, BuildDecisionService, EnemyPlayerService, ChatHistory, EnemyStrategyHistory, FrameToTimeConverter, SharkyOptions, ChatService, SimCityService);
            Managers.Add(BuildManager);
        }
    }
}