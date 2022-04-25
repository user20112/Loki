﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers.Terran
{
    public class ScvMicroController : IndividualMicroController
    {
        private MacroData MacroData;

        public ScvMicroController(LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
            MacroData = lokiBot.MacroData;
        }

        public override List<SC2APIProtocol.Action> Idle(UnitCommander commander, Point2D defensivePoint, int frame)
        {
            List<SC2APIProtocol.Action> action = null;
            UpdateState(commander, defensivePoint, defensivePoint, null, null, Formation.Normal, frame);
            if (Repair(commander, null, frame, out action)) { return action; }
            return action;
        }

        public override List<SC2APIProtocol.Action> Support(UnitCommander commander, IEnumerable<UnitCommander> supportTargets, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            List<SC2APIProtocol.Action> action = null;

            if ((commander.UnitCalculation.Unit.Health < commander.UnitCalculation.Unit.HealthMax / 4) ||
                (commander.UnitCalculation.Unit.Health < commander.UnitCalculation.Unit.HealthMax && commander.UnitCalculation.EnemiesInRangeOfAvoid.Count(e => e.EnemiesInRangeOf.Count() == 0) > 0))
            {
                return Retreat(commander, defensivePoint, groupCenter, frame);
            }

            if (Repair(commander, supportTargets, frame, out action)) { return action; }

            return base.Support(commander, supportTargets, target, defensivePoint, groupCenter, frame);
        }

        protected override bool OffensiveAbility(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (commander.UnitRole == UnitRole.Support)
            {
                if (Repair(commander, null, frame, out action)) { return true; }
            }

            return false;
        }

        protected bool Repair(UnitCommander commander, IEnumerable<UnitCommander> supportTargets, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (MacroData.Minerals < 5) { return false; }

            IOrderedEnumerable<UnitCalculation> repairTargets = null;
            if (supportTargets != null)
            {
                repairTargets = supportTargets.Select(c => c.UnitCalculation).Where(a => a.Attributes.Contains(Attribute.Mechanical) && a.Unit.BuildProgress == 1 && a.Unit.Health < a.Unit.HealthMax).OrderByDescending(a => a.Unit.HealthMax - a.Unit.Health);
            }
            if (repairTargets == null || repairTargets.Count() == 0)
            {
                repairTargets = commander.UnitCalculation.NearbyAllies.Take(25).Where(a => a.Attributes.Contains(Attribute.Mechanical) && a.Unit.BuildProgress == 1 && a.Unit.Health < a.Unit.HealthMax).OrderByDescending(a => a.Unit.HealthMax - a.Unit.Health);
            }

            var repairTarget = repairTargets.FirstOrDefault(a => Vector2.DistanceSquared(a.Position, commander.UnitCalculation.Position) <= (a.Unit.Radius + commander.UnitCalculation.Unit.Radius + commander.UnitCalculation.Range) * (a.Unit.Radius + commander.UnitCalculation.Unit.Radius + commander.UnitCalculation.Range));
            if (repairTarget == null)
            {
                repairTarget = repairTargets.FirstOrDefault();
            }

            if (repairTarget != null)
            {
                action = commander.Order(frame, Abilities.EFFECT_REPAIR, targetTag: repairTarget.Unit.Tag);
                return true;
            }

            return false;
        }
    }
}