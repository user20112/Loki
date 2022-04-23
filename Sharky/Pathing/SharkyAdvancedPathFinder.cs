﻿using Roy_T.AStar.Graphs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.Pathing
{
    public class SharkyAdvancedPathFinder : IPathFinder
    {
        private Grid AirGrid;
        private int AirGridLastUpdate;
        private float BaseSpeed = 1000f;
        private DebugService DebugService;
        private float EnemyDetectionSpeed = -250f;
        private float EnemyVisionSpeed = -500f;
        private float FriendlyDetectionSpeed = 250f;
        private float FriendlyVisionSpeed = 500f;
        private Grid GroundGrid;
        private int GroundGridLastUpdate;
        private float LowGroundNearHighGroundSpeed = -500f;
        private MapData MapData;
        private MapDataService MapDataService;
        private float MaxDistance = 10f;
        private float MinimumSpeed = 1f;
        private PathFinder PathFinder;
        private Grid UndetectedGroundGrid;
        private int UndetectedGroundGridLastUpdate;

        public SharkyAdvancedPathFinder(PathFinder pathFinder, MapData mapData, MapDataService mapDataService, DebugService debugService)
        {
            PathFinder = pathFinder;
            MapData = mapData;
            MapDataService = mapDataService;
            DebugService = debugService;

            GroundGridLastUpdate = -1;
            UndetectedGroundGridLastUpdate = -1;
            AirGridLastUpdate = -1;
        }

        public List<Vector2> GetGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            var cells = MapDataService.GetCells(startX, startY, MaxDistance);
            var end = new Vector2(endX, endY);
            var best = cells.Where(c => c.Walkable && !c.PathBlocked).OrderBy(c => c.EnemyGroundDpsInRange).ThenBy(c => Vector2.DistanceSquared(end, new Vector2(c.X, c.Y))).FirstOrDefault();
            if (best != null)
            {
                var grid = GetGroundGrid(frame);
                return GetPath(grid, startX, startY, best.X, best.Y);
            }

            return new List<Vector2>();
        }

        public List<Vector2> GetSafeAirPath(float startX, float startY, float endX, float endY, int frame)
        {
            var cells = MapDataService.GetCells(startX, startY, MaxDistance);
            var end = new Vector2(endX, endY);
            var best = cells.OrderBy(c => c.EnemyAirDpsInRange).ThenBy(c => Vector2.DistanceSquared(end, new Vector2(c.X, c.Y))).FirstOrDefault();
            if (best != null)
            {
                var grid = GetAirGrid(frame);
                return GetPath(grid, startX, startY, best.X, best.Y);
            }

            return new List<Vector2>();
        }

        public List<Vector2> GetSafeGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            return GetGroundPath(startX, startY, endX, endY, frame);
        }

        public List<Vector2> GetUndetectedGroundPath(float startX, float startY, float endX, float endY, int frame)
        {
            var cells = MapDataService.GetCells(startX, startY, MaxDistance);
            var end = new Vector2(endX, endY);
            var best = cells.Where(c => c.Walkable && !c.PathBlocked).OrderBy(c => c.InEnemyDetection).ThenBy(c => Vector2.DistanceSquared(end, new Vector2(c.X, c.Y))).FirstOrDefault();
            if (best != null)
            {
                var grid = GetUndetectedGroundGrid(frame);
                return GetPath(grid, startX, startY, best.X, best.Y);
            }

            return new List<Vector2>();
        }

        private Grid GetAirGrid(int frame)
        {
            Node[,] nodes = new Node[MapData.MapWidth, MapData.MapHeight];
            if (AirGridLastUpdate < frame - 2)
            {
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        var node = new Node(new Position(x, y));
                        nodes[x, y] = node;
                    }
                }

                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        var speed = BaseSpeed - MapData.Map[x][y].EnemyAirDpsInRange + MapData.Map[x][y].SelfAirDpsInRange;
                        if (MapData.Map[x][y].InSelfVision)
                        {
                            speed += FriendlyVisionSpeed;
                        }
                        if (MapData.Map[x][y].InEnemyVision)
                        {
                            speed += EnemyVisionSpeed;
                        }
                        //if (MapData.Map[x][y].SelfDetection) // TODO: Self detection
                        //{
                        //    speed += FriendlyDetectionSpeed;
                        //}
                        if (MapData.Map[x][y].InEnemyDetection)
                        {
                            speed += EnemyDetectionSpeed;
                        }
                        if (speed < MinimumSpeed)
                        {
                            speed = MinimumSpeed;
                        }

                        foreach (Node neighbor in GetNeighbors(x, y, 1, nodes))
                        {
                            var node = nodes[x, y];
                            node.Connect(neighbor, Velocity.FromMetersPerSecond(speed));
                        }
                    }
                }

                AirGrid = Grid.CreateGridFrom2DArrayOfNodes(nodes);
                AirGridLastUpdate = frame;
            }
            return AirGrid;
        }

        private Grid GetGroundGrid(int frame)
        {
            Node[,] nodes = new Node[MapData.MapWidth, MapData.MapHeight];
            if (GroundGridLastUpdate < frame - 2)
            {
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        var node = new Node(new Position(x, y));
                        nodes[x, y] = node;
                    }
                }

                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (MapData.Map[x][y].Walkable && !MapData.Map[x][y].PathBlocked)
                        {
                            var speed = BaseSpeed - MapData.Map[x][y].EnemyGroundDpsInRange + MapData.Map[x][y].SelfGroundDpsInRange;
                            if (MapData.Map[x][y].InSelfVision)
                            {
                                speed += FriendlyVisionSpeed;
                            }
                            if (MapData.Map[x][y].InEnemyVision)
                            {
                                speed += EnemyVisionSpeed;
                            }
                            //if (MapData.Map[x][y].SelfDetection) // TODO: Self detection
                            //{
                            //    speed += FriendlyDetectionSpeed;
                            //}
                            if (MapData.Map[x][y].InEnemyDetection)
                            {
                                speed += EnemyDetectionSpeed;
                            }
                            if (speed < MinimumSpeed)
                            {
                                speed = MinimumSpeed;
                            }
                            // TODO: low ground near high ground, add a high ground proximity to the MapCell
                            foreach (Node neighbor in GetNeighbors(x, y, 1, nodes))
                            {
                                if (MapData.Map[(int)neighbor.Position.X][(int)neighbor.Position.Y].Walkable && !MapData.Map[(int)neighbor.Position.X][(int)neighbor.Position.Y].PathBlocked)
                                {
                                    var node = nodes[x, y];
                                    node.Connect(neighbor, Velocity.FromMetersPerSecond(speed));
                                }
                            }
                        }
                    }
                }

                GroundGrid = Grid.CreateGridFrom2DArrayOfNodes(nodes);
                GroundGridLastUpdate = frame;
            }
            return GroundGrid;
        }

        private List<Node> GetNeighbors(float x, float y, float range, Node[,] nodes)
        {
            var neighbors = new List<Node>();

            var xMin = (int)Math.Floor(x - range);
            var xMax = (int)Math.Ceiling(x + range);
            int yMin = (int)Math.Floor(y - range);
            int yMax = (int)Math.Ceiling(y + range);

            if (xMin < 0)
            {
                xMin = 0;
            }
            if (xMax >= nodes.GetLength(0))
            {
                xMax = nodes.GetLength(0) - 1;
            }
            if (yMin < 0)
            {
                yMin = 0;
            }
            if (yMax >= nodes.GetLength(1))
            {
                yMax = nodes.GetLength(1) - 1;
            }

            for (int currentX = xMin; currentX <= xMax; currentX++)
            {
                for (int currentY = yMin; currentY <= yMax; currentY++)
                {
                    neighbors.Add(nodes[currentX, currentY]);
                }
            }

            return neighbors;
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

        private Grid GetUndetectedGroundGrid(int frame)
        {
            Node[,] nodes = new Node[MapData.MapWidth, MapData.MapHeight];
            if (UndetectedGroundGridLastUpdate < frame - 2)
            {
                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        var node = new Node(new Position(x, y));
                        nodes[x, y] = node;
                    }
                }

                for (var x = 0; x < MapData.MapWidth; x++)
                {
                    for (var y = 0; y < MapData.MapHeight; y++)
                    {
                        if (MapData.Map[x][y].Walkable)
                        {
                            var speed = BaseSpeed - MapData.Map[x][y].EnemyGroundDpsInRange + MapData.Map[x][y].SelfGroundDpsInRange;
                            if (MapData.Map[x][y].InSelfVision)
                            {
                                speed += FriendlyVisionSpeed;
                            }
                            if (MapData.Map[x][y].InEnemyVision)
                            {
                                speed += EnemyVisionSpeed;
                            }
                            //if (MapData.Map[x][y].SelfDetection) // TODO: Self detection
                            //{
                            //    speed += FriendlyDetectionSpeed;
                            //}
                            if (MapData.Map[x][y].InEnemyDetection)
                            {
                                speed += EnemyDetectionSpeed;
                            }
                            if (speed < MinimumSpeed)
                            {
                                speed = MinimumSpeed;
                            }
                            // TODO: low ground near high ground, add a high ground proximity to the MapCell
                            foreach (Node neighbor in GetNeighbors(x, y, 1, nodes))
                            {
                                if (MapData.Map[(int)neighbor.Position.X][(int)neighbor.Position.Y].Walkable && !MapData.Map[(int)neighbor.Position.X][(int)neighbor.Position.Y].PathBlocked)
                                {
                                    var node = nodes[x, y];
                                    node.Connect(neighbor, Velocity.FromMetersPerSecond(speed));
                                }
                            }
                        }
                    }
                }

                UndetectedGroundGrid = Grid.CreateGridFrom2DArrayOfNodes(nodes);
                UndetectedGroundGridLastUpdate = frame;
            }
            return UndetectedGroundGrid;
        }
    }
}