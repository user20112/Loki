﻿using SC2APIProtocol;
using Sharky.Builds.BuildingPlacement;
using Sharky.MicroTasks.Attack;
using Sharky.Pathing;
using System.Collections.Generic;

namespace Sharky
{
    public class TargetingData
    {
        public Point2D AttackPoint { get; set; }
        public AttackState AttackState { get; set; }
        public ChokePoints ChokePoints { get; set; }
        public Point2D EnemyMainBasePoint { get; set; }
        public Point2D ForwardDefensePoint { get; set; }
        public List<Point2D> ForwardDefenseWallOffPoints { get; set; }
        public bool HiddenEnemyBase { get; set; }
        public Point2D MainDefensePoint { get; set; }
        public Point2D NaturalBasePoint { get; set; }
        public Point2D SelfMainBasePoint { get; set; }
        public List<UnitCalculation> WallBuildings { get; set; }
        public WallOffBasePosition WallOffBasePosition { get; set; }
    }
}