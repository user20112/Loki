﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;

namespace Sharky.MicroControllers.Protoss
{
    public class InterceptorMicroController : IndividualMicroController
    {
        public InterceptorMicroController(LokiBot.BaseLokiBot lokiBot, IPathFinder pathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, pathFinder, microPriority, groupUpEnabled)
        {
        }

        public override List<Action> Attack(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            return null;
        }

        public override List<Action> Idle(UnitCommander commander, Point2D defensivePoint, int frame)
        {
            return null;
        }

        public override List<Action> Retreat(UnitCommander commander, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            return null;
        }

        public override List<Action> Support(UnitCommander commander, IEnumerable<UnitCommander> supportTargets, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            return null;
        }
    }
}