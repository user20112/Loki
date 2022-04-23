﻿using SC2APIProtocol;
using Sharky.Pathing;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.BuildingPlacement
{
    public class HardCodedWallOffPlacement : IBuildingPlacement
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private MapData MapData;
        private ProtossWallService ProtossWallService;
        private SharkyUnitData SharkyUnitData;
        private TerranWallService TerranWallService;
        private WallService WallService;

        public HardCodedWallOffPlacement(ActiveUnitData activeUnitData, SharkyUnitData sharkyUnitData, MapData mapData, BaseData baseData, WallService wallService, TerranWallService terranWallService, ProtossWallService protossWallService)
        {
            ActiveUnitData = activeUnitData;
            SharkyUnitData = sharkyUnitData;
            MapData = mapData;
            BaseData = baseData;

            WallService = wallService;
            TerranWallService = terranWallService;
            ProtossWallService = protossWallService;
        }

        public Point2D FindPlacement(Point2D target, UnitTypes unitType, int size, bool ignoreResourceProximity = false, float maxDistance = 50, bool requireSameHeight = false, WallOffType wallOffType = WallOffType.None, bool requireVision = false, bool allowBlockBase = true)
        {
            var mineralProximity = 2;
            if (ignoreResourceProximity) { mineralProximity = 0; };

            if (wallOffType == WallOffType.Partial && MapData.WallData == null) { return null; }
            if (wallOffType == WallOffType.Terran && MapData.WallData == null) { return null; }

            var baseLocation = WallService.GetBaseLocation();
            if (baseLocation == null) { return null; }

            WallData wallData = null;
            if (wallOffType == WallOffType.Partial)
            {
                wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == baseLocation.X && b.BasePosition.Y == baseLocation.Y);
                if (wallData == null) { return null; }
            }
            else if (wallOffType == WallOffType.Terran)
            {
                wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == baseLocation.X && b.BasePosition.Y == baseLocation.Y);
                if (wallData == null)
                {
                    var firstBase = BaseData.SelfBases.FirstOrDefault();
                    if (firstBase != null)
                    {
                        wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == firstBase.Location.X && b.BasePosition.Y == firstBase.Location.Y);
                    }
                }
                if (wallData == null) { return null; }
            }

            if (unitType == UnitTypes.PROTOSS_PYLON)
            {
                var placement = ProtossWallService.FindPylonPlacement(wallData, maxDistance, mineralProximity, wallOffType);
                if (placement == null) { return null; }
                if (Vector2.DistanceSquared(new Vector2(placement.X, placement.Y), new Vector2(target.X, target.Y)) > maxDistance * maxDistance) { return null; }
                return placement;
            }
            else if (SharkyUnitData.TerranTypes.Contains(unitType))
            {
                var placement = TerranWallService.FindTerranPlacement(wallData, unitType, wallOffType);
                if (placement == null) { return null; }
                if (Vector2.DistanceSquared(new Vector2(placement.X, placement.Y), new Vector2(target.X, target.Y)) > maxDistance * maxDistance) { return null; }
                return placement;
            }
            else
            {
                var placement = FindProductionPlacement(wallData, size, maxDistance, mineralProximity, wallOffType);
                if (placement == null) { return null; }
                if (Vector2.DistanceSquared(new Vector2(placement.X, placement.Y), new Vector2(target.X, target.Y)) > maxDistance * maxDistance) { return null; }
                return placement;
            }
        }

        private Point2D FindFullWallProductionPlacement(WallData wallData, float size, float maxDistance)
        {
            return FindPartialWallProductionPlacement(wallData, size, maxDistance);
        }

        private Point2D FindPartialWallProductionPlacement(WallData wallData, float size, float maxDistance)
        {
            if (wallData.WallSegments == null) { return null; }
            var existingBuildings = ActiveUnitData.SelfUnits.Values.Where(u => u.Attributes.Contains(Attribute.Structure));
            var radius = (size / 2f);
            var powerSources = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_PYLON && c.UnitCalculation.Unit.BuildProgress == 1).Where(c => Vector2.DistanceSquared(c.UnitCalculation.Position, new Vector2(wallData.Pylons.FirstOrDefault().X, wallData.Pylons.FirstOrDefault().Y)) < 15 * 15);

            foreach (var segment in wallData.WallSegments.Where(w => w.Size == size))
            {
                var point = segment.Position;
                if (!existingBuildings.Any(e => e.Position.X == point.X && e.Position.Y == point.Y))
                {
                    if (WallService.Buildable(point, radius) && ProtossWallService.Powered(powerSources, point, radius))
                    {
                        return point;
                    }
                }
            }
            return null;
        }

        private Point2D FindProductionPlacement(WallData wallData, float size, float maxDistance, float minimumMineralProximinity, WallOffType wallOffType)
        {
            if (wallOffType == WallOffType.Partial)
            {
                return FindPartialWallProductionPlacement(wallData, size, 4);
            }

            return FindFullWallProductionPlacement(wallData, size, 4);
        }
    }
}