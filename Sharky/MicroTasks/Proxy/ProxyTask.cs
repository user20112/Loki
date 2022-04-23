﻿using SC2APIProtocol;
using Sharky.MicroControllers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroTasks
{
    public class ProxyTask : MicroTask
    {
        private ActiveUnitData ActiveUnitData;
        private DebugService DebugService;
        private IIndividualMicroController IndividualMicroController;
        private MacroData MacroData;
        private MicroTaskData MicroTaskData;
        private SharkyUnitData SharkyUnitData;

        public ProxyTask(SharkyUnitData sharkyUnitData, bool enabled, float priority, MacroData macroData, string proxyName, MicroTaskData microTaskData, DebugService debugService, ActiveUnitData activeUnitData, IIndividualMicroController individualMicroController, int desiredWorkers = 1)
        {
            SharkyUnitData = sharkyUnitData;
            Priority = priority;
            MacroData = macroData;
            ProxyName = proxyName;
            MicroTaskData = microTaskData;
            ActiveUnitData = activeUnitData;
            DebugService = debugService;
            IndividualMicroController = individualMicroController;

            UnitCommanders = new List<UnitCommander>();
            Enabled = enabled;
            DesiredWorkers = desiredWorkers;
        }

        public int DesiredWorkers { get; set; }

        public string ProxyName { get; set; }

        private bool started { get; set; }

        public override void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        {
            if (UnitCommanders.Count() < DesiredWorkers)
            {
                if (started && DesiredWorkers == 1)
                {
                    Disable();
                    return;
                }

                var commander = ActiveUnitData.Commanders.Values.Where(c => c.UnitRole == UnitRole.Build && c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_SCV && c.UnitCalculation.Unit.Orders.Any(o => ActiveUnitData.SelfUnits.Values.Any(s => s.Attributes.Contains(Attribute.Structure) && s.Unit.BuildProgress == 1 && o.TargetWorldSpacePos != null && s.Position.X == o.TargetWorldSpacePos.X && s.Position.Y == o.TargetWorldSpacePos.Y))).Concat(
                    ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.UnitClassifications.Contains(UnitClassification.Worker) && !c.UnitCalculation.Unit.BuffIds.Any(b => SharkyUnitData.CarryingResourceBuffs.Contains((Buffs)b))).Where(c => (c.UnitRole == UnitRole.PreBuild || c.UnitRole == UnitRole.None || c.UnitRole == UnitRole.Minerals) && !c.UnitCalculation.Unit.Orders.Any(o => SharkyUnitData.BuildingData.Values.Any(b => (uint)b.Ability == o.AbilityId))))
                    .OrderBy(p => Vector2.DistanceSquared(p.UnitCalculation.Position, new Vector2(MacroData.Proxies[ProxyName].Location.X, MacroData.Proxies[ProxyName].Location.Y))).FirstOrDefault();

                if (commander != null)
                {
                    commander.UnitRole = UnitRole.Proxy;
                    commander.Claimed = true;
                    UnitCommanders.Add(commander);
                    started = true;
                    return;
                }
            }
        }

        public override void Disable()
        {
            foreach (var commander in UnitCommanders)
            {
                commander.Claimed = false;
                commander.UnitRole = UnitRole.None;
            }
            UnitCommanders = new List<UnitCommander>();

            Enabled = false;
            if (MacroData.Proxies.ContainsKey(ProxyName))
            {
                MacroData.Proxies[ProxyName].Enabled = false;
            }
        }

        public override void Enable()
        {
            Enabled = true;
            started = false;
            if (MacroData.Proxies.ContainsKey(ProxyName))
            {
                MacroData.Proxies[ProxyName].Enabled = true;
            }
        }

        public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            var commands = new List<SC2APIProtocol.Action>();

            if (MacroData.Proxies.ContainsKey(ProxyName))
            {
                commands.AddRange(MoveToProxyLocation(frame));
            }

            return commands;
        }

        private float DistanceToResourceCenter(KeyValuePair<ulong, UnitCommander> commander)
        {
            var resourceCenter = commander.Value.UnitCalculation.NearbyAllies.Take(25).FirstOrDefault(a => a.UnitClassifications.Contains(UnitClassification.ResourceCenter));
            if (resourceCenter != null)
            {
                return Vector2.DistanceSquared(commander.Value.UnitCalculation.Position, resourceCenter.Position);
            }
            return 0;
        }

        private IEnumerable<SC2APIProtocol.Action> MoveToProxyLocation(int frame)
        {
            var commands = new List<SC2APIProtocol.Action>();

            foreach (var commander in UnitCommanders.Where(c => !c.UnitCalculation.Unit.Orders.Any(o => SharkyUnitData.BuildingData.Values.Any(b => (uint)b.Ability == o.AbilityId))))
            {
                if (commander.UnitRole != UnitRole.Proxy && commander.UnitRole != UnitRole.Build)
                {
                    commander.UnitRole = UnitRole.Proxy;
                }
                if (Vector2.DistanceSquared(new Vector2(MacroData.Proxies[ProxyName].Location.X, MacroData.Proxies[ProxyName].Location.Y), commander.UnitCalculation.Position) > MacroData.Proxies[ProxyName].MaximumBuildingDistance)
                {
                    List<SC2APIProtocol.Action> action;
                    if (IndividualMicroController.NavigateToTarget(commander, MacroData.Proxies[ProxyName].Location, null, null, Formation.Normal, frame, out action))
                    {
                        if (action != null)
                        {
                            commands.AddRange(action);
                        }
                    }
                }
            }

            return commands;
        }
    }
}