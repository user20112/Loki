using Sharky.Pathing;

namespace Sharky.MicroControllers.Protoss
{
    public class ObserverMicroController : FlyingDetectorMicroController
    {
        public ObserverMicroController(Sharky.LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }
    }
}