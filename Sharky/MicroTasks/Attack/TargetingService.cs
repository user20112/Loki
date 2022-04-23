﻿using SC2APIProtocol;
using Sharky.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroTasks.Attack
{
    public class TargetingService
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private int EnemyBuildingCount = 0;
        private MapDataService MapDataService;
        private TargetingData TargetingData;

        public TargetingService(ActiveUnitData activeUnitData, MapDataService mapDataService, BaseData baseData, TargetingData targetingData)
        {
            ActiveUnitData = activeUnitData;
            MapDataService = mapDataService;
            BaseData = baseData;
            TargetingData = targetingData;
        }

        public Point2D GetArmyPoint(IEnumerable<UnitCommander> armyUnits, float trimRangeSquared = 100)
        {
            var vectors = armyUnits.Select(u => u.UnitCalculation.Position);
            return GetArmyPoint(vectors, trimRangeSquared);
        }

        public Point2D GetArmyPoint(IEnumerable<UnitCalculation> armyUnits, float trimRangeSquared = 100)
        {
            var vectors = armyUnits.Select(u => u.Position);
            return GetArmyPoint(vectors, trimRangeSquared);
        }

        public Point2D UpdateAttackPoint(Point2D armyPoint, Point2D attackPoint)
        {
            var enemyBuildings = ActiveUnitData.EnemyUnits.Where(e => e.Value.UnitTypeData.Attributes.Contains(SC2APIProtocol.Attribute.Structure) && e.Value.Unit.UnitType != (uint)UnitTypes.ZERG_CREEPTUMOR && e.Value.Unit.UnitType != (uint)UnitTypes.ZERG_CREEPTUMORBURROWED && e.Value.Unit.UnitType != (uint)UnitTypes.ZERG_CREEPTUMORQUEEN);
            var currentEnemyBuildingCount = enemyBuildings.Count();

            if (MapDataService.SelfVisible(attackPoint) || EnemyBuildingCount != currentEnemyBuildingCount)
            {
                TargetingData.HiddenEnemyBase = false;
                EnemyBuildingCount = currentEnemyBuildingCount;

                var armyVector = new Vector2(armyPoint.X, armyPoint.Y);

                var enemyBuilding = ActiveUnitData.EnemyUnits.Where(e => e.Value.Unit.UnitType != (uint)UnitTypes.ZERG_CREEPTUMORBURROWED && e.Value.Unit.UnitType != (uint)UnitTypes.ZERG_CREEPTUMOR && e.Value.UnitTypeData.Attributes.Contains(SC2APIProtocol.Attribute.Structure)).OrderBy(e => Vector2.DistanceSquared(e.Value.Position, armyVector)).FirstOrDefault().Value;
                if (enemyBuilding != null)
                {
                    return new Point2D { X = enemyBuilding.Unit.Pos.X, Y = enemyBuilding.Unit.Pos.Y };
                }
                else
                {
                    attackPoint = TargetingData.EnemyMainBasePoint;
                }
            }

            if (currentEnemyBuildingCount == 0 && MapDataService.SelfVisible(attackPoint) && MapDataService.Visibility(TargetingData.EnemyMainBasePoint) > 0)
            {
                // can't find enemy base, choose a random base location
                TargetingData.HiddenEnemyBase = true;
                var bases = BaseData.BaseLocations.Where(b => !MapDataService.SelfVisible(b.Location));
                if (bases.Count() == 0)
                {
                    // find a random spot on the map and check there
                    return new Point2D { X = new Random().Next(0, MapDataService.MapData.MapWidth), Y = new Random().Next(0, MapDataService.MapData.MapHeight) };
                }
                else
                {
                    return bases.ToList()[new Random().Next(0, bases.Count())].Location;
                }
            }

            return attackPoint;
        }

        private Point2D GetArmyPoint(IEnumerable<Vector2> vectors, float trimRangeSquared)
        {
            if (vectors.Count() > 0)
            {
                var average = new Vector2(vectors.Average(v => v.X), vectors.Average(v => v.Y));
                var trimmed = vectors.Where(v => Vector2.DistanceSquared(average, v) < trimRangeSquared);
                if (trimmed.Count() > 0)
                {
                    var trimmedAverage = new Point2D { X = trimmed.Average(v => v.X), Y = trimmed.Average(v => v.Y) };
                    return trimmedAverage;
                }
                else
                {
                    return new Point2D { X = average.X, Y = average.Y };
                }
            }
            else
            {
                return TargetingData.ForwardDefensePoint;
            }
        }
    }
}