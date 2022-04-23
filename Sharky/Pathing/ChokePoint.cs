using SC2APIProtocol;
using System.Collections.Generic;
using System.Numerics;

namespace Sharky.Pathing
{
    public class ChokePoint
    {
        public Vector2 Center { get; set; }
        public List<Point2D> Points { get; set; }
    }
}