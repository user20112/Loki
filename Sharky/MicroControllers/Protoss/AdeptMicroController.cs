﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.MicroControllers.Protoss
{
    public class AdeptMicroController : IndividualMicroController
    {
        public AdeptMicroController(Sharky.LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
            AvoidDamageDistance = 2;
        }

        public override List<Action> NavigateToPoint(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            List<SC2APIProtocol.Action> action = null;

            var bestTarget = GetBestHarassTarget(commander, target);

            if (SpecialCaseMove(commander, target, defensivePoint, null, bestTarget, Formation.Normal, frame, out action)) { return action; }
            if (PreOffenseOrder(commander, target, defensivePoint, null, bestTarget, frame, out action)) { return action; }
            if (AvoidTargettedOneHitKills(commander, target, defensivePoint, frame, out action)) { return action; }
            if (OffensiveAbility(commander, target, defensivePoint, null, bestTarget, frame, out action)) { return action; }

            if (WeaponReady(commander, frame))
            {
                if (AttackBestTargetInRange(commander, target, bestTarget, frame, out action)) { return action; }
            }

            if (AvoidAllDamage(commander, target, defensivePoint, frame, out action)) { return action; }

            return commander.Order(frame, Abilities.MOVE, target);
        }

        protected override bool PreOffenseOrder(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (target == defensivePoint && commander.UnitCalculation.NearbyEnemies.Count() == 0)
            {
                return false;
            }

            if (commander.AbilityOffCooldown(Abilities.EFFECT_ADEPTPHASESHIFT, frame, SharkyOptions.FramesPerSecond, SharkyUnitData))
            {
                action = commander.Order(frame, Abilities.EFFECT_ADEPTPHASESHIFT, target);
                return true;
            }

            return false;
        }
    }
}