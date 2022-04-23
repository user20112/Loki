﻿namespace Sharky.MicroTasks.Harass
{
    public class DenyExpansionsTask : MicroTask
    {
        //private BaseData BaseData;
        //private MicroData MicroData;
        //private bool Started;
        //private TargetingData TargetingData;

        //public DenyExpansionsTask(LokiBot.LokiBot lokiBot, bool enabled = true, float priority = -1f)
        //{
        //    BaseData = lokiBot.BaseData;
        //    MicroData = lokiBot.MicroData;
        //    TargetingData = lokiBot.TargetingData;

        //    Priority = priority;
        //    Enabled = enabled;

        //    UnitCommanders = new List<UnitCommander>();

        //    DesiredUnitsClaims = new List<DesiredUnitsClaim>();
        //    Started = false;
        //    DisableAfterFailure = true;
        //}

        //public List<DesiredUnitsClaim> DesiredUnitsClaims { get; set; }

        //public bool DisableAfterFailure { get; set; }
        //private List<HarassGroupInfo> HarassGroupInfo { get; set; }

        //public override void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        //{
        //    if (HarassGroupInfo == null) { return; }
        //    foreach (var commander in commanders)
        //    {
        //        if (!commander.Value.Claimed)
        //        {
        //            var unitType = commander.Value.UnitCalculation.Unit.UnitType;
        //            foreach (var desiredUnitClaim in DesiredUnitsClaims)
        //            {
        //                if ((uint)desiredUnitClaim.UnitType == unitType && !commander.Value.UnitCalculation.Unit.IsHallucination && UnitCommanders.Count(u => u.UnitCalculation.Unit.UnitType == (uint)desiredUnitClaim.UnitType) < DesiredUnitsClaims.Where(c => (uint)c.UnitType == unitType).Sum(c => c.Count))
        //                {
        //                    commander.Value.Claimed = true;
        //                    commander.Value.UnitRole = UnitRole.None;
        //                    UnitCommanders.Add(commander.Value);
        //                    Started = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        //public override IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        //{
        //    var commands = new List<SC2APIProtocol.Action>();

        //    AssignHarassers();

        //    foreach (var harassGroupInfo in HarassGroupInfo)
        //    {
        //        foreach (var commander in harassGroupInfo.HarassInfo.Harassers)
        //        {
        //            // guard that expansion, if worker or floating CC/OC is nearby, body block the spot, stay within radius that would block building
        //            // prioritize killing workers, especially scv's building
        //            var distanceSquared = Vector2.DistanceSquared(commander.UnitCalculation.Position, new Vector2(harassGroupInfo.HarassInfo.BaseLocation.Location.X, harassGroupInfo.HarassInfo.BaseLocation.Location.Y));
        //            var microController = GetMicroController(commander);

        //            if (distanceSquared > 100)
        //            {
        //                if (commander.UnitCalculation.EnemiesThreateningDamage.Count() > 0)
        //                {
        //                    if (Vector2.DistanceSquared(commander.UnitCalculation.Position, new Vector2(TargetingData.ForwardDefensePoint.X, TargetingData.ForwardDefensePoint.Y)) < 225)
        //                    {
        //                        var defendAction = microController.Attack(commander, harassGroupInfo.HarassInfo.BaseLocation.Location, TargetingData.ForwardDefensePoint, null, frame);
        //                        if (defendAction != null)
        //                        {
        //                            commands.AddRange(defendAction);
        //                            continue;
        //                        }
        //                    }

        //                    var action = microController.NavigateToPoint(commander, harassGroupInfo.HarassInfo.BaseLocation.Location, TargetingData.ForwardDefensePoint, null, frame);
        //                    if (action != null)
        //                    {
        //                        commands.AddRange(action);
        //                    }
        //                }
        //                else
        //                {
        //                    var action = commander.Order(frame, Abilities.MOVE, harassGroupInfo.HarassInfo.BaseLocation.Location);
        //                    if (action != null)
        //                    {
        //                        commands.AddRange(action);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (distanceSquared > 4 && commander.UnitCalculation.NearbyEnemies.Any(e => e.UnitClassifications.Contains(UnitClassification.ResourceCenter) || e.UnitClassifications.Contains(UnitClassification.Worker)))
        //                {
        //                    var enemy = GetEnemyBuildingScv(commander.UnitCalculation.NearbyEnemies);
        //                    if (enemy != null)
        //                    {
        //                        var attackAction = commander.Order(frame, Abilities.ATTACK, targetTag: enemy.Unit.Tag);
        //                        if (attackAction != null)
        //                        {
        //                            commands.AddRange(attackAction);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // if worker or floating CC / OC is nearby, body block the spot
        //                        var action = commander.Order(frame, Abilities.MOVE, harassGroupInfo.HarassInfo.BaseLocation.Location);
        //                        if (action != null)
        //                        {
        //                            commands.AddRange(action);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var action = microController.HarassWorkers(commander, harassGroupInfo.HarassInfo.BaseLocation.Location, TargetingData.ForwardDefensePoint, frame);
        //                    if (action != null)
        //                    {
        //                        commands.AddRange(action);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return commands;
        //}

        //public override void RemoveDeadUnits(List<ulong> deadUnits)
        //{
        //    foreach (var tag in deadUnits)
        //    {
        //        UnitCommanders.RemoveAll(c => c.UnitCalculation.Unit.Tag == tag);
        //        if (HarassGroupInfo != null)
        //        {
        //            foreach (var harassInfo in HarassGroupInfo)
        //            {
        //                harassInfo.HarassInfo.Harassers.RemoveAll(h => h.UnitCalculation.Unit.Tag == tag);
        //            }
        //        }
        //    }
        //    if (DisableAfterFailure && Started && UnitCommanders.Count == 0)
        //    {
        //        Disable();
        //    }
        //}

        //private void AssignHarassers()
        //{
        //    if (HarassGroupInfo == null)
        //    {
        //        HarassGroupInfo = new List<HarassGroupInfo>();
        //    }

        //    foreach (var baseLocation in BaseData.EnemyBaseLocations.Where(b => (b.ResourceCenter == null || b.ResourceCenter.BuildProgress < 1) && !HarassGroupInfo.Any(i => i.HarassInfo.BaseLocation.Location.X == b.Location.X && i.HarassInfo.BaseLocation.Location.Y == b.Location.Y)))
        //    {
        //        if (HarassGroupInfo.Count() >= DesiredUnitsClaims.Count()) { break; }
        //        var group = DesiredUnitsClaims.FirstOrDefault(c => !HarassGroupInfo.Any(h => h.DesiredHarassers == c));
        //        if (group != null)
        //        {
        //            HarassGroupInfo.Add(new HarassGroupInfo { DesiredHarassers = group, HarassInfo = new HarassInfo { BaseLocation = baseLocation, Harassers = new List<UnitCommander>(), LastClearedFrame = -1, LastDefendedFrame = -1, LastPathFailedFrame = -1 } });
        //        }
        //    }

        //    foreach (var baseLocation in BaseData.SelfBases)
        //    {
        //        HarassGroupInfo.RemoveAll(h => h.HarassInfo.BaseLocation.Location.X == baseLocation.Location.X && h.HarassInfo.BaseLocation.Location.Y == baseLocation.Location.Y);
        //    }
        //    foreach (var baseLocation in BaseData.EnemyBases.Where(b => b.ResourceCenter != null && b.ResourceCenter.BuildProgress >= 1 && !b.ResourceCenter.IsFlying))
        //    {
        //        HarassGroupInfo.RemoveAll(h => h.HarassInfo.BaseLocation.Location.X == baseLocation.Location.X && h.HarassInfo.BaseLocation.Location.Y == baseLocation.Location.Y);
        //    }

        //    if (HarassGroupInfo.Count() > 0 && UnitCommanders.Any(u => u.UnitRole == UnitRole.None))
        //    {
        //        var unasignedCommanders = UnitCommanders.Where(u => u.UnitRole == UnitRole.None).ToList();
        //        if (unasignedCommanders.Count() > 0)
        //        {
        //            foreach (var info in HarassGroupInfo)
        //            {
        //                var commander = unasignedCommanders.First();
        //                var unitType = commander.UnitCalculation.Unit.UnitType;
        //                if (info.DesiredHarassers.Count > info.HarassInfo.Harassers.Count())
        //                {
        //                    if ((uint)info.DesiredHarassers.UnitType == unitType)
        //                    {
        //                        unasignedCommanders.Remove(commander);
        //                        commander.UnitRole = UnitRole.Harass;
        //                        info.HarassInfo.Harassers.Add(commander);
        //                        if (unasignedCommanders.Count() == 0)
        //                        {
        //                            return;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private UnitCalculation GetEnemyBuildingScv(List<UnitCalculation> enemies)
        //{
        //    var unfinishedBuilding = enemies.FirstOrDefault(e => e.Unit.BuildProgress < 1);
        //    if (unfinishedBuilding != null)
        //    {
        //        var scv = enemies.Where(e => e.Unit.UnitType == (uint)UnitTypes.TERRAN_SCV).OrderBy(e => Vector2.DistanceSquared(e.Position, unfinishedBuilding.Position)).FirstOrDefault();
        //        if (scv != null)
        //        {
        //            return scv;
        //        }
        //    }
        //    return null;
        //}

        //private IIndividualMicroController GetMicroController(UnitCommander commander)
        //{
        //    if (MicroData.IndividualMicroControllers.TryGetValue((UnitTypes)commander.UnitCalculation.Unit.UnitType, out var individualMicroController))
        //    {
        //        return individualMicroController;
        //    }
        //    return MicroData.IndividualMicroController;
        //}
    }
}