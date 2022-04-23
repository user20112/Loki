﻿using SC2APIProtocol;
using System;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.BuildingPlacement
{
    public class MissileTurretPlacement : IBuildingPlacement
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private BuildingService BuildingService;

        public MissileTurretPlacement(Sharky.LokiBot.LokiBot lokiBot)
        {
            BaseData = lokiBot.BaseData;
            BuildingService = lokiBot.BuildingService;
            ActiveUnitData = lokiBot.ActiveUnitData;
        }

        public Point2D FindPlacement(Point2D target, UnitTypes unitType, int size, bool ignoreResourceProximity = false, float maxDistance = 50, bool requireSameHeight = false, WallOffType wallOffType = WallOffType.None, bool requireVision = false, bool allowBlockBase = true)
        {
            var x = target.X;
            var y = target.Y;
            var reference = new Vector2(x, y);
            var nearestBase = BaseData.BaseLocations.OrderBy(b => Vector2.DistanceSquared(new Vector2(b.Location.X, b.Location.Y), reference)).FirstOrDefault();

            if (nearestBase != null && nearestBase.MineralLineLocation != null && nearestBase.MineralFields != null && nearestBase.MineralFields.Count() > 0)
            {
                var referenceAngle = GetAngle(nearestBase.Location.X, nearestBase.Location.Y, nearestBase.MineralLineLocation.X, nearestBase.MineralLineLocation.Y);

                var right = nearestBase.MineralFields.OrderByDescending(m => GetAngle(nearestBase.Location.X, nearestBase.Location.Y, m.Pos.X, m.Pos.Y)).FirstOrDefault();
                var left = nearestBase.MineralFields.OrderBy(m => GetAngle(nearestBase.Location.X, nearestBase.Location.Y, m.Pos.X, m.Pos.Y)).FirstOrDefault();

                if (right == null || left == null)
                {
                    return null;
                }

                if (Vector2.DistanceSquared(new Vector2(left.Pos.X, left.Pos.Y), new Vector2(right.Pos.X, right.Pos.Y)) < 9)
                {
                    var rights = nearestBase.MineralFields.Where(m => CrossProduct(m.Pos.X, m.Pos.Y, nearestBase.MineralLineLocation.X, nearestBase.MineralLineLocation.Y) > 0);
                    var lefts = nearestBase.MineralFields.Where(m => CrossProduct(m.Pos.X, m.Pos.Y, nearestBase.MineralLineLocation.X, nearestBase.MineralLineLocation.Y) < 0);

                    right = rights.OrderByDescending(m => AngleDifference(nearestBase.Location, m.Pos, referenceAngle)).FirstOrDefault();
                    left = lefts.OrderBy(m => AngleDifference(nearestBase.Location, m.Pos, referenceAngle)).FirstOrDefault();
                }

                var rightAngle = GetAngle(nearestBase.Location.X, nearestBase.Location.Y, right.Pos.X, right.Pos.Y) + .25;
                var placement = GetPlacement(rightAngle, nearestBase, reference, maxDistance);
                if (placement == null)
                {
                    rightAngle += .25;
                    placement = GetPlacement(rightAngle, nearestBase, reference, maxDistance);
                }
                if (placement != null)
                {
                    return placement;
                }
                var leftAngle = GetAngle(nearestBase.Location.X, nearestBase.Location.Y, left.Pos.X, left.Pos.Y) - .25;
                placement = GetPlacement(leftAngle, nearestBase, reference, maxDistance);
                if (placement == null)
                {
                    leftAngle -= .25;
                    placement = GetPlacement(leftAngle, nearestBase, reference, maxDistance);
                }
                if (placement != null)
                {
                    return placement;
                }
            }

            return null;
        }

        private double AngleDifference(Point2D baseLocation, Point mineralPoint, double referenceAngle)
        {
            var angle = GetAngle(baseLocation.X, baseLocation.Y, mineralPoint.X - .75f, mineralPoint.Y - .75f);

            var difference = angle - referenceAngle;

            return difference;
        }

        private float CrossProduct(float x1, float y1, float x2, float y2)
        {
            return (x1 * y2) - (y1 * x2);
        }

        private double GetAngle(float x, float y, float x2, float y2)
        {
            return Math.Atan2(y2 - y, x - x2);
        }

        private Point2D GetPlacement(double angle, BaseLocation nearestBase, Vector2 reference, float maxDistance)
        {
            for (var distance = 6; distance >= 4; distance--)
            {
                var xDif = -distance * Math.Cos(angle);
                var yDif = -distance * Math.Sin(angle);

                var point = new Point2D { X = (float)Math.Round(nearestBase.Location.X + xDif), Y = (float)Math.Round(nearestBase.Location.Y - yDif) };

                if (BuildingService.AreaBuildable(point.X, point.Y, .5f) && !BuildingService.Blocked(point.X, point.Y, .5f, .2f) && !BuildingService.HasAnyCreep(point.X, point.Y, .5f))
                {
                    if (Vector2.DistanceSquared(new Vector2(reference.X, reference.Y), new Vector2(point.X, point.Y)) <= maxDistance * maxDistance &&
                        !ActiveUnitData.SelfUnits.Any(u => u.Value.Unit.UnitType == (uint)UnitTypes.TERRAN_MISSILETURRET && Vector2.DistanceSquared(new Vector2(point.X, point.Y), u.Value.Position) <= 9))
                    {
                        return point;
                    }
                }
            }
            return null;
        }
    }
}