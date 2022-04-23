﻿using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.Pathing
{
    public class SharkyPathFinder : IPathFinder
    {
        private Grid AirDamageGrid;
        private int AirDamageLastUpdate;
        private Grid BuildingGrid;
        private DebugService DebugService;
        private Grid EnemyDetectionGrid;
        private Grid EnemyVisionGrid;
        private Grid EnemyVisionGroundGrid;
        private Grid GroundDamageGrid; // TDOO: include buildings for ground grids, can't walk through them
        private int GroundDamageLastUpdate;
        private Grid GroundDetectionGrid;
        private int GroundDetectionLastUpdate;
        private MapData MapData;
        private MapDataService MapDataService;
        private int MapLastUpdate;

        private PathFinder PathFinder;
        private Grid WalkGrid;

        public SharkyPathFinder(PathFinder pathFinder, MapData mapData, MapDataService mapDataService, DebugService debugService)
        {
            PathFinder = pathFinder;
            MapData = mapData;
            MapDataService = mapDataService;
            DebugService = debugService;

            GroundDamageLastUpdate = -1;
            GroundDetectionLastUpdate = -1;
            AirDamageLastUpdate = -1;
            MapLastUpdate = -1;
        }

        public List<Vector2> GetGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            var grid = GetMapGrid(frame);
            return GetPath(grid, startX, startY, endX, endY);
        }

        public List<Vector2> GetSafeAirPath(float startX, float startY, float endX, float endY, int frame)
        {
            var grid = GetAirDamageGrid(frame);
            var path = GetPath(grid, startX, startY, endX, endY);
            if (path.Count() == 0)
            {
                var cells = MapDataService.GetCells(startX, startY, 1);
                var best = cells.OrderBy(c => c.EnemyAirDpsInRange).FirstOrDefault();
                if (best != null)
                {
                    path = new List<Vector2> { new Vector2(startX, startY), new Vector2(best.X, best.Y) };
                }
            }

            return path;
        }

        public List<Vector2> GetSafeGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            var grid = GetGroundDamageGrid(frame);
            var path = GetPath(grid, startX, startY, endX, endY);
            if (path.Count() == 0)
            {
                var cells = MapDataService.GetCells(startX, startY, 1);
                var best = cells.Where(c => c.Walkable).OrderBy(c => c.EnemyGroundDpsInRange).FirstOrDefault();
                if (best != null)
                {
                    path = new List<Vector2> { new Vector2(startX, startY), new Vector2(best.X, best.Y) };
                }
            }

            return path;
        }

        public List<Vector2> GetUndetectedGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            var grid = GetGroundDetectionGrid(frame);
            var path = GetPath(grid, startX, startY, endX, endY);
            if (path.Count() == 0)
            {
                var cells = MapDataService.GetCells(startX, startY, 1);
                var best = cells.Where(c => c.Walkable).OrderBy(c => c.EnemyGroundDpsInRange).FirstOrDefault();
                if (best != null)
                {
                    path = new List<Vector2> { new Vector2(startX, startY), new Vector2(best.X, best.Y) };
                }
            }

            return path;
        }

        private Grid GetAirDamageGrid(int frame)
        {
            if (AirDamageLastUpdate < frame)
            {
                var gridSize = new GridSize(columns: MapData.MapWidth, rows: MapData.MapHeight);
                var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
                var traversalVelocity = Velocity.FromMetersPerSecond(1);
                AirDamageGrid = Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (MapData.Map[x][y].EnemyAirDpsInRange > 0)
                        {
                            AirDamageGrid.DisconnectNode(new GridPosition(x, y));
                        }
                    }
                }
                AirDamageLastUpdate = frame;
            }
            return AirDamageGrid;
        }

        private Grid GetGroundDamageGrid(int frame)
        {
            if (GroundDamageLastUpdate < frame)
            {
                var gridSize = new GridSize(columns: MapData.MapWidth, rows: MapData.MapHeight);
                var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
                var traversalVelocity = Velocity.FromMetersPerSecond(1);
                GroundDamageGrid = Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (!MapData.Map[x][y].Walkable || MapData.Map[x][y].EnemyGroundDpsInRange > 0)
                        {
                            GroundDamageGrid.DisconnectNode(new GridPosition(x, y));
                        }
                    }
                }
                GroundDamageLastUpdate = frame;
            }
            return GroundDamageGrid;
        }

        private Grid GetGroundDetectionGrid(int frame)
        {
            if (GroundDetectionLastUpdate < frame)
            {
                var gridSize = new GridSize(columns: MapData.MapWidth, rows: MapData.MapHeight);
                var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
                var traversalVelocity = Velocity.FromMetersPerSecond(1);
                GroundDetectionGrid = Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (!MapData.Map[x][y].Walkable || MapData.Map[x][y].InEnemyDetection)
                        {
                            GroundDetectionGrid.DisconnectNode(new GridPosition(x, y));
                        }
                    }
                }
                GroundDetectionLastUpdate = frame;
            }
            return GroundDetectionGrid;
        }

        private List<Vector2> GetHiddenAirPath(float startX, float startY, float endX, float endY)
        {
            return GetPath(EnemyVisionGrid, startX, startY, endX, endY);
        }

        private List<Vector2> GetHiddenGroundPath(float startX, float startY, float endX, float endY)
        {
            return GetPath(EnemyVisionGroundGrid, startX, startY, endX, endY);
        }

        private Grid GetMapGrid(int frame)
        {
            if (MapLastUpdate < frame)
            {
                var gridSize = new GridSize(columns: MapData.MapWidth, rows: MapData.MapHeight);
                var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
                var traversalVelocity = Velocity.FromMetersPerSecond(1);
                WalkGrid = Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (!MapData.Map[x][y].Walkable)
                        {
                            WalkGrid.DisconnectNode(new GridPosition(x, y));
                            //DebugService.DrawSphere(new Point { X = x, Y = y, Z = MapData.Map[x][y].TerrainHeight + 1 }, 2, new Color { R = 0, G = 255, B = 0 });
                        }
                        else
                        {
                            //DebugService.DrawSphere(new Point { X = x, Y = y, Z = MapData.Map[x][y].TerrainHeight + 1 }, 2, new Color { R = 255, G = 0, B = 0 });
                        }
                    }
                }
            }
            return WalkGrid;
        }

        private List<GridPosition> GetNodesInRange(Point position, float range, int columns, int rows)
        {
            var nodes = new List<GridPosition>();
            //nodes.Add(new GridPosition((int)position.X, (int)position.Y));
            var xMin = (int)Math.Floor(position.X - range);
            var xMax = (int)Math.Ceiling(position.X + range);
            int yMin = (int)Math.Floor(position.Y - range);
            int yMax = (int)Math.Ceiling(position.Y + range);

            if (xMin < 0)
            {
                xMin = 0;
            }
            if (xMax >= columns)
            {
                xMax = columns - 1;
            }
            if (yMin < 0)
            {
                yMin = 0;
            }
            if (yMax >= rows)
            {
                yMax = rows - 1;
            }

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    nodes.Add(new GridPosition(x, y));
                }
            }

            return nodes;
        }

        private List<Vector2> GetPath(Grid grid, float startX, float startY, float endX, float endY)
        {
            if (startX >= grid.GridSize.Columns)
            {
                startX = grid.GridSize.Columns - 1;
            }
            if (endX >= grid.GridSize.Columns)
            {
                endX = grid.GridSize.Columns - 1;
            }
            if (startY >= grid.GridSize.Rows)
            {
                startY = grid.GridSize.Rows - 1;
            }
            if (endY >= grid.GridSize.Rows)
            {
                endY = grid.GridSize.Rows - 1;
            }
            try
            {
                var path = PathFinder.FindPath(new GridPosition((int)startX, (int)startY), new GridPosition((int)endX, (int)endY), grid);
                return path.Edges.Select(e => new Vector2(e.End.Position.X, e.End.Position.Y)).ToList();
            }
            catch (Exception)
            {
                return new List<Vector2>();
            }
        }

        private void UpdateBuildingGrid(IEnumerable<UnitCalculation> buildings, IEnumerable<Unit> resourceUnits)
        {
            // TODO: store the old buildings, if the buildings are the same don't update, just return
            BuildingGrid = WalkGrid;
            foreach (var building in buildings)
            {
                var nodes = GetNodesInRange(building.Unit.Pos, building.Unit.Radius, BuildingGrid.Columns, BuildingGrid.Rows);
                foreach (var node in nodes)
                {
                    BuildingGrid.DisconnectNode(node);
                }
            }
            foreach (var resource in resourceUnits)
            {
                var nodes = GetNodesInRange(resource.Pos, resource.Radius, BuildingGrid.Columns, BuildingGrid.Rows);
                foreach (var node in nodes)
                {
                    BuildingGrid.DisconnectNode(node);
                }
            }
        }

        private void UpdateEnemyVisionGrid(IEnumerable<UnitCalculation> enemyUnits)
        {
            EnemyVisionGrid = Grid.CreateGridWithLateralAndDiagonalConnections(WalkGrid.GridSize, new Size(Distance.FromMeters(1), Distance.FromMeters(1)), Velocity.FromMetersPerSecond(1));
            foreach (var enemy in enemyUnits)
            {
                var nodes = GetNodesInRange(enemy.Unit.Pos, 11, EnemyVisionGrid.Columns, EnemyVisionGrid.Rows); // TODO: get sight range of every unit, // TODO: units on low ground can't see high ground
                foreach (var node in nodes)
                {
                    EnemyVisionGrid.DisconnectNode(node);
                }
            }
        }

        private void UpdateEnemyVisionGroundGrid(IEnumerable<UnitCalculation> enemyUnits)
        {
            EnemyVisionGroundGrid = BuildingGrid;
            foreach (var enemy in enemyUnits)
            {
                var nodes = GetNodesInRange(enemy.Unit.Pos, 11, EnemyVisionGroundGrid.Columns, EnemyVisionGroundGrid.Rows); // TODO: get sight range of every unit // TODO: units on low ground can't see high ground
                foreach (var node in nodes)
                {
                    EnemyVisionGroundGrid.DisconnectNode(node);
                }
            }
        }
    }
}