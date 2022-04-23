using Sharky.MicroControllers;
using Sharky.MicroTasks.Attack;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroTasks
{
    public class AttackTask : MicroTask
    {
        private ActiveUnitData ActiveUnitData;
        private ArmySplitter ArmySplitter;
        private AttackData AttackData;
        private DefenseService DefenseService;
        private EnemyCleanupService EnemyCleanupService;
        private MacroData MacroData;
        private IMicroController MicroController;
        private MicroTaskData MicroTaskData;
        private SharkyUnitData SharkyUnitData;
        private TargetingData TargetingData;
        private TargetingService TargetingService;

        public AttackTask(IMicroController microController, TargetingData targetingData, ActiveUnitData activeUnitData, DefenseService defenseService, MacroData macroData, AttackData attackData, TargetingService targetingService, MicroTaskData microTaskData, SharkyUnitData sharkyUnitData, ArmySplitter armySplitter, EnemyCleanupService enemyCleanupService, float priority)
        {
            MicroController = microController;
            TargetingData = targetingData;
            ActiveUnitData = activeUnitData;
            DefenseService = defenseService;
            MacroData = macroData;
            AttackData = attackData;
            TargetingService = targetingService;
            MicroTaskData = microTaskData;
            SharkyUnitData = sharkyUnitData;
            ArmySplitter = armySplitter;
            EnemyCleanupService = enemyCleanupService;
            Priority = priority;

            UnitCommanders = new List<UnitCommander>();

            Enabled = true;
        }

        public override void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        {
            foreach (var commander in commanders)
            {
                if (!commander.Value.Claimed && commander.Value.UnitRole == UnitRole.Attack && commander.Value.UnitCalculation.UnitClassifications.Contains(UnitClassification.ArmyUnit))
                {
                    commander.Value.Claimed = true;
                    UnitCommanders.Add(commander.Value);
                }
            }
        }

        public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            var actions = new List<SC2APIProtocol.Action>();
            var hiddenBase = TargetingData.HiddenEnemyBase;
            AttackData.ArmyPoint = TargetingService.GetArmyPoint(UnitCommanders);
            TargetingData.AttackPoint = TargetingService.UpdateAttackPoint(AttackData.ArmyPoint, TargetingData.AttackPoint);

            var attackingEnemies = ActiveUnitData.SelfUnits.Where(u => u.Value.UnitClassifications.Contains(UnitClassification.ResourceCenter) || u.Value.UnitClassifications.Contains(UnitClassification.ProductionStructure)).SelectMany(u => u.Value.NearbyEnemies).Distinct();
            if (attackingEnemies.Count() > 0)
            {
                var armyPoint = new Vector2(AttackData.ArmyPoint.X, AttackData.ArmyPoint.Y);
                var distanceToAttackPoint = Vector2.DistanceSquared(armyPoint, new Vector2(TargetingData.AttackPoint.X, TargetingData.AttackPoint.Y));
                var closerEnemies = attackingEnemies.Where(e => Vector2.DistanceSquared(e.Position, armyPoint) < distanceToAttackPoint);
                if (!AttackData.Attacking)
                {
                    closerEnemies = attackingEnemies;
                }
                if (closerEnemies.Count() > 0)
                {
                    actions = ArmySplitter.SplitArmy(frame, closerEnemies, TargetingData.AttackPoint, UnitCommanders, false);
                    RemoveTemporaryUnits();
                    return actions;
                }
            }

            if (!hiddenBase && TargetingData.HiddenEnemyBase)
            {
                ResetClaimedUnits();
                if (MicroTaskData.MicroTasks.ContainsKey("FindHiddenBaseTask"))
                {
                    MicroTaskData.MicroTasks["FindHiddenBaseTask"].Enable();
                }
            }
            else if (!TargetingData.HiddenEnemyBase)
            {
                if (MicroTaskData.MicroTasks.ContainsKey("FindHiddenBaseTask") && MicroTaskData.MicroTasks["FindHiddenBaseTask"].Enabled)
                {
                    MicroTaskData.MicroTasks["FindHiddenBaseTask"].Disable();
                }
            }

            if (AttackData.Attacking)
            {
                actions = MicroController.Attack(UnitCommanders, TargetingData.AttackPoint, TargetingData.ForwardDefensePoint, AttackData.ArmyPoint, frame);
                RemoveTemporaryUnits();
                return actions;
            }
            else
            {
                var cleanupActions = EnemyCleanupService.CleanupEnemies(UnitCommanders, TargetingData.ForwardDefensePoint, AttackData.ArmyPoint, frame);
                if (cleanupActions != null)
                {
                    actions = cleanupActions;
                }
                else
                {
                    actions = MicroController.Retreat(UnitCommanders, TargetingData.ForwardDefensePoint, AttackData.ArmyPoint, frame);
                }

                RemoveTemporaryUnits();
                return actions;
            }
        }

        private void RemoveTemporaryUnits()
        {
            UnitCommanders.RemoveAll(u => SharkyUnitData.UndeadTypes.Contains((UnitTypes)u.UnitCalculation.Unit.UnitType));
        }
    }
}