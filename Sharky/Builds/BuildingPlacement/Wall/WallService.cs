﻿using SC2APIProtocol;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.BuildingPlacement
{
    public class WallService
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private BuildingService BuildingService;
        private SharkyUnitData SharkyUnitData;
        private TargetingData TargetingData;

        public WallService(Sharky.LokiBot.LokiBot lokiBot)
        {
            ActiveUnitData = lokiBot.ActiveUnitData;
            BuildingService = lokiBot.BuildingService;
            SharkyUnitData = lokiBot.SharkyUnitData;
            TargetingData = lokiBot.TargetingData;
            BaseData = lokiBot.BaseData;
        }

        public bool Buildable(Point2D point, float radius)
        {
            if (BuildingService.AreaBuildable(point.X, point.Y, radius) && !BuildingService.Blocked(point.X, point.Y, radius, -.5f) && !BuildingService.HasAnyCreep(point.X, point.Y, radius))
            {
                var mineralFields = ActiveUnitData.NeutralUnits.Where(u => SharkyUnitData.MineralFieldTypes.Contains((UnitTypes)u.Value.Unit.UnitType) || SharkyUnitData.GasGeyserTypes.Contains((UnitTypes)u.Value.Unit.UnitType));
                var squared = (1 + .5) * (1 + .5);
                var nexusDistanceSquared = 0;
                var nexusClashes = ActiveUnitData.SelfUnits.Where(u => (u.Value.Unit.UnitType == (uint)UnitTypes.PROTOSS_NEXUS || u.Value.Unit.UnitType == (uint)UnitTypes.PROTOSS_PYLON) && Vector2.DistanceSquared(u.Value.Position, new Vector2(point.X, point.Y)) < squared + nexusDistanceSquared);
                if (nexusClashes.Count() == 0)
                {
                    var clashes = mineralFields.Where(u => Vector2.DistanceSquared(u.Value.Position, new Vector2(point.X, point.Y)) < squared);
                    if (clashes.Count() == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Point2D GetBaseLocation()
        {
            if (TargetingData.WallOffBasePosition == WallOffBasePosition.Main)
            {
                return TargetingData.SelfMainBasePoint;
            }
            else if (TargetingData.WallOffBasePosition == WallOffBasePosition.Natural)
            {
                return TargetingData.NaturalBasePoint;
            }
            else
            {
                if (TargetingData.ForwardDefenseWallOffPoints == null) { return null; }
                var wallPoint = TargetingData.ForwardDefenseWallOffPoints.FirstOrDefault();
                if (wallPoint == null) { return null; }

                var baseLocation = BaseData.SelfBases.OrderBy(b => Vector2.DistanceSquared(new Vector2(b.Location.X, b.Location.Y), new Vector2(wallPoint.X, wallPoint.Y))).FirstOrDefault();
                if (baseLocation == null) { return null; }
                return baseLocation.Location;
            }
        }
    }
}