using Sharky.Pathing;

namespace Sharky.MicroControllers.Protoss
{
    public class ObserverMicroController : FlyingDetectorMicroController
    {
        public ObserverMicroController(Sharky.LokiBot.LokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }
    }
}