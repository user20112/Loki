﻿using SC2APIProtocol;

namespace Sharky.Builds.BuildingPlacement
{
    public interface IBuildingPlacement
    {
        Point2D FindPlacement(Point2D target, UnitTypes unitType, int size, bool ignoreResourceProximity = false, float maxDistance = 50, bool requireSameHeight = false, WallOffType wallOffType = WallOffType.None, bool requireVision = false, bool allowBlockBase = true);
    }
}