﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers.Zerg
{
    public class BanelingMicroController : IndividualMicroController
    {
        private EnemyData EnemyData;
        private int LastManualDetonationFrame;
        private float SplashRadius;

        public BanelingMicroController(LokiBot.BaseLokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
            EnemyData = lokiBot.EnemyData;
            AvoidDamageDistance = 5;
            SplashRadius = 2.2f - 0.375f;
            LastManualDetonationFrame = 0;
        }

        // TODO: banelings went dumb when there was a wall, tried to attack units on high ground instead of going to ramp and busting it first, make sure the best target is at the same height
        // TODO: if below certain number of units regroup and wait for more, or use the attackdatamanager to determine if should attack or not
        // TODO: while retreating if nearby enemies just attack
        // TODO: if no targets left follow enemy drones in case they gorup together, if no enemy drones left go to group center or nearest friendly unit that isn't a baneling

        protected override bool AttackBestTarget(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (AttackBestTargetInRange(commander, target, bestTarget, frame, out action))
            {
                return true;
            }

            if (bestTarget != null && MicroPriority != MicroPriority.NavigateToLocation && commander.UnitCalculation.NearbyEnemies.Any(e => e.Unit.Tag == bestTarget.Unit.Tag))
            {
                if (GetHighGroundVision(commander, target, defensivePoint, bestTarget, frame, out action)) { return true; }

                var enemyPosition = GetBestTargetAttackPoint(commander, bestTarget);

                action = commander.Order(frame, Abilities.MOVE, enemyPosition);
                return true;
            }

            if (GroupUpEnabled && GroupUp(commander, target, groupCenter, true, frame, out action))
            {
                return true;
            }

            if (bestTarget != null && MicroPriority != MicroPriority.NavigateToLocation)
            {
                var enemyPosition = GetBestTargetAttackPoint(commander, bestTarget);
                action = commander.Order(frame, Abilities.MOVE, enemyPosition);
                return true;
            }

            action = commander.Order(frame, Abilities.MOVE, target); // no damaging targets in range, attack towards the main target
            return true;
        }

        protected override bool AttackBestTargetInRange(UnitCommander commander, Point2D target, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (bestTarget != null)
            {
                // if do just as much or more damage by exploding in spot just explode
                List<UnitCalculation> hitUnits;
                List<UnitCalculation> hitSelfUnits;
                var targetDamage = SplashDamage(commander, commander.UnitCalculation.NearbyEnemies.Take(25).Where(u => AttackersFilter(commander, u)), bestTarget, out hitUnits);
                var selfDetonateDamage = SplashDamage(commander, commander.UnitCalculation.NearbyEnemies.Take(25).Where(u => AttackersFilter(commander, u)), commander.UnitCalculation, out hitSelfUnits);

                var detonateChoke = hitSelfUnits.Any(s => s.Attributes.Contains(Attribute.Structure)) && hitSelfUnits.Count() > 1;
                if (detonateChoke)
                {
                    if (EnemyData.EnemyRace == Race.Zerg)
                    {
                        detonateChoke = false;
                    }
                    if (EnemyData.EnemyRace == Race.Protoss && !hitSelfUnits.Any(s => s.Unit.UnitType == (uint)UnitTypes.PROTOSS_PYLON))
                    {
                        detonateChoke = false;
                    }
                }
                if (detonateChoke && !TargetingData.ChokePoints.Bad.Any(b => Vector2.DistanceSquared(b.Center, commander.UnitCalculation.Position) < 9))
                {
                    detonateChoke = false;
                }

                if (targetDamage > 35 || selfDetonateDamage > 35 || detonateChoke || bestTarget.UnitClassifications.Contains(UnitClassification.DefensiveStructure))
                {
                    if (frame > LastManualDetonationFrame + 3 &&
                        (selfDetonateDamage >= targetDamage || detonateChoke))
                    {
                        foreach (var enemy in hitSelfUnits)
                        {
                            enemy.IncomingDamage += GetDamage(commander.UnitCalculation.Weapon, enemy.Unit, SharkyUnitData.UnitData[(UnitTypes)enemy.Unit.UnitType]);
                        }
                        action = commander.Order(frame, Abilities.EFFECT_EXPLODE);
                        LastManualDetonationFrame = frame;
                        return true;
                    }

                    if (commander.UnitCalculation.EnemiesInRange.Any(e => e.Unit.Tag == bestTarget.Unit.Tag))
                    {
                        bestTarget.IncomingDamage += GetDamage(commander.UnitCalculation.Weapons, bestTarget.Unit, bestTarget.UnitTypeData);
                        if (bestTarget.Unit.DisplayType == DisplayType.Visible)
                        {
                            foreach (var enemy in hitUnits)
                            {
                                enemy.IncomingDamage += GetDamage(commander.UnitCalculation.Weapon, enemy.Unit, SharkyUnitData.UnitData[(UnitTypes)enemy.Unit.UnitType]);
                            }
                            action = commander.Order(frame, Abilities.ATTACK, null, bestTarget.Unit.Tag);
                        }
                        else
                        {
                            foreach (var enemy in hitUnits)
                            {
                                enemy.IncomingDamage += GetDamage(commander.UnitCalculation.Weapon, enemy.Unit, SharkyUnitData.UnitData[(UnitTypes)enemy.Unit.UnitType]);
                            }
                            action = commander.Order(frame, Abilities.MOVE, new Point2D { X = bestTarget.Unit.Pos.X, Y = bestTarget.Unit.Pos.Y });
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool AttackersFilter(UnitCommander commander, UnitCalculation enemyAttack)
        {
            if (enemyAttack.Unit.IsFlying || enemyAttack.Unit.UnitType == (uint)UnitTypes.ZERG_EGG)
            {
                return false;
            }

            return base.AttackersFilter(commander, enemyAttack);
        }

        protected override bool AvoidDamage(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;
            if (commander.UnitCalculation.EnemiesThreateningDamage.Count() > 1)
            {
                return false;
            }
            return base.AvoidDamage(commander, target, defensivePoint, frame, out action);
        }

        protected override UnitCalculation GetBestDpsReduction(UnitCommander commander, Weapon weapon, IEnumerable<UnitCalculation> primaryTargets, IEnumerable<UnitCalculation> secondaryTargets)
        {
            float outerSplashRadius = 2.2f;
            var dpsReductions = new Dictionary<ulong, float>();
            foreach (var enemyAttack in primaryTargets.Where(e => !e.Unit.IsFlying && AttackersFilter(commander, e)))
            {
                float totalDamage = 0;
                foreach (var splashedEnemy in secondaryTargets.Where(e => !e.Unit.IsFlying))
                {
                    if (Vector2.DistanceSquared(splashedEnemy.Position, enemyAttack.Position) < outerSplashRadius * outerSplashRadius)
                    {
                        totalDamage += GetDamage(weapon, splashedEnemy.Unit, SharkyUnitData.UnitData[(UnitTypes)splashedEnemy.Unit.UnitType]);
                    }
                }
                dpsReductions[enemyAttack.Unit.Tag] = totalDamage;
            }

            var best = dpsReductions.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            return primaryTargets.FirstOrDefault(t => t.Unit.Tag == best);
        }

        protected override UnitCalculation GetBestTarget(UnitCommander commander, Point2D target, int frame)
        {
            var existingAttackOrder = commander.UnitCalculation.Unit.Orders.Where(o => o.AbilityId == (uint)Abilities.ATTACK || o.AbilityId == (uint)Abilities.ATTACK_ATTACK).FirstOrDefault();

            var range = commander.UnitCalculation.Range;

            var attacks = commander.UnitCalculation.NearbyEnemies.Take(25).Where(u => AttackersFilter(commander, u));

            if (attacks.Count() > 0)
            {
                var damages = new Dictionary<ulong, float>();
                foreach (var enemyAttack in attacks.Where(e => !e.Unit.IsFlying))
                {
                    List<UnitCalculation> hitUnits;
                    float totalDamage = SplashDamage(commander, attacks, enemyAttack, out hitUnits);
                    damages[enemyAttack.Unit.Tag] = totalDamage;
                }

                var best = damages.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                commander.BestTarget = attacks.FirstOrDefault(t => t.Unit.Tag == best);
                return commander.BestTarget;
            }

            return null;
        }

        protected override bool Retreat(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame, out List<SC2APIProtocol.Action> action)
        {
            if (commander.UnitCalculation.EnemiesInRangeOf.Count(e => !e.Unit.IsFlying) > 3)
            {
                var bestTarget = GetBestTarget(commander, target, frame);
                if (AttackBestTarget(commander, target, defensivePoint, target, bestTarget, frame, out action)) { return true; }
            }
            return base.Retreat(commander, target, defensivePoint, frame, out action);
        }

        private float SplashDamage(UnitCommander commander, IEnumerable<UnitCalculation> attacks, UnitCalculation enemyAttack, out List<UnitCalculation> hitUnits)
        {
            hitUnits = new List<UnitCalculation>();
            float totalDamage = 0;
            foreach (var splashedEnemy in attacks.Where(e => AttackersFilter(commander, e)))
            {
                if (Vector2.DistanceSquared(splashedEnemy.Position, enemyAttack.Position) < (splashedEnemy.Unit.Radius + SplashRadius) * (splashedEnemy.Unit.Radius + SplashRadius))
                {
                    hitUnits.Add(splashedEnemy);
                    totalDamage += GetDamage(commander.UnitCalculation.Weapon, splashedEnemy.Unit, SharkyUnitData.UnitData[(UnitTypes)splashedEnemy.Unit.UnitType]);
                }
            }

            return totalDamage;
        }
    }
}