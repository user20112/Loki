﻿using SC2APIProtocol;
using Sharky.Builds.BuildingPlacement;
using Sharky.Managers.Terran;
using Sharky.Pathing;
using Sharky.S2ClientTypeEnums;
using System.Linq;

namespace Sharky.Builds.Terran
{
    public class TerranSharkyBuild : SharkyBuild
    {
        private BaseData BaseData;
        private MapDataService MapDataService;
        private OrbitalManager OrbitalManager;
        private SharkyOptions SharkyOptions;
        private SharkyUnitData SharkyUnitData;
        private TargetingData TargetingData;

        public TerranSharkyBuild(Sharky.LokiBot.BaseLokiBot lokiBot) : base(lokiBot)
        {
            TargetingData = lokiBot.TargetingData;
            MapDataService = lokiBot.MapDataService;
            SharkyOptions = lokiBot.SharkyOptions;
            OrbitalManager = lokiBot.OrbitalManager;
            SharkyUnitData = lokiBot.SharkyUnitData;
            BaseData = lokiBot.BaseData;

            ScanAttackPointTime = 120f;
            ScanNextEnemyBaseTime = 120f;
        }

        protected float ScanAttackPointTime { get; set; }
        protected float ScanNextEnemyBaseTime { get; set; }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            BuildOptions.WallOffType = WallOffType.Terran;
            TargetingData.WallOffBasePosition = WallOffBasePosition.Natural;
        }

        protected bool CommandCenterScvKilled()
        {
            var building = ActiveUnitData.Commanders.FirstOrDefault(c => c.Value.UnitCalculation.Unit.BuildProgress < 1 && c.Value.UnitCalculation.Unit.BuildProgress > 0 && c.Value.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_COMMANDCENTER && c.Value.UnitCalculation.Unit.BuildProgress == c.Value.UnitCalculation.PreviousUnit.BuildProgress);
            if (building.Value != null)
            {
                var scvs = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_SCV);
                var buildingScv = scvs.FirstOrDefault(c => c.UnitCalculation.Unit.Orders.Any(o => o.TargetUnitTag == building.Key || (o.TargetWorldSpacePos != null && o.TargetWorldSpacePos.X == building.Value.UnitCalculation.Position.X && o.TargetWorldSpacePos.Y == building.Value.UnitCalculation.Position.Y)));
                if (buildingScv == null)
                {
                    return true;
                }
            }

            return false;
        }

        protected void ScanAttackPoint()
        {
            if (MacroData.Minerals >= 50 && MapDataService.LastFrameVisibility(TargetingData.AttackPoint) < MacroData.Frame - (ScanAttackPointTime * SharkyOptions.FramesPerSecond))
            {
                if (OrbitalManager.ScanQueue.Count == 0 && OrbitalManager.LastScanFrame < MacroData.Frame - 10 && !SharkyUnitData.Effects.Any(e => e.EffectId == (uint)Effects.SCAN && e.Alliance == Alliance.Self))
                {
                    OrbitalManager.ScanQueue.Push(TargetingData.AttackPoint);
                }
            }
        }

        protected void ScanNextEnemyBase()
        {
            if (MacroData.Minerals >= 50 && OrbitalManager.ScanQueue.Count == 0 && OrbitalManager.LastScanFrame < MacroData.Frame - 10 && !SharkyUnitData.Effects.Any(e => e.EffectId == (uint)Effects.SCAN && e.Alliance == Alliance.Self))
            {
                var nextEnemyExpansion = BaseData.EnemyBaseLocations.FirstOrDefault(b => !BaseData.EnemyBases.Any(e => b.Location == e.Location));
                if (nextEnemyExpansion != null)
                {
                    if (SharkyOptions != null && MapDataService.LastFrameVisibility(nextEnemyExpansion.Location) < MacroData.Frame - (ScanNextEnemyBaseTime * SharkyOptions.FramesPerSecond))
                    {
                        OrbitalManager.ScanQueue.Push(nextEnemyExpansion.Location);
                    }
                }
            }
        }

        protected void SendScvForCommandCenter(int frame)
        {
            if (UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER) == 1 && MacroData.Minerals > 275)
            {
                PrePositionBuilderTask.SendBuilder(TargetingData.NaturalBasePoint, frame);
            }
        }

        protected void SendScvForFirstDepot(int frame)
        {
            if (MacroData.FoodUsed == 13 && MacroData.Minerals > 80 && UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_SUPPLYDEPOT) == 0)
            {
                if (MapDataService != null && MapDataService.MapData.WallData != null)
                {
                    var wallData = MapDataService.MapData.WallData.FirstOrDefault(b => b.BasePosition.X == TargetingData.SelfMainBasePoint.X && b.BasePosition.Y == TargetingData.SelfMainBasePoint.Y);
                    if (wallData != null && wallData.Depots != null)
                    {
                        var point = wallData.Depots.FirstOrDefault();
                        if (point != null)
                        {
                            PrePositionBuilderTask.SendBuilder(point, frame);
                            return;
                        }
                    }
                }
                PrePositionBuilderTask.SendBuilder(TargetingData.ForwardDefensePoint, frame);
            }
        }
    }
}