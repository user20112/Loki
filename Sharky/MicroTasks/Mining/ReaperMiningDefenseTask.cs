using Sharky.MicroControllers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Sharky.MicroTasks.Mining
{
    public class ReaperMiningDefenseTask : MicroTask
    {
        private ActiveUnitData ActiveUnitData;
        private EnemyData EnemyData;
        private UnitCalculation EnemyReaper;
        private MineralWalker MineralWalker;

        public ReaperMiningDefenseTask(Sharky.LokiBot.LokiBot lokiBot, bool enabled, float priority)
        {
            ActiveUnitData = lokiBot.ActiveUnitData;
            EnemyData = lokiBot.EnemyData;
            MineralWalker = lokiBot.MineralWalker;

            Priority = priority;

            UnitCommanders = new List<UnitCommander>();
            Enabled = enabled;
        }

        public override void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        {
        }

        public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            if (EnemyData.EnemyRace == SC2APIProtocol.Race.Zerg || EnemyData.EnemyRace == SC2APIProtocol.Race.Protoss)
            {
                Disable();
            }

            var commands = new List<SC2APIProtocol.Action>();

            GetEnemyReaper();

            commands = DefendAgainstReaper(frame);

            UnitCommanders.RemoveAll(c => c.UnitRole != UnitRole.ChaseReaper);

            return commands;
        }

        public override void RemoveDeadUnits(List<ulong> deadUnits)
        {
            foreach (var tag in deadUnits)
            {
                if (EnemyReaper != null && EnemyReaper.Unit.Tag == tag)
                {
                    EnemyReaper = null;
                }
            }
            base.RemoveDeadUnits(deadUnits);
        }

        private void ClaimDefenders()
        {
            var worker = EnemyReaper.NearbyEnemies.FirstOrDefault(e => e.UnitClassifications.Contains(UnitClassification.Worker) &&
                e.NearbyAllies.Any(a => a.UnitClassifications.Contains(UnitClassification.ResourceCenter) &&
                ActiveUnitData.Commanders.ContainsKey(e.Unit.Tag) && ActiveUnitData.Commanders[e.Unit.Tag].UnitRole != UnitRole.ChaseReaper &&
                e.Unit.Health + e.Unit.Shield >= 40));

            if (worker != null)
            {
                ActiveUnitData.Commanders[worker.Unit.Tag].UnitRole = UnitRole.ChaseReaper;
                UnitCommanders.Add(ActiveUnitData.Commanders[worker.Unit.Tag]);
            }
        }

        private List<SC2APIProtocol.Action> DefendAgainstReaper(int frame)
        {
            var commands = new List<SC2APIProtocol.Action>();

            if (EnemyReaper != null && UnitCommanders.Count() < 3)
            {
                ClaimDefenders();
            }

            foreach (var commander in UnitCommanders)
            {
                if (EnemyReaper == null || commander.UnitCalculation.Unit.Health + commander.UnitCalculation.Unit.Shield <= 25 || !commander.UnitCalculation.NearbyAllies.Any(a => a.UnitClassifications.Contains(UnitClassification.ResourceCenter)))
                {
                    commander.UnitRole = UnitRole.None;
                    List<SC2APIProtocol.Action> action;
                    if (MineralWalker.MineralWalkHome(commander, frame, out action))
                    {
                        commands.AddRange(action);
                    }
                }
                else
                {
                    var action = commander.Order(frame, Abilities.ATTACK, targetTag: EnemyReaper.Unit.Tag);
                    if (action != null)
                    {
                        commands.AddRange(action);
                    }
                }
            }

            return commands;
        }

        private void GetEnemyReaper()
        {
            EnemyReaper = ActiveUnitData.EnemyUnits.Values.FirstOrDefault(e => e.Unit.UnitType == (uint)UnitTypes.TERRAN_REAPER && e.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.ResourceCenter)));
        }
    }
}