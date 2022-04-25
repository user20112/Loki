﻿using SC2APIProtocol;
using System;
using System.Linq;

namespace Sharky.Builds
{
    public class BuildNothing : SharkyBuild
    {
        public BuildNothing(Sharky.LokiBot.BaseLokiBot lokiBot) : base(lokiBot)
        {
        }

        public override void OnFrame(ResponseObservation observation)
        {
            var positions = "";
            var nexuss = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_NEXUS);
            foreach (var nexus in nexuss)
            {
                positions += $"nexus {nexus.UnitCalculation.Position}, ";
            }

            var pylons = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_PYLON);
            foreach (var pylon in pylons)
            {
                positions += $"pylon {pylon.UnitCalculation.Position}, ";
            }

            var gateways = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_GATEWAY);
            foreach (var gatway in gateways)
            {
                positions += $"gateway {gatway.UnitCalculation.Position}, ";
            }

            var cybers = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.PROTOSS_CYBERNETICSCORE);
            foreach (var cybercore in cybers)
            {
                positions += $"cybercore {cybercore.UnitCalculation.Position}, ";
            }

            if (MacroData.Minerals == 1000)
            {
                Console.WriteLine($"1000 minerals at {observation.Observation.GameLoop} frames");
            }

            if (MacroData.Frame == SharkyOptions.FramesPerSecond * 60 * 5)
            {
                Console.WriteLine($"{MacroData.Minerals} minerals at {observation.Observation.GameLoop} frames, {FrameToTimeConverter.GetTime((int)observation.Observation.GameLoop)}");
            }
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            BuildOptions.StrictGasCount = true;
            BuildOptions.StrictSupplyCount = true;
            BuildOptions.StrictWorkerCount = true;

            AttackData.CustomAttackFunction = true;
            AttackData.Attacking = false;
            AttackData.UseAttackDataManager = false;
        }
    }
}