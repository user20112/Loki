﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers.Protoss
{
    public class HighTemplarMicroController : IndividualMicroController
    {
        private int FeedbackRangeSquared = 121;

        // actually range 10, but give an extra 1 range to get first feedback in
        private int lastStormFrame = 0;

        private double StormRadius = 1.5;
        private int StormRangeSquared = 82;

        public HighTemplarMicroController(Sharky.LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }

        public override List<SC2APIProtocol.Action> Retreat(UnitCommander commander, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            List<Action> actions = null;
            if (OffensiveAbility(commander, defensivePoint, defensivePoint, groupCenter, null, frame, out actions))
            {
                return actions;
            }

            return base.Retreat(commander, defensivePoint, groupCenter, frame);
        }

        protected override bool OffensiveAbility(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (Storm(commander, frame, out action))
            {
                return true;
            }

            if (Feedback(commander, frame, out action))
            {
                return true;
            }

            if (Merge(commander, frame, out action))
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

            if (commander.UnitCalculation.Unit.Energy < 40 ||
                (!SharkyUnitData.ResearchedUpgrades.Contains((uint)Upgrades.PSISTORMTECH) && !commander.UnitCalculation.NearbyEnemies.Any(e => e.Unit.Energy > 10) && // stay in the back if can't use spells on anything
                commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.ArmyUnit)) && commander.UnitCalculation.NearbyAllies.Count(a => a.UnitClassifications.Contains(UnitClassification.ArmyUnit)) > 2))
            {
                if (Retreat(commander, target, defensivePoint, frame, out action)) { return true; }
            }

            return false;
        }

        private bool Feedback(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.Unit.Energy < 50)
            {
                return false;
            }

            var vector = commander.UnitCalculation.Position;
            var enemiesInRange = commander.UnitCalculation.NearbyEnemies.Take(25).Where(e => e.Unit.Energy > 1 && e.Unit.DisplayType == DisplayType.Visible && Vector2.DistanceSquared(e.Position, vector) < FeedbackRangeSquared).OrderByDescending(e => e.Unit.Energy);

            var oneShotKill = enemiesInRange.Where(e => e.Unit.Energy * .5 > e.Unit.Health + e.Unit.Shield).FirstOrDefault();
            if (oneShotKill != null)
            {
                action = commander.Order(frame, Abilities.EFFECT_FEEDBACK, null, oneShotKill.Unit.Tag);
                return true;
            }
            var target = enemiesInRange.FirstOrDefault(e => e.Unit.UnitType != (uint)UnitTypes.ZERG_OVERSEER && e.Unit.Energy > 50);
            if (commander.UnitCalculation.TargetPriorityCalculation.TargetPriority == TargetPriority.KillDetection)
            {
                var detector = enemiesInRange.FirstOrDefault(e => e.UnitClassifications.Contains(UnitClassification.Detector));
                if (detector != null)
                {
                    target = detector;
                }
            }

            if (target != null && target.Unit.Energy > 50)
            {
                action = commander.Order(frame, Abilities.EFFECT_FEEDBACK, null, target.Unit.Tag);
                return true;
            }

            return false;
        }

        private Point2D GetBestAttack(UnitCalculation potentialAttack, IEnumerable<UnitCalculation> enemies)
        {
            var killCounts = new Dictionary<Point, float>();
            foreach (var enemyAttack in enemies)
            {
                int hitCount = 0;
                foreach (var splashedEnemy in enemyAttack.NearbyAllies.Take(25).Where(a => !a.Attributes.Contains(Attribute.Structure) && !a.Unit.BuffIds.Contains((uint)Buffs.PSISTORM)))
                {
                    if (Vector2.DistanceSquared(splashedEnemy.Position, enemyAttack.Position) < (splashedEnemy.Unit.Radius + StormRadius) * (splashedEnemy.Unit.Radius + StormRadius))
                    {
                        hitCount++;
                    }
                }
                foreach (var splashedAlly in potentialAttack.NearbyAllies.Take(25).Where(a => !a.Attributes.Contains(Attribute.Structure)))
                {
                    if (Vector2.DistanceSquared(splashedAlly.Position, enemyAttack.Position) < (splashedAlly.Unit.Radius + StormRadius) * (splashedAlly.Unit.Radius + StormRadius))
                    {
                        hitCount -= 3;
                    }
                }
                killCounts[enemyAttack.Unit.Pos] = hitCount;
            }

            var best = killCounts.OrderByDescending(x => x.Value).FirstOrDefault();

            if (best.Value < 3) // only attack if going to hit >= 3 units
            {
                return null;
            }
            return new Point2D { X = best.Key.X, Y = best.Key.Y };
        }

        private bool Merge(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.Unit.Energy > 40 || commander.UnitCalculation.NearbyEnemies.Count() == 0)
            {
                return false;
            }

            if (commander.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)Abilities.MORPH_ARCHON || o.AbilityId == (uint)Abilities.MORPH_ARCHON2))
            {
                return true;
            }

            var otherHighTemplar = commander.UnitCalculation.NearbyAllies.Take(25).Where(a => a.Unit.UnitType == (uint)UnitTypes.PROTOSS_HIGHTEMPLAR && a.Unit.Energy <= 40);

            if (otherHighTemplar.Count() > 0)
            {
                var target = otherHighTemplar.OrderBy(o => Vector2.DistanceSquared(o.Position, commander.UnitCalculation.Position)).FirstOrDefault();
                if (target != null)
                {
                    var merge = commander.Merge(target.Unit.Tag);
                    if (merge != null)
                    {
                        commander.UnitRole = UnitRole.Morph;
                        action = new List<Action> { merge };
                    }
                    return true;
                }
            }

            return false;
        }

        private bool Storm(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.Unit.Energy < 75 || !SharkyUnitData.ResearchedUpgrades.Contains((uint)Upgrades.PSISTORMTECH))
            {
                return false;
            }

            if (!commander.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)Abilities.EFFECT_PSISTORM))
            {
                if (!commander.AbilityOffCooldown(Abilities.EFFECT_PSISTORM, frame, SharkyOptions.FramesPerSecond, SharkyUnitData))
                {
                    return false;
                }

                if (lastStormFrame >= frame - 5)
                {
                    return false;
                }
            }

            var enemies = commander.UnitCalculation.NearbyEnemies.Take(25).Where(a => !a.Attributes.Contains(Attribute.Structure) && !a.Unit.BuffIds.Contains((uint)Buffs.PSISTORM) && !a.Unit.BuffIds.Contains((uint)Buffs.ORACLESTASISTRAPTARGET)).OrderBy(u => u.Unit.Health);
            if (enemies.Count() > 2)
            {
                var bestAttack = GetBestAttack(commander.UnitCalculation, enemies);
                if (commander.UnitCalculation.TargetPriorityCalculation.TargetPriority == TargetPriority.WinAir)
                {
                    var airAttackers = enemies.Where(u => u.DamageAir);
                    if (airAttackers.Count() > 0)
                    {
                        var air = GetBestAttack(commander.UnitCalculation, airAttackers);
                        if (air != null)
                        {
                            bestAttack = air;
                        }
                    }
                }
                else if (commander.UnitCalculation.TargetPriorityCalculation.TargetPriority == TargetPriority.WinGround)
                {
                    var groundAttackers = enemies.Where(u => u.DamageGround);
                    if (groundAttackers.Count() > 0)
                    {
                        var ground = GetBestAttack(commander.UnitCalculation, groundAttackers);
                        if (ground != null)
                        {
                            bestAttack = ground;
                        }
                    }
                }
                else
                {
                    if (enemies.Count() > 0)
                    {
                        var any = GetBestAttack(commander.UnitCalculation, enemies);
                        if (any != null)
                        {
                            bestAttack = any;
                        }
                    }
                }

                if (bestAttack != null)
                {
                    action = commander.Order(frame, Abilities.EFFECT_PSISTORM, bestAttack);
                    lastStormFrame = frame;
                    return true;
                }
            }

            return false;
        }
    }
}