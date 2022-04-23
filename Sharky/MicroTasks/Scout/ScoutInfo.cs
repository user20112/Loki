using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky.MicroTasks.Scout
{
    public class ScoutInfo
    {
        public List<UnitCommander> Harassers { get; set; }
        public int LastClearedFrame { get; set; }
        public int LastDefendedFrame { get; set; }
        public int LastPathFailedFrame { get; set; }
        public Point2D Location { get; set; }
    }
}