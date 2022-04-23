﻿using SC2APIProtocol;
using Sharky.Builds.BuildingPlacement;
using Sharky.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.Managers
{
    public class TargetingManager : SharkyManager
    {
        private ActiveUnitData ActiveUnitData;
        private int baseCount;
        private BaseData BaseData;
        private ChokePointService ChokePointService;
        private ChokePointsService ChokePointsService;
        private DebugService DebugService;
        private EnemyData EnemyData;
        private int LastUpdateFrame;
        private MacroData MacroData;
        private MapData MapData;
        private Point2D PreviousAttackPoint;
        private Point2D PreviousDefensePoint;
        private SharkyUnitData SharkyUnitData;
        private TargetingData TargetingData;

        public TargetingManager(SharkyUnitData sharkyUnitData, BaseData baseData, MacroData macroData, TargetingData targetingData, MapData mapData, EnemyData enemyData,
            ChokePointService chokePointService, ChokePointsService chokePointsService, DebugService debugService, ActiveUnitData activeUnitData)
        {
            SharkyUnitData = sharkyUnitData;
            BaseData = baseData;
            MacroData = macroData;
            TargetingData = targetingData;
            MapData = mapData;
            ActiveUnitData = activeUnitData;
            EnemyData = enemyData;

            ChokePointService = chokePointService;
            ChokePointsService = chokePointsService;
            DebugService = debugService;

            baseCount = 0;
            LastUpdateFrame = -10000;
            TargetingData.ChokePoints = new ChokePoints();
            TargetingData.WallOffBasePosition = WallOffBasePosition.Current;
            TargetingData.WallBuildings = new List<UnitCalculation>();
        }

        public override IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation)
        {
            UpdateDefensePoint((int)observation.Observation.GameLoop);
            UpdateChokePoints((int)observation.Observation.GameLoop);
            UpdateWall((int)observation.Observation.GameLoop);

            DebugService.DrawSphere(new Point { X = TargetingData.MainDefensePoint.X, Y = TargetingData.MainDefensePoint.Y, Z = 12 }, 2, new Color { R = 0, G = 255, B = 0 });
            DebugService.DrawSphere(new Point { X = TargetingData.ForwardDefensePoint.X, Y = TargetingData.ForwardDefensePoint.Y, Z = 12 }, 2, new Color { R = 0, G = 0, B = 255 });
            DebugService.DrawSphere(new Point { X = TargetingData.AttackPoint.X, Y = TargetingData.AttackPoint.Y, Z = 12 }, 2, new Color { R = 255, G = 0, B = 0 });

            if (TargetingData.ForwardDefenseWallOffPoints != null)
            {
                var wallPoints = TargetingData.ForwardDefenseWallOffPoints;
                var wallCenter = new Vector2(wallPoints.Sum(p => p.X) / wallPoints.Count(), wallPoints.Sum(p => p.Y) / wallPoints.Count());
                DebugService.DrawSphere(new Point { X = wallCenter.X, Y = wallCenter.Y, Z = 12 }, 1, new Color { R = 1, G = 250, B = 1 });
            }

            foreach (var chokePoint in TargetingData.ChokePoints.Good)
            {
                DebugService.DrawSphere(new Point { X = chokePoint.Center.X, Y = chokePoint.Center.Y, Z = 12 }, 1, new Color { R = 250, G = 250, B = 250 });
            }

            return null;
        }

        public override void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentId)
        {
            foreach (var location in gameInfo.StartRaw.StartLocations)
            {
                TargetingData.AttackPoint = location;
                TargetingData.EnemyMainBasePoint = location;
            }
            foreach (var unit in observation.Observation.RawData.Units.Where(u => u.Alliance == Alliance.Self && SharkyUnitData.UnitData[(UnitTypes)u.UnitType].Attributes.Contains(SC2APIProtocol.Attribute.Structure)))
            {
                TargetingData.MainDefensePoint = new Point2D { X = unit.Pos.X, Y = unit.Pos.Y };

                var chokePoint = ChokePointService.FindDefensiveChokePoint(new Point2D { X = unit.Pos.X + 4, Y = unit.Pos.Y + 4 }, TargetingData.AttackPoint, 0);
                if (chokePoint != null)
                {
                    TargetingData.ForwardDefensePoint = chokePoint;
                    var chokePoints = ChokePointService.GetEntireChokePoint(chokePoint);
                    if (chokePoints != null)
                    {
                        var wallPoints = ChokePointService.GetWallOffPoints(chokePoints);
                        if (wallPoints != null)
                        {
                            TargetingData.ForwardDefenseWallOffPoints = wallPoints;
                        }
                    }
                }
                else
                {
                    TargetingData.ForwardDefensePoint = new Point2D { X = unit.Pos.X, Y = unit.Pos.Y };
                }

                if (MapData.WallData != null)
                {
                    var wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == unit.Pos.X && b.BasePosition.Y == unit.Pos.Y);
                    if (wallData != null && wallData.Door != null)
                    {
                        TargetingData.ForwardDefensePoint = wallData.Door;
                    }
                }

                TargetingData.SelfMainBasePoint = new Point2D { X = unit.Pos.X, Y = unit.Pos.Y };
                var naturalBaseLocation = BaseData.BaseLocations.Skip(1).Take(1).FirstOrDefault();
                if (naturalBaseLocation != null)
                {
                    TargetingData.NaturalBasePoint = naturalBaseLocation.Location;
                }

                AddCalcultedWallData();

                return;
            }
        }

        private WallData AddCalculatedWallDataForBase(BaseLocation baseLocation, Point2D oppositeLocation, WallData data)
        {
            var location = new Point2D { X = baseLocation.Location.X + 4, Y = baseLocation.Location.Y + 4 };
            var chokePoints = ChokePointsService.GetChokePoints(location, oppositeLocation, 0);
            var chokePoint = chokePoints.Good.FirstOrDefault();
            if (chokePoint != null && Vector2.DistanceSquared(chokePoint.Center, new Vector2(location.X, location.Y)) < 900)
            {
                var wallPoints = ChokePointService.GetWallOffPoints(chokePoint.Points);

                if (wallPoints != null)
                {
                    var wallCenter = new Vector2(wallPoints.Sum(p => p.X) / wallPoints.Count(), wallPoints.Sum(p => p.Y) / wallPoints.Count());

                    if (chokePoint.Center.X > wallCenter.X) // left to right
                    {
                        if (chokePoint.Center.Y < wallCenter.Y) // top to bottom
                        {
                            var baseX = wallPoints.Last().X;
                            var baseY = wallPoints.Last().Y;

                            if (data.FullDepotWall == null)
                            {
                                data.FullDepotWall = new List<Point2D> { new Point2D { X = baseX, Y = baseY + 1 }, new Point2D { X = baseX - 2, Y = baseY }, new Point2D { X = baseX - 3, Y = baseY - 2 } };
                            }
                            if (data.Depots == null)
                            {
                                data.Depots = new List<Point2D> { new Point2D { X = baseX - 3, Y = baseY - 2 }, new Point2D { X = baseX, Y = baseY + 1 } };
                            }
                            if (data.Production == null)
                            {
                                data.Production = new List<Point2D> { new Point2D { X = baseX - 2.5f, Y = baseY + .5f } };
                            }
                            if (data.ProductionWithAddon == null)
                            {
                                data.ProductionWithAddon = new List<Point2D> { new Point2D { X = baseX - 4.5f, Y = baseY + .5f } };
                            }
                            if (data.RampCenter == null)
                            {
                                data.RampCenter = new Point2D { X = baseX + .5f, Y = baseY - 2.5f };
                            }

                            if (data.Pylons == null)
                            {
                                data.Pylons = new List<Point2D> { new Point2D { X = baseX - 3, Y = baseY + 3 } };
                            }
                            if (data.WallSegments == null)
                            {
                                data.WallSegments = new List<WallSegment>
                                {
                                    new WallSegment { Position = new Point2D { X = baseX - .5f, Y = baseY + 1.5f }, Size = 3 },
                                    new WallSegment { Position = new Point2D { X = baseX - 3.5f, Y = baseY + .5f }, Size = 3 }
                                };
                            }
                            if (data.Block == null)
                            {
                                data.Block = new Point2D { X = baseX - 3, Y = baseY - 2 };
                            }
                            if (data.Door == null)
                            {
                                data.Door = new Point2D { X = baseX - 3, Y = baseY - 2 };
                            }
                        }
                        else // bottom to top
                        {
                            var baseX = wallPoints.First().X;
                            var baseY = wallPoints.First().Y;

                            if (data.FullDepotWall == null)
                            {
                                data.FullDepotWall = new List<Point2D> { new Point2D { X = baseX - 1, Y = baseY }, new Point2D { X = baseX, Y = baseY - 2 }, new Point2D { X = baseX + 2, Y = baseY - 3 } };
                            }
                            if (data.Depots == null)
                            {
                                data.Depots = new List<Point2D> { new Point2D { X = baseX - 1, Y = baseY }, new Point2D { X = baseX + 2, Y = baseY - 3 } };
                            }
                            if (data.Production == null)
                            {
                                data.Production = new List<Point2D> { new Point2D { X = baseX - .5f, Y = baseY - 2.5f } };
                            }
                            if (data.ProductionWithAddon == null)
                            {
                                data.ProductionWithAddon = new List<Point2D> { new Point2D { X = baseX - 2.5f, Y = baseY - 2.5f } };
                            }
                            if (data.RampCenter == null)
                            {
                                data.RampCenter = new Point2D { X = baseX + 2f, Y = baseY };
                            }

                            if (data.Pylons == null)
                            {
                                data.Pylons = new List<Point2D> { new Point2D { X = baseX - 3, Y = baseY - 3 } };
                            }
                            if (data.WallSegments == null)
                            {
                                data.WallSegments = new List<WallSegment>
                                {
                                    new WallSegment { Position = new Point2D { X = baseX - 1.5f, Y = baseY - .5f }, Size = 3 },
                                    new WallSegment { Position = new Point2D { X = baseX - .5f, Y = baseY - 3.5f }, Size = 3 }
                                };
                            }
                            if (data.Block == null)
                            {
                                data.Block = new Point2D { X = baseX + 2, Y = baseY - 3 };
                            }
                            if (data.Door == null)
                            {
                                data.Door = new Point2D { X = baseX + 2, Y = baseY - 3 };
                            }
                        }
                    }
                    else // right to left
                    {
                        if (chokePoint.Center.Y < wallCenter.Y) // top to bottom
                        {
                            var baseX = wallPoints.Last().X;
                            var baseY = wallPoints.Last().Y;

                            if (data.FullDepotWall == null)
                            {
                                data.FullDepotWall = new List<Point2D> { new Point2D { X = baseX, Y = baseY + 1 }, new Point2D { X = baseX - 1, Y = baseY + 3 }, new Point2D { X = baseX - 3, Y = baseY + 4 } };
                            }
                            if (data.Depots == null)
                            {
                                data.Depots = new List<Point2D> { new Point2D { X = baseX, Y = baseY + 1 }, new Point2D { X = baseX - 3, Y = baseY + 4 } };
                            }
                            if (data.Production == null)
                            {
                                data.Production = new List<Point2D> { new Point2D { X = baseX - .5f, Y = baseY + 3.5f } };
                            }
                            if (data.RampCenter == null)
                            {
                                data.RampCenter = new Point2D { X = baseX - 3.5f, Y = baseY + .5f };
                            }

                            if (data.Pylons == null)
                            {
                                data.Pylons = new List<Point2D> { new Point2D { X = baseX + 2, Y = baseY + 5 } };
                            }
                            if (data.WallSegments == null)
                            {
                                data.WallSegments = new List<WallSegment>
                                {
                                    new WallSegment { Position = new Point2D { X = baseX + .5f, Y = baseY + 1.5f }, Size = 3 },
                                    new WallSegment { Position = new Point2D { X = baseX - .5f, Y = baseY + 4.5f }, Size = 3 }
                                };
                            }
                            if (data.Block == null)
                            {
                                data.Block = new Point2D { X = baseX - 3, Y = baseY + 4 };
                            }
                            if (data.Door == null)
                            {
                                data.Door = new Point2D { X = baseX - 3, Y = baseY + 4 };
                            }
                        }
                        else // bottom to top
                        {
                            var baseX = wallPoints.First().X;
                            var baseY = wallPoints.First().Y;

                            if (data.FullDepotWall == null)
                            {
                                data.FullDepotWall = new List<Point2D> { new Point2D { X = baseX + 1, Y = baseY }, new Point2D { X = baseX + 3, Y = baseY + 1 }, new Point2D { X = baseX + 4, Y = baseY + 3 } };
                            }
                            if (data.RampCenter == null)
                            {
                                data.RampCenter = new Point2D { X = baseX + .5f, Y = baseY + 3.5f };
                            }
                            if (data.Depots == null)
                            {
                                data.Depots = new List<Point2D> { new Point2D { X = baseX + 1, Y = baseY }, new Point2D { X = baseX + 4, Y = baseY + 3 } };
                            }
                            if (data.Production == null)
                            {
                                data.Production = new List<Point2D> { new Point2D { X = baseX + 3.5f, Y = baseY + .5f } };
                            }

                            if (data.Pylons == null)
                            {
                                data.Pylons = new List<Point2D> { new Point2D { X = baseX + 4, Y = baseY - 2 } };
                            }
                            if (data.WallSegments == null)
                            {
                                data.WallSegments = new List<WallSegment>
                                {
                                    new WallSegment { Position = new Point2D { X = baseX + 1.5f, Y = baseY - .5f }, Size = 3 },
                                    new WallSegment { Position = new Point2D { X = baseX + 4.5f, Y = baseY + .5f }, Size = 3 }
                                };
                            }
                            if (data.Block == null)
                            {
                                data.Block = new Point2D { X = baseX + 4, Y = baseY + 3 };
                            }
                            if (data.Door == null)
                            {
                                data.Door = new Point2D { X = baseX + 4, Y = baseY + 3 };
                            }
                        }
                    }
                }
            }

            return data;
        }

        private void AddCalcultedWallData()
        {
            if (MapData.WallData == null)
            {
                MapData.WallData = new List<WallData>();
            }

            var oppositeBase = BaseData.EnemyBaseLocations.FirstOrDefault();
            var oppositeLocation = new Point2D { X = oppositeBase.Location.X + 4, Y = oppositeBase.Location.Y + 4 };
            var baseLocation = BaseData.BaseLocations.FirstOrDefault();
            if (baseLocation != null)
            {
                var data = MapData.WallData.FirstOrDefault(d => d.BasePosition.X == baseLocation.Location.X && d.BasePosition.Y == baseLocation.Location.Y);
                MapData.WallData.Remove(data);
                if (data == null) { data = new WallData { BasePosition = baseLocation.Location }; }
                data = AddCalculatedWallDataForBase(baseLocation, oppositeLocation, data);
                MapData.WallData.Add(data);
            }

            oppositeBase = BaseData.BaseLocations.FirstOrDefault();
            oppositeLocation = new Point2D { X = oppositeBase.Location.X + 4, Y = oppositeBase.Location.Y + 4 };
            baseLocation = BaseData.EnemyBaseLocations.FirstOrDefault();
            if (baseLocation != null)
            {
                var data = MapData.WallData.FirstOrDefault(d => d.BasePosition.X == baseLocation.Location.X && d.BasePosition.Y == baseLocation.Location.Y);
                MapData.WallData.Remove(data);
                if (data == null) { data = new WallData { BasePosition = baseLocation.Location }; }
                data = AddCalculatedWallDataForBase(baseLocation, oppositeLocation, data);
                MapData.WallData.Add(data);
            }
        }

        private void AddWalls(IEnumerable<UnitCalculation> buildings, WallData wallData)
        {
            if (EnemyData.SelfRace == Race.Protoss)
            {
                if (wallData.WallSegments != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => wallData.WallSegments.Any(w => w.Position.X == b.Position.X && w.Position.Y == b.Position.Y)));
                }
                if (wallData.Pylons != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => b.Unit.UnitType == (uint)UnitTypes.PROTOSS_PYLON && wallData.Pylons.Any(w => w.X == b.Position.X && w.Y == b.Position.Y)));
                }
            }
            else if (EnemyData.SelfRace == Race.Terran)
            {
                if (wallData.Bunkers != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => b.Unit.UnitType == (uint)UnitTypes.TERRAN_BUNKER && wallData.Bunkers.Any(w => w.X == b.Position.X && w.Y == b.Position.Y)));
                }
                if (wallData.Depots != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => (b.Unit.UnitType == (uint)UnitTypes.TERRAN_SUPPLYDEPOT || b.Unit.UnitType == (uint)UnitTypes.TERRAN_SUPPLYDEPOTLOWERED) && wallData.Depots.Any(w => w.X == b.Position.X && w.Y == b.Position.Y)));
                }
                if (wallData.Production != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => wallData.Production.Any(w => w.X == b.Position.X && w.Y == b.Position.Y)));
                }
                if (wallData.ProductionWithAddon != null)
                {
                    TargetingData.WallBuildings.AddRange(buildings.Where(b => wallData.ProductionWithAddon.Any(w => w.X == b.Position.X && w.Y == b.Position.Y)));
                }
            }
        }

        private void UpdateChokePoints(int frame)
        {
            if (frame == 0)
            {
                PreviousAttackPoint = TargetingData.AttackPoint;
                PreviousDefensePoint = TargetingData.ForwardDefensePoint;
                TargetingData.ChokePoints = ChokePointsService.GetChokePoints(TargetingData.ForwardDefensePoint, TargetingData.AttackPoint, frame);
            }
        }

        private void UpdateDefensePoint(int frame)
        {
            if (baseCount != BaseData.SelfBases.Count())
            {
                var resourceCenters = ActiveUnitData.SelfUnits.Values.Where(u => u.UnitClassifications.Contains(UnitClassification.ResourceCenter) && !u.Unit.IsFlying);
                var ordered = BaseData.BaseLocations.Where(b => resourceCenters.Any(r => Vector2.DistanceSquared(r.Position, new Vector2(b.Location.X, b.Location.Y)) < 25)).OrderBy(b => Vector2.DistanceSquared(new Vector2(b.Location.X, b.Location.Y), new Vector2(TargetingData.AttackPoint.X, TargetingData.AttackPoint.Y)));
                var closestBase = ordered.FirstOrDefault();
                if (TargetingData.WallOffBasePosition == WallOffBasePosition.Natural && resourceCenters.Count() == 1)
                {
                    closestBase = BaseData.BaseLocations.FirstOrDefault(b => b.Location.X == TargetingData.NaturalBasePoint.X && b.Location.Y == TargetingData.NaturalBasePoint.Y);
                }
                if (closestBase != null)
                {
                    TargetingData.MainDefensePoint = closestBase.MineralLineLocation;
                    var chokePoint = ChokePointService.FindDefensiveChokePoint(new Point2D { X = closestBase.Location.X + 4, Y = closestBase.Location.Y + 4 }, TargetingData.AttackPoint, frame);

                    if (chokePoint != null)
                    {
                        TargetingData.ForwardDefensePoint = chokePoint;
                        var chokePoints = ChokePointService.GetEntireChokePoint(chokePoint);
                        if (chokePoints != null)
                        {
                            var wallPoints = ChokePointService.GetWallOffPoints(chokePoints);
                            if (wallPoints != null)
                            {
                                TargetingData.ForwardDefenseWallOffPoints = wallPoints;
                            }
                        }
                    }
                    else
                    {
                        var angle = Math.Atan2(closestBase.Location.Y - TargetingData.EnemyMainBasePoint.Y, TargetingData.EnemyMainBasePoint.X - closestBase.Location.X);
                        TargetingData.ForwardDefensePoint = new Point2D { X = closestBase.Location.X + (float)(6 * Math.Cos(angle)), Y = closestBase.Location.Y - (float)(6 * Math.Sin(angle)) };
                    }

                    if (MapData.WallData != null)
                    {
                        var wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == closestBase.Location.X && b.BasePosition.Y == closestBase.Location.Y);
                        if (wallData != null && wallData.Door != null)
                        {
                            var angle = Math.Atan2(wallData.Door.Y - TargetingData.SelfMainBasePoint.Y, TargetingData.SelfMainBasePoint.X - wallData.Door.X);
                            TargetingData.ForwardDefensePoint = new Point2D { X = wallData.Door.X + (float)(2 * Math.Cos(angle)), Y = wallData.Door.Y - (float)(2 * Math.Sin(angle)) };
                        }
                    }
                }
                var farthestBase = ordered.LastOrDefault();
                if (farthestBase != null)
                {
                    TargetingData.SelfMainBasePoint = farthestBase.Location;
                }
                baseCount = BaseData.SelfBases.Count();
            }
            foreach (var task in MacroData.Proxies)
            {
                if (task.Value.Enabled)
                {
                    TargetingData.ForwardDefensePoint = task.Value.Location;
                }
            }
        }

        private void UpdateWall(int frame)
        {
            if (frame > LastUpdateFrame + 50)
            {
                TargetingData.WallBuildings.Clear();

                var buildings = ActiveUnitData.SelfUnits.Values.Where(u => u.Attributes.Contains(SC2APIProtocol.Attribute.Structure));

                if (EnemyData.SelfRace == Race.Protoss && MapData.WallData != null)
                {
                    var wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == TargetingData.SelfMainBasePoint.X && b.BasePosition.Y == TargetingData.SelfMainBasePoint.Y);
                    if (wallData != null)
                    {
                        AddWalls(buildings, wallData);
                    }
                    wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == TargetingData.NaturalBasePoint.X && b.BasePosition.Y == TargetingData.NaturalBasePoint.Y);
                    if (wallData != null)
                    {
                        AddWalls(buildings, wallData);
                    }
                }
                else if (EnemyData.SelfRace == Race.Terran && MapData.WallData != null)
                {
                    var wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == TargetingData.SelfMainBasePoint.X && b.BasePosition.Y == TargetingData.SelfMainBasePoint.Y);
                    if (wallData != null)
                    {
                        AddWalls(buildings, wallData);
                    }
                    wallData = MapData.WallData.FirstOrDefault(b => b.BasePosition.X == TargetingData.NaturalBasePoint.X && b.BasePosition.Y == TargetingData.NaturalBasePoint.Y);
                    if (wallData != null)
                    {
                        AddWalls(buildings, wallData);
                    }
                }

                LastUpdateFrame = frame;
            }
        }
    }
}