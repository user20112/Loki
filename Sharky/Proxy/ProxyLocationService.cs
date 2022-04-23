﻿using SC2APIProtocol;
using Sharky.Pathing;
using System;
using System.Linq;
using System.Numerics;

namespace Sharky.Proxy
{
    public class ProxyLocationService : IProxyLocationService
    {
        private AreaService AreaService;
        private BaseData BaseData;
        private MapDataService MapDataService;
        private IPathFinder PathFinder;
        private TargetingData TargetingData;

        public ProxyLocationService(BaseData baseData, TargetingData targetingData, IPathFinder pathFinder, MapDataService mapDataService, AreaService areaService)
        {
            BaseData = baseData;
            TargetingData = targetingData;
            PathFinder = pathFinder;
            MapDataService = mapDataService;
            AreaService = areaService;
        }

        public CliffProxyData GetCliffProxyData(float offsetDistance = 0)
        {
            var outsideProxyLocation = GetCliffProxyLocation(offsetDistance);
            var targetLocation = TargetingData.EnemyMainBasePoint;

            var angle = Math.Atan2(targetLocation.Y - outsideProxyLocation.Y, outsideProxyLocation.X - targetLocation.X);
            var x = -6 * Math.Cos(angle);
            var y = -6 * Math.Sin(angle);
            var loadingLocation = new Point2D { X = outsideProxyLocation.X + (float)x, Y = outsideProxyLocation.Y - (float)y };

            var loadingVector = new Vector2(loadingLocation.X, loadingLocation.Y);
            var DropArea = AreaService.GetTargetArea(targetLocation);
            var dropVector = DropArea.OrderBy(p => Vector2.DistanceSquared(new Vector2(p.X, p.Y), loadingVector)).First();
            x = -2 * Math.Cos(angle);
            y = -2 * Math.Sin(angle);
            var dropLocation = new Point2D { X = dropVector.X + (float)x, Y = dropVector.Y - (float)y };

            return new CliffProxyData { OutsideProxyLocation = outsideProxyLocation, TargetLocation = targetLocation, LoadingLocation = loadingLocation, DropLocation = dropLocation };
        }

        public Point2D GetCliffProxyLocation(float offsetDistance = 0)
        {
            var numberOfCloseLocations = NumberOfCloseBaseLocations();
            if (MapDataService.MapData.MapName.ToLower().Contains("glittering") || MapDataService.MapData.MapName.ToLower().Contains("berlingrad"))
            {
                numberOfCloseLocations = 3;
            }
            if (MapDataService.MapData.MapName.ToLower().Contains("curious"))
            {
                numberOfCloseLocations = 5;
            }
            var closeAirLocations = BaseData.EnemyBaseLocations.Take(5).OrderBy(b => Vector2.DistanceSquared(new Vector2(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y), new Vector2(b.Location.X, b.Location.Y))).Take(numberOfCloseLocations);

            var baseLocation = closeAirLocations.OrderBy(b => PathFinder.GetGroundPath(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y, b.Location.X, b.Location.Y, 0).Count()).Last().Location;

            var angle = Math.Atan2(TargetingData.EnemyMainBasePoint.Y - baseLocation.Y, baseLocation.X - TargetingData.EnemyMainBasePoint.X);
            var x = offsetDistance * Math.Cos(angle);
            var y = offsetDistance * Math.Sin(angle);
            var location = new Point2D { X = baseLocation.X + (float)x, Y = baseLocation.Y - (float)y };
            if (MapDataService.PathWalkable(location))
            {
                return location;
            }
            return baseLocation;
        }

        public Point2D GetGroundProxyLocation(float offsetDistance = 6)
        {
            int proxyBase = NumberOfCloseBaseLocations() + 1;
            var orderedLocations = BaseData.BaseLocations.OrderBy(b => PathFinder.GetGroundPath(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y, b.Location.X, b.Location.Y, 0).Count());

            var baseLocation = orderedLocations.Take(proxyBase).Last().Location;
            if (MapDataService.MapData.MapName.ToLower().Contains("blackburn"))
            {
                baseLocation = orderedLocations.Take(proxyBase).Skip(2).First().Location;
            }
            if (MapDataService.MapData.MapName.ToLower().Contains("berlingrad"))
            {
                baseLocation = orderedLocations.Take(proxyBase).Skip(3).First().Location;
            }
            if (MapDataService.MapData.MapName.ToLower().Contains("glittering"))
            {
                baseLocation = orderedLocations.Take(proxyBase).Skip(4).First().Location;
            }

            var angle = Math.Atan2(TargetingData.EnemyMainBasePoint.Y - baseLocation.Y, baseLocation.X - TargetingData.EnemyMainBasePoint.X);
            var x = offsetDistance * Math.Cos(angle);
            var y = offsetDistance * Math.Sin(angle);
            var location = new Point2D { X = baseLocation.X + (float)x, Y = baseLocation.Y - (float)y };
            if (MapDataService.PathWalkable(location))
            {
                return location;
            }
            return baseLocation;
        }

        public BaseLocation GetSelfCliffProxyBaseLocation()
        {
            var numberOfCloseLocations = NumberOfCloseBaseLocations();
            var closeAirLocations = BaseData.BaseLocations.Take(5).OrderBy(b => Vector2.DistanceSquared(new Vector2(TargetingData.SelfMainBasePoint.X, TargetingData.SelfMainBasePoint.Y), new Vector2(b.Location.X, b.Location.Y))).Take(numberOfCloseLocations);

            return closeAirLocations.OrderBy(b => PathFinder.GetGroundPath(TargetingData.SelfMainBasePoint.X + 4, TargetingData.SelfMainBasePoint.Y + 4, b.Location.X, b.Location.Y, 0).Count()).Last();
        }

        public BaseLocation GetSelfGroundProxyBaseLocation()
        {
            int proxyBase = NumberOfCloseBaseLocations() + 1;
            var orderedLocations = BaseData.BaseLocations.OrderBy(b => PathFinder.GetGroundPath(TargetingData.SelfMainBasePoint.X + 4, TargetingData.SelfMainBasePoint.Y + 4, b.Location.X, b.Location.Y, 0).Count());

            return orderedLocations.Take(proxyBase).Last();
        }

        private int NumberOfCloseBaseLocations()
        {
            if (MapDataService.MapData.MapName.ToLower().Contains("blackburn")) { return 3; }
            if (MapDataService.MapData.MapName.ToLower().Contains("berlingrad")) { return 4; }
            if (MapDataService.MapData.MapName.ToLower().Contains("glittering")) { return 4; }
            return BaseData.BaseLocations.Count(b => Vector2.DistanceSquared(new Vector2(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y), new Vector2(b.Location.X, b.Location.Y)) < 1200);
        }
    }
}