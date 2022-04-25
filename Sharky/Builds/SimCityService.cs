using SC2APIProtocol;
using Sharky.Builds.BuildingPlacement;
using Sharky.Builds.MacroServices;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.Builds
{
    public class SimCityService
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private int FrameInterval;
        private int LastFrame;
        private MacroData MacroData;
        private IBuildingPlacement ProtectNexusBatteryPlacement;
        private IBuildingPlacement ProtectNexusCannonPlacement;
        private IBuildingPlacement ProtectNexusPylonPlacement;
        private TargetingData TargetingData;
        private UnitCountService UnitCountService;
        private WorkerBuilderService WorkerBuilderService;

        public SimCityService(Sharky.LokiBot.BaseLokiBot lokiBot)
        {
            MacroData = lokiBot.MacroData;
            BaseData = lokiBot.BaseData;
            TargetingData = lokiBot.TargetingData;
            ProtectNexusPylonPlacement = lokiBot.ProtectNexusPylonPlacement;
            ProtectNexusCannonPlacement = lokiBot.ProtectNexusCannonPlacement;
            ProtectNexusBatteryPlacement = lokiBot.ProtectNexusBatteryPlacement;
            WorkerBuilderService = lokiBot.WorkerBuilderService;
            ActiveUnitData = lokiBot.ActiveUnitData;
            UnitCountService = lokiBot.UnitCountService;

            FrameInterval = 20;
            LastFrame = 0;
        }

        public IEnumerable<SC2APIProtocol.Action> OnFrame()
        {
            if (MacroData.Frame - LastFrame < FrameInterval && TargetingData.SelfMainBasePoint != null && TargetingData.NaturalBasePoint != null)
            {
                return null;
            }

            LastFrame = MacroData.Frame;

            if (MacroData.DesiredExtraBaseSimCityPylons > 0 && MacroData.Minerals >= 100)
            {
                foreach (var data in BaseData.SelfBases.Where(b => !(b.Location.X == TargetingData.SelfMainBasePoint.X && b.Location.Y == TargetingData.SelfMainBasePoint.Y) && !(b.Location.X == TargetingData.NaturalBasePoint.X && b.Location.Y == TargetingData.NaturalBasePoint.Y)))
                {
                    var pylonLocation = ProtectNexusPylonPlacement.FindPlacement(data.Location, UnitTypes.PROTOSS_PYLON, 1);
                    if (pylonLocation != null)
                    {
                        var action = Build(pylonLocation, Abilities.BUILD_PYLON);
                        if (action != null)
                        {
                            return action;
                        }
                    }
                }
            }

            if (MacroData.DesiredExtraBaseSimCityCannons > 0 && MacroData.Minerals >= 150 && UnitCountService.Completed(UnitTypes.PROTOSS_FORGE) > 0)
            {
                foreach (var data in BaseData.SelfBases.Where(b => !(b.Location.X == TargetingData.SelfMainBasePoint.X && b.Location.Y == TargetingData.SelfMainBasePoint.Y) && !(b.Location.X == TargetingData.NaturalBasePoint.X && b.Location.Y == TargetingData.NaturalBasePoint.Y)))
                {
                    var cannonLocation = ProtectNexusCannonPlacement.FindPlacement(data.Location, UnitTypes.PROTOSS_PHOTONCANNON, 1);
                    if (cannonLocation != null)
                    {
                        var action = Build(cannonLocation, Abilities.BUILD_PHOTONCANNON);
                        if (action != null && action.Count() > 0)
                        {
                            return action;
                        }
                    }
                }
            }

            if (MacroData.DesiredExtraBaseSimCityBatteries > 0 && MacroData.Minerals >= 100 && UnitCountService.Completed(UnitTypes.PROTOSS_CYBERNETICSCORE) > 0)
            {
                foreach (var data in BaseData.SelfBases.Where(b => !(b.Location.X == TargetingData.SelfMainBasePoint.X && b.Location.Y == TargetingData.SelfMainBasePoint.Y) && !(b.Location.X == TargetingData.NaturalBasePoint.X && b.Location.Y == TargetingData.NaturalBasePoint.Y)))
                {
                    var cannonLocation = ProtectNexusBatteryPlacement.FindPlacement(data.Location, UnitTypes.PROTOSS_CYBERNETICSCORE, 1);
                    if (cannonLocation != null)
                    {
                        var action = Build(cannonLocation, Abilities.BUILD_SHIELDBATTERY);
                        if (action != null && action.Count() > 0)
                        {
                            return action;
                        }
                    }
                }
            }

            return null;
        }

        private IEnumerable<SC2APIProtocol.Action> Build(Point2D location, Abilities ability)
        {
            if (ActiveUnitData.Commanders.Values.Any(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_PROBE && c.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)ability && o.TargetWorldSpacePos.X == location.X && o.TargetWorldSpacePos.Y == location.Y)))
            {
                return null;
            }

            var worker = WorkerBuilderService.GetWorker(location);
            if (worker != null)
            {
                worker.UnitRole = UnitRole.Build;
                return worker.Order(MacroData.Frame, ability, location);
            }
            return null;
        }
    }
}