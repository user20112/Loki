﻿using SC2APIProtocol;
using Sharky.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.MicroControllers.Terran
{
    public class VikingMicroController : IndividualMicroController
    {
        public VikingMicroController(Sharky.LokiBot.LokiBot lokiBot, IPathFinder sharkyPathFinder, MicroPriority microPriority, bool groupUpEnabled)
            : base(lokiBot, sharkyPathFinder, microPriority, groupUpEnabled)
        {
        }

        protected override Point2D GetSupportSpot(UnitCommander commander, UnitCommander unitToSupport, Point2D target, Point2D defensivePoint)
        {
            var angle = Math.Atan2(unitToSupport.UnitCalculation.Position.Y - target.Y, target.X - unitToSupport.UnitCalculation.Position.X);
            var nearestEnemy = unitToSupport.UnitCalculation.EnemiesInRange.FirstOrDefault();
            if (nearestEnemy != null)
            {
                angle = Math.Atan2(unitToSupport.UnitCalculation.Position.Y - nearestEnemy.Position.Y, nearestEnemy.Position.X - unitToSupport.UnitCalculation.Position.X);
            }
            var x = 10f * Math.Cos(angle);
            var y = 10f * Math.Sin(angle);

            var supportPoint = new Point2D { X = unitToSupport.UnitCalculation.Position.X + (float)x, Y = unitToSupport.UnitCalculation.Position.Y - (float)y };

            return supportPoint;
        }

        protected override bool OffensiveAbility(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out List<SC2APIProtocol.Action> action)
        {
            action = null;

            if (commander.UnitCalculation.NearbyEnemies.Count() > 0 &&
                !commander.UnitCalculation.NearbyEnemies.Any(e => e.Unit.IsFlying) &&
                !commander.UnitCalculation.NearbyEnemies.Any(e => e.DamageGround && e.UnitClassifications.Any(c => c == UnitClassification.ArmyUnit || c == UnitClassification.DefensiveStructure) || MapDataService.MapHeight(e.Unit.Pos) != MapDataService.MapHeight(commander.UnitCalculation.Unit.Pos)))
            {
                action = commander.Order(frame, Abilities.MORPH_VIKINGASSAULTMODE);
                return true;
            }

            return false;
        }
    }
}