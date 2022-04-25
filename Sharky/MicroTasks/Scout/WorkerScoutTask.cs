﻿using SC2APIProtocol;
using Sharky.MicroControllers;
using Sharky.Pathing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroTasks
{
    public class WorkerScoutTask : MicroTask
    {
        private AreaService AreaService;
        private BaseData BaseData;
        private DebugService DebugService;
        private MapDataService MapDataService;
        private MineralWalker MineralWalker;
        private List<Point2D> ScoutPoints;
        private SharkyUnitData SharkyUnitData;
        private TargetingData TargetingData;

        public WorkerScoutTask(Sharky.LokiBot.BaseLokiBot lokiBot, bool enabled, float priority)
        {
            SharkyUnitData = lokiBot.SharkyUnitData;
            TargetingData = lokiBot.TargetingData;
            MapDataService = lokiBot.MapDataService;
            DebugService = lokiBot.DebugService;
            BaseData = lokiBot.BaseData;
            AreaService = lokiBot.AreaService;
            MineralWalker = lokiBot.MineralWalker;

            Priority = priority;

            UnitCommanders = new List<UnitCommander>();
            Enabled = enabled;
        }

        private bool started { get; set; }

        public override void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        {
            if (UnitCommanders.Count() == 0)
            {
                if (started)
                {
                    Disable();
                    return;
                }

                foreach (var commander in commanders)
                {
                    if (!commander.Value.Claimed && commander.Value.UnitCalculation.UnitClassifications.Contains(UnitClassification.Worker) && !commander.Value.UnitCalculation.Unit.BuffIds.Any(b => SharkyUnitData.CarryingResourceBuffs.Contains((Buffs)b)))
                    {
                        if (commander.Value.UnitCalculation.Unit.Orders.Any(o => !SharkyUnitData.MiningAbilities.Contains((Abilities)o.AbilityId)))
                        {
                        }
                        else
                        {
                            commander.Value.Claimed = true;
                            commander.Value.UnitRole = UnitRole.Scout;
                            UnitCommanders.Add(commander.Value);
                            started = true;
                            return;
                        }
                    }
                }
            }
        }

        public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            var commands = new List<SC2APIProtocol.Action>();

            if (ScoutPoints == null)
            {
                ScoutPoints = AreaService.GetTargetArea(TargetingData.EnemyMainBasePoint, 5);
                ScoutPoints.Add(BaseData.EnemyBaseLocations.Skip(1).First().Location);
                var ramp = TargetingData.ChokePoints.Bad.FirstOrDefault();
                if (ramp != null)
                {
                    ScoutPoints.Add(new Point2D { X = ramp.Center.X, Y = ramp.Center.Y });
                }
            }

            var mainVector = new Vector2(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y);
            var points = ScoutPoints.OrderBy(p => MapDataService.LastFrameVisibility(p)).ThenBy(p => Vector2.DistanceSquared(mainVector, new Vector2(p.X, p.Y)));

            foreach (var commander in UnitCommanders)
            {
                if (commander.UnitCalculation.Unit.ShieldMax > 5 && (commander.UnitCalculation.Unit.Shield < 5 || (commander.UnitCalculation.Unit.Shield < commander.UnitCalculation.Unit.ShieldMax && commander.UnitCalculation.EnemiesInRangeOf.Count() > 0)))
                {
                    if (MineralWalker.MineralWalkHome(commander, frame, out List<Action> mineralWalk))
                    {
                        commands.AddRange(mineralWalk);
                        return commands;
                    }
                }

                if (commander.UnitCalculation.NearbyEnemies.Count() > 1 && (commander.UnitCalculation.Unit.Shield + commander.UnitCalculation.Unit.Health == commander.UnitCalculation.Unit.ShieldMax + commander.UnitCalculation.Unit.HealthMax) && !commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.ArmyUnit)))
                {
                    var enemy = GetEnemyBuildingScv(commander.UnitCalculation.NearbyEnemies);
                    if (enemy != null)
                    {
                        var attackAction = commander.Order(frame, Abilities.ATTACK, targetTag: enemy.Unit.Tag);
                        if (attackAction != null)
                        {
                            commands.AddRange(attackAction);
                        }
                        return commands;
                    }
                }

                var action = commander.Order(frame, Abilities.MOVE, points.FirstOrDefault());
                if (action != null)
                {
                    commands.AddRange(action);
                }
            }

            return commands;
        }

        private UnitCalculation GetEnemyBuildingScv(List<UnitCalculation> enemies)
        {
            var unfinishedBuilding = enemies.FirstOrDefault(e => e.Unit.BuildProgress < 1);
            if (unfinishedBuilding != null)
            {
                var scv = enemies.Where(e => e.Unit.UnitType == (uint)UnitTypes.TERRAN_SCV).OrderBy(e => Vector2.DistanceSquared(e.Position, unfinishedBuilding.Position)).FirstOrDefault();
                if (scv != null)
                {
                    return scv;
                }
            }
            return null;
        }
    }
}