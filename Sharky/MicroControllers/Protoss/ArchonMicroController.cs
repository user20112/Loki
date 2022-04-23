﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers.Protoss
{
    public class ArchonMicroController : IndividualMicroController
    {
        public ArchonMicroController(Sharky.LokiBot.LokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }

        protected override bool AvoidTargettedDamage(UnitCommander commander, Point2D target, Point2D defensivePoint, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (commander.UnitCalculation.Unit.Shield > 0)
            {
                return false;
            }

            return base.AvoidTargettedDamage(commander, target, defensivePoint, frame, out action);
        }

        protected override UnitCalculation GetBestDpsReduction(UnitCommander commander, Weapon weapon, IEnumerable<UnitCalculation> primaryTargets, IEnumerable<UnitCalculation> secondaryTargets)
        {
            float splashRadius = 1f;
            var dpsReductions = new Dictionary<ulong, float>();
            foreach (var enemyAttack in primaryTargets)
            {
                float dpsReduction = 0;
                foreach (var splashedEnemy in secondaryTargets)
                {
                    if (Vector2.DistanceSquared(splashedEnemy.Position, enemyAttack.Position) < (splashedEnemy.Unit.Radius + splashRadius) * (splashedEnemy.Unit.Radius + splashRadius))
                    {
                        dpsReduction += splashedEnemy.Dps / TimeToKill(weapon, splashedEnemy.Unit, SharkyUnitData.UnitData[(UnitTypes)splashedEnemy.Unit.UnitType]);
                    }
                }
                dpsReductions[enemyAttack.Unit.Tag] = dpsReduction;
            }

            var best = dpsReductions.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            return primaryTargets.FirstOrDefault(t => t.Unit.Tag == best);
        }

        protected override bool WeaponReady(UnitCommander commander, int frame)
        {
            return commander.UnitCalculation.Unit.WeaponCooldown < 5;
        }
    }
}