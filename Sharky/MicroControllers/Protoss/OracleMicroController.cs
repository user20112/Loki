﻿using SC2APIProtocol;
using Sharky.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers.Protoss
{
    public class OracleMicroController : IndividualMicroController
    {
        private float RevelationRadius = 6;
        private float RevelationRange = 9;

        public OracleMicroController(Sharky.LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled, float avoidDamageDistance = .5f)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled, avoidDamageDistance)
        {
        }

        public override List<SC2APIProtocol.Action> HarassWorkers(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame)
        {
            List<SC2APIProtocol.Action> action = null;

            var formation = GetDesiredFormation(commander);
            var bestTarget = GetBestHarassTarget(commander, target);

            if (PulsarBeam(commander, frame, bestTarget, out action)) { return action; }

            if (commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.ORACLEWEAPON))
            {
                if (WeaponReady(commander, frame))
                {
                    if (AttackBestTarget(commander, target, defensivePoint, null, bestTarget, frame, out action)) { return action; }
                }
                else
                {
                    if (Move(commander, target, defensivePoint, null, bestTarget, formation, frame, out action)) { return action; }
                }
            }

            return NavigateToPoint(commander, target, defensivePoint, null, frame);
            if (Move(commander, target, defensivePoint, null, bestTarget, formation, frame, out action)) { return action; }

            return commander.Order(frame, Abilities.MOVE, target);
        }

        public override List<SC2APIProtocol.Action> NavigateToPoint(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            List<SC2APIProtocol.Action> action = null;

            if (commander.UnitCalculation.NearbyEnemies.Count(e => e.DamageAir) > 0)
            {
                if (commander.RetreatPathFrame + 20 < frame)
                {
                    commander.RetreatPath = SharkyPathFinder.GetSafeAirPath(commander.UnitCalculation.Unit.Pos.X, commander.UnitCalculation.Unit.Pos.Y, target.X, target.Y, frame);
                    commander.RetreatPathFrame = frame;
                }

                if (FollowPath(commander, frame, out action)) { return action; }
            }

            if (AvoidTargettedDamage(commander, target, defensivePoint, frame, out action))
            {
                return action;
            }

            if (AvoidDamage(commander, target, defensivePoint, frame, out action))
            {
                return action;
            }

            NavigateToTarget(commander, target, groupCenter, null, Formation.Normal, frame, out action);

            return action;
        }

        protected override bool AttackBestTarget(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.Worker)))
            {
                commander.UnitCalculation.TargetPriorityCalculation.TargetPriority = TargetPriority.KillWorkers;

                var quickKill = commander.UnitCalculation.NearbyEnemies.Where(e => e.UnitClassifications.Contains(UnitClassification.Worker) && e.SimulatedHitpoints <= 5).OrderBy(e => Vector2.DistanceSquared(e.Position, commander.UnitCalculation.Position)).FirstOrDefault();
                if (quickKill != null && Vector2.DistanceSquared(quickKill.Position, commander.UnitCalculation.Position) < (commander.UnitCalculation.Range + 2) * (commander.UnitCalculation.Range + 2))
                {
                    action = commander.Order(frame, Abilities.ATTACK, targetTag: quickKill.Unit.Tag);
                    return true;
                }
            }

            if (AttackBestTargetInRange(commander, target, bestTarget, frame, out action))
            {
                return true;
            }

            return false;
        }

        protected override bool AttackBestTargetInRange(UnitCommander commander, Point2D target, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (bestTarget != null && (bestTarget.UnitClassifications.Contains(UnitClassification.Worker) || !commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.Worker))))
            {
                if (commander.UnitCalculation.EnemiesInRange.Any(e => e.Unit.Tag == bestTarget.Unit.Tag) && bestTarget.Unit.DisplayType == DisplayType.Visible)
                {
                    if (bestTarget.Unit.Health + bestTarget.Unit.Shield < bestTarget.PreviousUnit.Health + bestTarget.PreviousUnit.Shield)
                    {
                        if (!commander.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)Abilities.ATTACK_ATTACK && o.TargetWorldSpacePos != null))
                        {
                            action = commander.Order(frame, Abilities.ATTACK, new Point2D { X = bestTarget.Position.X, Y = bestTarget.Position.Y });
                        }
                        else
                        {
                            action = commander.Order(frame, Abilities.ATTACK, null, bestTarget.Unit.Tag);
                        }
                        commander.LastInRangeAttackFrame = frame;
                        return true;
                    }
                    else
                    {
                        if (commander.UnitCalculation.Unit.Orders.Any(o => o.TargetWorldSpacePos != null && o.AbilityId == (uint)Abilities.ATTACK_ATTACK))
                        {
                            action = commander.Order(frame, Abilities.ATTACK, new Point2D { X = bestTarget.Position.X, Y = bestTarget.Position.Y }, 0, true);
                        }
                        else
                        {
                            action = commander.Order(frame, Abilities.ATTACK, null, bestTarget.Unit.Tag);
                        }
                        commander.LastInRangeAttackFrame = frame;
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool DealWithCyclones(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            var lockOnRange = 7;
            var enemyCyclones = commander.UnitCalculation.NearbyEnemies.Where(u => u.Unit.UnitType == (uint)UnitTypes.TERRAN_CYCLONE && InRange(commander.UnitCalculation.Position, u.Position, commander.UnitCalculation.Unit.Radius + lockOnRange + 4));

            if (commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.LOCKON) || enemyCyclones.Any(c => c.EnemiesInRange.Count() < 2))
            {
                if (Retreat(commander, defensivePoint, defensivePoint, frame, out action)) { return true; }
            }

            return base.DealWithCyclones(commander, target, defensivePoint, frame, out action);
        }

        protected override bool MaintainRange(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            var range = 9f;
            if (commander.UnitRole == UnitRole.Harass)
            {
                range = commander.UnitCalculation.Weapon.Range;
            }
            var enemiesInRange = new List<UnitCalculation>();

            foreach (var enemyAttack in commander.UnitCalculation.NearbyEnemies)
            {
                if (DamageService.CanDamage(enemyAttack, commander.UnitCalculation) && InRange(commander.UnitCalculation.Position, enemyAttack.Position, range + commander.UnitCalculation.Unit.Radius + enemyAttack.Unit.Radius))
                {
                    enemiesInRange.Add(enemyAttack);
                }
            }

            var closestEnemy = enemiesInRange.OrderBy(u => Vector2.DistanceSquared(u.Position, commander.UnitCalculation.Position)).FirstOrDefault();
            if (closestEnemy == null)
            {
                return false;
            }

            var avoidPoint = GetPositionFromRange(commander, closestEnemy.Unit.Pos, commander.UnitCalculation.Unit.Pos, range + commander.UnitCalculation.Unit.Radius + closestEnemy.Unit.Radius);
            if (AvoidDeceleration(commander, avoidPoint, false, frame, out action)) { return true; }
            action = commander.Order(frame, Abilities.MOVE, avoidPoint);
            return true;
        }

        protected override bool MoveToAttackTarget(UnitCommander commander, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (WeaponReady(commander, frame))
            {
                var point = new Point2D { X = bestTarget.Position.X, Y = bestTarget.Position.Y };
                action = commander.Order(frame, Abilities.MOVE, point);
                return true;
            }

            var attackPoint = GetPositionFromRange(commander, bestTarget.Unit.Pos, commander.UnitCalculation.Unit.Pos, commander.UnitCalculation.Range + bestTarget.Unit.Radius + commander.UnitCalculation.Unit.Radius);
            if (AvoidDeceleration(commander, attackPoint, false, frame, out action)) { return true; }
            action = commander.Order(frame, Abilities.MOVE, attackPoint);
            return true;
        }

        protected override bool OffensiveAbility(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (PulsarBeam(commander, frame, bestTarget, out action))
            {
                return true;
            }

            if (Revelation(commander, frame, out action))
            {
                return true;
            }

            if (StasisWard(commander, frame, bestTarget, out action))
            {
                return true;
            }

            return false;
        }

        protected override bool PreOffenseOrder(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            commander.SkipFrame = false;

            var cloakedPosition = CloakedInvader(commander);
            if (cloakedPosition != null && commander.UnitCalculation.Unit.Energy >= 25)
            {
                action = commander.Order(frame, Abilities.EFFECT_ORACLEREVELATION, cloakedPosition);
                return true;
            }

            var order = commander.UnitCalculation.Unit.Orders.FirstOrDefault(o => o.AbilityId == (uint)Abilities.EFFECT_ORACLEREVELATION && o.TargetWorldSpacePos != null);
            if (order != null && commander.UnitCalculation.Unit.Shield == commander.UnitCalculation.Unit.ShieldMax && commander.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)Abilities.EFFECT_ORACLEREVELATION))
            {
                if (commander.UnitCalculation.Unit.Shield > commander.UnitCalculation.Unit.ShieldMax / 2.0)
                {
                    return true;
                }

                if (Revelation(commander, frame, out action))
                {
                    return true;
                }
            }

            if (AvoidTargettedDamage(commander, target, defensivePoint, frame, out action))
            {
                return true;
            }

            if (AvoidDamage(commander, target, defensivePoint, frame, out action))
            {
                return true;
            }

            return false;
        }

        protected override bool WeaponReady(UnitCommander commander, int frame)
        {
            if (commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.ORACLEWEAPON))
            {
                return true;
            }
            return false;
        }

        private Point2D CloakedInvader(UnitCommander commander)
        {
            var pos = commander.UnitCalculation.Position;

            if (commander.UnitRole != UnitRole.Defend)
            {
                var hiddenUnits = ActiveUnitData.EnemyUnits.Values.Where(e => e.Unit.DisplayType == DisplayType.Hidden).OrderBy(e => Vector2.DistanceSquared(pos, e.Position));
                var hiddenByAllies = hiddenUnits.FirstOrDefault(e => e.NearbyEnemies.Any(e => !e.Unit.IsHallucination));
                if (hiddenByAllies != null)
                {
                    return new Point2D { X = hiddenByAllies.Unit.Pos.X, Y = hiddenByAllies.Unit.Pos.Y };
                }
            }

            var unit = ActiveUnitData.SelfUnits.Values.Where(a => a.Unit.UnitType == (uint)UnitTypes.PROTOSS_NEXUS).SelectMany(a => a.NearbyEnemies).Where(e => SharkyUnitData.CloakableAttackers.Contains((UnitTypes)e.Unit.UnitType) && !e.Unit.BuffIds.Contains((uint)Buffs.ORACLEREVELATION)).OrderBy(e => Vector2.DistanceSquared(pos, e.Position)).FirstOrDefault();
            if (unit != null)
            {
                return new Point2D { X = unit.Unit.Pos.X, Y = unit.Unit.Pos.Y };
            }

            return null;
        }

        private bool DeactivatePulsarBeam(UnitCommander commander, int frame, UnitCalculation bestTarget, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (!commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.ORACLEWEAPON))
            {
                return false;
            }

            if (bestTarget == null || !commander.UnitCalculation.NearbyEnemies.Any(e => e.Unit.Tag == bestTarget.Unit.Tag))
            {
                action = commander.Order(frame, Abilities.BEHAVIOR_PULSARBEAMOFF);
                return true;
            }

            return false;
        }

        private Point2D GetBestRevelationLocation(UnitCommander commander)
        {
            var enemiesInRange = commander.UnitCalculation.NearbyEnemies.Take(25).Where(e => !e.Attributes.Contains(SC2APIProtocol.Attribute.Structure) && Vector2.DistanceSquared(e.Position, commander.UnitCalculation.Position) < RevelationRange * RevelationRange);

            if (enemiesInRange.Count() == 0)
            {
                enemiesInRange = commander.UnitCalculation.NearbyEnemies.Take(25).Where(e => !e.Attributes.Contains(SC2APIProtocol.Attribute.Structure));
                if (enemiesInRange.Count() == 0)
                {
                    return null;
                }
            }

            var hitCounts = new Dictionary<Point, float>();
            foreach (var enemyAttack in commander.UnitCalculation.NearbyEnemies.Take(25))
            {
                float hits = 0;
                foreach (var hitEnemy in enemiesInRange)
                {
                    if (Vector2.DistanceSquared(hitEnemy.Position, enemyAttack.Position) <= (hitEnemy.Unit.Radius + RevelationRadius) * (hitEnemy.Unit.Radius + RevelationRadius))
                    {
                        hits += 1;
                    }
                }
                hitCounts[enemyAttack.Unit.Pos] = hits;
            }

            var position = hitCounts.OrderByDescending(x => x.Value).First().Key;

            return new Point2D { X = position.X, Y = position.Y }; // TODO: if there are any cloaked units, go for the spot that hits the most cloaked units
        }

        private bool PulsarBeam(UnitCommander commander, int frame, UnitCalculation bestTarget, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (DeactivatePulsarBeam(commander, frame, bestTarget, out action))
            {
                return true;
            }

            if (commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.ORACLEWEAPON) || commander.UnitCalculation.Unit.Energy < 50 || bestTarget == null)
            {
                return false;
            }

            if (commander.UnitCalculation.EnemiesInRange.Any(e => e.Unit.Tag == bestTarget.Unit.Tag))
            {
                action = commander.Order(frame, Abilities.BEHAVIOR_PULSARBEAMON);
                return true;
            }

            return false;
        }

        private bool Revelation(UnitCommander commander, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (commander.UnitCalculation.Unit.Energy < 25 || !commander.AbilityOffCooldown(Abilities.EFFECT_ORACLEREVELATION, frame, SharkyOptions.FramesPerSecond, SharkyUnitData) || commander.UnitCalculation.Unit.BuffIds.Contains((uint)Buffs.ORACLEWEAPON))
            {
                return false;
            }

            var cloackedPosition = CloakedInvader(commander);
            if (cloackedPosition != null)
            {
                action = commander.Order(frame, Abilities.EFFECT_ORACLEREVELATION, cloackedPosition);
                return true;
            }

            if (commander.UnitCalculation.NearbyEnemies.Any(e => e.Unit.BuffIds.Contains((uint)Buffs.ORACLEREVELATION)))
            {
                return false; // TODO: unless a unit is invisible
            }

            var revelationLocation = GetBestRevelationLocation(commander);
            if (revelationLocation != null)
            {
                action = commander.Order(frame, Abilities.EFFECT_ORACLEREVELATION, revelationLocation);
                return true;
            }

            return false;
        }

        private bool StasisWard(UnitCommander commander, int frame, UnitCalculation bestTarget, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            return false; // TODO:  stasis ward, put stasis wards on the tops of ramps
        }
    }
}