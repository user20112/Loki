﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.MicroControllers.Protoss
{
    public class SentryMicroController : IndividualMicroController
    {
        public SentryMicroController(LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }

        protected override bool OffensiveAbility(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (GuardianShield(commander, frame, out action))
            {
                return true;
            }

            if (Hallucinate(commander, frame, out action))
            {
                return true;
            }

            return false;
        }

        protected override bool PreOffenseOrder(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (OffensiveAbility(commander, target, defensivePoint, groupCenter, bestTarget, frame, out action)) { return true; }

            if (commander.UnitCalculation.Unit.Shield < 20)
            {
                if (AvoidDamage(commander, target, defensivePoint, frame, out action))
                {
                    return true;
                }
            }

            return false;
        }

        private bool GuardianShield(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.GUARDIANSHIELD) || commander.UnitCalculation.Unit.Energy < 75)
            {
                return false;
            }

            if (commander.UnitCalculation.EnemiesInRangeOf.Count(e => e.Range > 1) > 3)
            {
                action = commander.Order(frame, Abilities.EFFECT_GUARDIANSHIELD);
                return true;
            }
            return false;
        }

        private bool Hallucinate(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.Unit.Energy < 75)
            {
                return false;
            }

            var height = MapDataService.MapHeight(commander.UnitCalculation.Unit.Pos);
            if (!commander.UnitCalculation.NearbyAllies.Take(25).Any(a => a.Unit.IsFlying || a.Unit.UnitType == (uint)UnitTypes.PROTOSS_COLOSSUS))
            {
                if (commander.UnitCalculation.NearbyEnemies.Take(25).Any(e => e.UnitClassifications.Contains(UnitClassification.ArmyUnit) && MapDataService.MapHeight(e.Unit.Pos) > height))
                {
                    action = commander.Order(frame, Abilities.HALLUCINATION_COLOSSUS);
                    return true;
                }
            }

            if (commander.UnitCalculation.NearbyEnemies.Take(25).Count(e => e.UnitClassifications.Contains(UnitClassification.ArmyUnit)) > 3 && !commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.Detector)))
            {
                action = commander.Order(frame, Abilities.HALLUCINATION_ARCHON);
                return true;
            }
            return false;
        }
    }
}