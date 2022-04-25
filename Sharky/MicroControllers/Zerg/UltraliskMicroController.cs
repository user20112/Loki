using Sharky.Pathing;

namespace Sharky.MicroControllers.Zerg
{
    public class UltraliskMicroController : IndividualMicroController
    {
        public UltraliskMicroController(Sharky.LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }
    }
}