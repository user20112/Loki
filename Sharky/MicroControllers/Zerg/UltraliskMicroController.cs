using Sharky.Pathing;

namespace Sharky.MicroControllers.Zerg
{
    public class UltraliskMicroController : IndividualMicroController
    {
        public UltraliskMicroController(Sharky.LokiBot.LokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }
    }
}