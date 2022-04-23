﻿using SC2APIProtocol;
using Sharky.Builds.BuildingPlacement;
using Sharky.Chat;
using Sharky.Pathing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.Managers.Terran
{
    public class OrbitalManager : SharkyManager
    {
        private ActiveUnitData ActiveUnitData;
        private BaseData BaseData;
        private ChatService ChatService;
        private EnemyData EnemyData;
        private MacroData MacroData;
        private MapDataService MapDataService;
        private bool MulesUnderAttackChatSent;
        private ResourceCenterLocator ResourceCenterLocator;
        private SharkyUnitData SharkyUnitData;
        private UnitCountService UnitCountService;

        public OrbitalManager(ActiveUnitData activeUnitData, BaseData baseData, EnemyData enemyData, MacroData macroData, UnitCountService unitCountService, ChatService chatService, ResourceCenterLocator resourceCenterLocator, MapDataService mapDataService, SharkyUnitData sharkyUnitData)
        {
            ActiveUnitData = activeUnitData;
            BaseData = baseData;
            EnemyData = enemyData;
            MacroData = macroData;
            UnitCountService = unitCountService;
            ChatService = chatService;
            ResourceCenterLocator = resourceCenterLocator;
            MapDataService = mapDataService;
            SharkyUnitData = sharkyUnitData;

            MulesUnderAttackChatSent = false;

            ScanQueue = new Stack<Point2D>();
            LastScanFrame = 0;
        }

        public int LastScanFrame { get; private set; }
        public Stack<Point2D> ScanQueue { get; set; }

        public override IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation)
        {
            var actions = new List<SC2APIProtocol.Action>();

            var frame = (int)observation.Observation.GameLoop;

            var takeBaseAction = TakeBases(frame);
            if (takeBaseAction != null)
            {
                actions.AddRange(takeBaseAction);
            }

            var orbital = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_ORBITALCOMMAND && c.UnitCalculation.Unit.BuildProgress == 1).OrderByDescending(c => c.UnitCalculation.Unit.Energy).FirstOrDefault();
            if (orbital != null)
            {
                var action = Scan(orbital, frame);
                if (action != null)
                {
                    actions.AddRange(action);
                }
                else
                {
                    action = Mule(orbital, frame);
                    if (action != null)
                    {
                        actions.AddRange(action);
                    }
                }
            }

            return actions;
        }

        private List<SC2APIProtocol.Action> Mule(UnitCommander orbital, int frame)
        {
            if (orbital.UnitCalculation.Unit.Energy >= 50 && !EnemyData.EnemyStrategies["InvisibleAttacks"].Detected || orbital.UnitCalculation.Unit.Energy > 95)
            {
                var highestMineralPatch = BaseData.SelfBases.Where(b => b.ResourceCenter.BuildProgress > .99 && b.MineralFields.Count() > 0 && ActiveUnitData.SelfUnits.ContainsKey(b.ResourceCenter.Tag) && ActiveUnitData.SelfUnits[b.ResourceCenter.Tag].NearbyEnemies.Count(e => e.UnitClassifications.Contains(UnitClassification.ArmyUnit)) < 2).SelectMany(m => m.MineralFields).OrderByDescending(m => m.MineralContents).FirstOrDefault();
                if (highestMineralPatch != null)
                {
                    return orbital.Order(frame, Abilities.EFFECT_CALLDOWNMULE, targetTag: highestMineralPatch.Tag);
                }

                foreach (var baseLocation in BaseData.SelfBases.Where(b => b.ResourceCenter.BuildProgress == 1 && b.MineralFields.Count() > 0))
                {
                    var baseVector = new Vector2(baseLocation.Location.X, baseLocation.Location.Y);
                    var mineralPatch = baseLocation.MineralFields.OrderByDescending(m => Vector2.DistanceSquared(new Vector2(m.Pos.X, m.Pos.Y), baseVector)).ThenByDescending(m => m.MineralContents).FirstOrDefault();
                    if (mineralPatch != null)
                    {
                        if (!MulesUnderAttackChatSent)
                        {
                            MulesUnderAttackChatSent = true;
                            ChatService.SendChatType("MulesCalledWhileUnderAttack");
                        }
                        return orbital.Order(frame, Abilities.EFFECT_CALLDOWNMULE, targetTag: mineralPatch.Tag);
                    }
                }

                var visibleMineral = ActiveUnitData.NeutralUnits.FirstOrDefault(u => SharkyUnitData.MineralFieldTypes.Contains((UnitTypes)u.Value.Unit.UnitType) && u.Value.Unit.DisplayType == DisplayType.Visible).Value;
                if (visibleMineral != null)
                {
                    return orbital.Order(frame, Abilities.EFFECT_CALLDOWNMULE, targetTag: visibleMineral.Unit.Tag);
                }
            }

            return null;
        }

        private List<SC2APIProtocol.Action> Scan(UnitCommander orbital, int frame)
        {
            if (orbital.UnitCalculation.Unit.Energy >= 50)
            {
                var undetectedEnemy = ActiveUnitData.EnemyUnits.Where(e => e.Value.Unit.DisplayType == DisplayType.Hidden).OrderByDescending(e => e.Value.EnemiesInRangeOf.Count()).FirstOrDefault();
                if (undetectedEnemy.Value != null && undetectedEnemy.Value.EnemiesInRangeOf.Count() > 0)
                {
                    LastScanFrame = frame;
                    return orbital.Order(frame, Abilities.EFFECT_SCAN, new Point2D { X = undetectedEnemy.Value.Position.X, Y = undetectedEnemy.Value.Position.Y });
                }

                foreach (var siegedTank in ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_SIEGETANKSIEGED))
                {
                    if (siegedTank.BestTarget != null && siegedTank.UnitCalculation.Unit.WeaponCooldown < 0.1f && siegedTank.UnitCalculation.EnemiesInRange.Any(e => e.Unit.Tag == siegedTank.BestTarget.Unit.Tag) && frame - siegedTank.BestTarget.FrameLastSeen > 10 && !MapDataService.SelfVisible(siegedTank.BestTarget.Unit.Pos))
                    {
                        LastScanFrame = frame;
                        return orbital.Order(frame, Abilities.EFFECT_SCAN, new Point2D { X = siegedTank.BestTarget.Position.X, Y = siegedTank.BestTarget.Position.Y });
                    }
                }

                if (ScanQueue.Count() > 0)
                {
                    var scanPoint = ScanQueue.Pop();
                    LastScanFrame = frame;
                    return orbital.Order(frame, Abilities.EFFECT_SCAN, scanPoint);
                }
            }

            return null;
        }

        private List<SC2APIProtocol.Action> TakeBases(int frame)
        {
            var actions = new List<SC2APIProtocol.Action>();

            var excess = UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_COMMANDCENTER) - BaseData.SelfBases.Count() - MacroData.DesiredMacroCommandCenters;
            if (excess > 0)
            {
                var flyingOrbitals = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_ORBITALCOMMANDFLYING && c.UnitRole != UnitRole.Repair);
                var macroOrbitals = ActiveUnitData.Commanders.Values.Where(c => c.UnitCalculation.Unit.UnitType == (uint)UnitTypes.TERRAN_ORBITALCOMMAND && !BaseData.SelfBases.Any(b => b.ResourceCenter != null && b.ResourceCenter.Tag == c.UnitCalculation.Unit.Tag));
                if (excess > flyingOrbitals.Count() && macroOrbitals.Count() > 0)
                {
                    actions.AddRange(macroOrbitals.FirstOrDefault().Order(frame, Abilities.CANCEL_LAST));
                    actions.AddRange(macroOrbitals.FirstOrDefault().Order(frame, Abilities.LIFT, queue: true));
                    return actions;
                }
                else
                {
                    foreach (var flyingOrbital in flyingOrbitals)
                    {
                        if (!flyingOrbital.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)Abilities.LAND || o.AbilityId == (uint)Abilities.LAND_ORBITALCOMMAND))
                        {
                            var location = ResourceCenterLocator.GetResourceCenterLocation(false);
                            if (location != null)
                            {
                                actions.AddRange(flyingOrbital.Order(frame, Abilities.LAND, location));
                                return actions;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}