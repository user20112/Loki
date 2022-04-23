﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;

namespace Sharky.MicroControllers.Terran
{
    public class ThorMicroController : IndividualMicroController
    {
        public ThorMicroController(LokiBot.LokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }

        protected override bool AvoidPointlessDamage(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            return false;
        }

        protected override bool WeaponReady(UnitCommander commander, int frame)
        {
            return commander.UnitCalculation.Unit.WeaponCooldown == 0 || commander.UnitCalculation.Unit.WeaponCooldown > 2; // a thor has multiple attacks, don't cancel the animation early
        }
    }
}