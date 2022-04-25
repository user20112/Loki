using SC2APIProtocol;
using Sharky.MicroControllers;
using Sharky.Pathing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroTasks
{
    public class ReaperScoutTask : MicroTask
    {
        private AreaService AreaService;
        private BaseData BaseData;
        private DebugService DebugService;
        private MapDataService MapDataService;
        private IIndividualMicroController ReaperController;
        private List<Point2D> ScoutPoints;
        private int StartFrame;
        private TargetingData TargetingData;
        private UnitCountService UnitCountService;

        public ReaperScoutTask(Sharky.LokiBot.BaseLokiBot lokiBot, bool enabled, float priority)
        {
            TargetingData = lokiBot.TargetingData;
            MapDataService = lokiBot.MapDataService;
            DebugService = lokiBot.DebugService;
            BaseData = lokiBot.BaseData;
            AreaService = lokiBot.AreaService;
            UnitCountService = lokiBot.UnitCountService;

            ReaperController = lokiBot.MicroData.IndividualMicroControllers[UnitTypes.TERRAN_REAPER];

            Priority = priority;

            UnitCommanders = new List<UnitCommander>();
            Enabled = enabled;
        }

        private int ScoutLocationIndex { get; set; }
        private List<Point2D> ScoutLocations { get; set; }
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
                    if (!commander.Value.Claimed && commander.Value.UnitRole == UnitRole.Scout && commander.Value.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_REAPER)
                    {
                        commander.Value.Claimed = true;
                        UnitCommanders.Add(commander.Value);
                        started = true;
                        StartFrame = commander.Value.UnitCalculation.FrameLastSeen;
                        return;
                    }
                }
            }
        }

        public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            var commands = new List<SC2APIProtocol.Action>();

            if (ScoutPoints == null)
            {
                var points = AreaService.GetTargetArea(TargetingData.EnemyMainBasePoint);
                ScoutPoints = new List<Point2D>();
                ScoutPoints.Add(BaseData.EnemyBaseLocations.First().Location);
                ScoutPoints.Add(BaseData.EnemyBaseLocations.Skip(1).First().Location);
                var ramp = TargetingData.ChokePoints.Bad.FirstOrDefault();
                if (ramp != null)
                {
                    ScoutPoints.Add(new Point2D { X = ramp.Center.X, Y = ramp.Center.Y });
                }

                var mainVector = new Vector2(TargetingData.EnemyMainBasePoint.X, TargetingData.EnemyMainBasePoint.Y);
                ScoutPoints.AddRange(points.OrderBy(p => Vector2.DistanceSquared(mainVector, new Vector2(p.X, p.Y))));
            }

            ScoutPoints.RemoveAll(p => MapDataService.LastFrameVisibility(p) > StartFrame);

            foreach (var point in ScoutPoints)
            {
                //DebugService.DrawSphere(new Point { X = point.X, Y = point.Y, Z = 12 });
            }

            foreach (var commander in UnitCommanders)
            {
                List<SC2APIProtocol.Action> action;
                if (commander.UnitCalculation.Unit.Health <= commander.UnitCalculation.Unit.HealthMax / 2f)
                {
                    action = ReaperController.Retreat(commander, TargetingData.MainDefensePoint, null, frame);
                }
                else
                {
                    if (ScoutPoints.Count() > 0)
                    {
                        action = ReaperController.Scout(commander, ScoutPoints.FirstOrDefault(), TargetingData.MainDefensePoint, frame);
                    }
                    else
                    {
                        if (SusceptibleToHarrassment())
                        {
                            action = ReaperController.HarassWorkers(commander, TargetingData.EnemyMainBasePoint, TargetingData.MainDefensePoint, frame);
                        }
                        else
                        {
                            action = ScoutEmptyBases(commander, frame);
                        }
                    }
                }
                if (action != null) { commands.AddRange(action); }
            }

            return commands;
        }

        private void GetScoutLocations()
        {
            ScoutLocations = new List<Point2D>();

            foreach (var baseLocation in BaseData.EnemyBaseLocations.Skip(BaseData.EnemyBases.Count()).Take(BaseData.EnemyBaseLocations.Count() - BaseData.EnemyBases.Count()))
            {
                ScoutLocations.Add(baseLocation.MineralLineLocation);
            }
            ScoutLocationIndex = 0;
        }

        private List<SC2APIProtocol.Action> ScoutEmptyBases(UnitCommander commander, int frame)
        {
            if (ScoutLocations == null)
            {
                GetScoutLocations();
            }

            if (Vector2.DistanceSquared(new Vector2(ScoutLocations[ScoutLocationIndex].X, ScoutLocations[ScoutLocationIndex].Y), commander.UnitCalculation.Position) < 2)
            {
                ScoutLocationIndex++;
                if (ScoutLocationIndex >= ScoutLocations.Count())
                {
                    ScoutLocationIndex = 0;
                }
            }
            else
            {
                if (Vector2.DistanceSquared(new Vector2(ScoutLocations[ScoutLocationIndex].X, ScoutLocations[ScoutLocationIndex].Y), commander.UnitCalculation.Position) < 4)
                {
                    ScoutLocationIndex++;
                    if (ScoutLocationIndex >= ScoutLocations.Count())
                    {
                        ScoutLocationIndex = 0;
                    }
                }
                else
                {
                    var action = ReaperController.Scout(commander, ScoutLocations[ScoutLocationIndex], TargetingData.ForwardDefensePoint, frame);
                    if (action != null)
                    {
                        return action;
                    }
                }
            }
            return null;
        }

        private bool SusceptibleToHarrassment()
        {
            if (UnitCountService.EnemyCount(UnitTypes.ZERG_QUEEN) >= 2)
            {
                return false;
            }
            return true;
        }
    }
}