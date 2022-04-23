﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sharky.MicroTasks
{
    public abstract class MicroTask : IMicroTask
    {
        public bool Enabled { get; protected set; }
        public float Priority { get; set; }
        public List<UnitCommander> UnitCommanders { get; set; }

        public virtual void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders)
        {
            throw new NotImplementedException();
        }

        public virtual void Disable()
        {
            foreach (var commander in UnitCommanders)
            {
                commander.Claimed = false;
                commander.UnitRole = UnitRole.None;
            }
            UnitCommanders = new List<UnitCommander>();

            Enabled = false;
        }

        public virtual void Enable()
        {
            Enabled = true;
        }

        public virtual IEnumerable<SC2APIProtocol.Action> PerformActions(int frame)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveDeadUnits(List<ulong> deadUnits)
        {
            foreach (var tag in deadUnits)
            {
                UnitCommanders.RemoveAll(c => c.UnitCalculation.Unit.Tag == tag);
            }
        }

        public virtual void ResetClaimedUnits()
        {
            foreach (var commander in UnitCommanders)
            {
                commander.Claimed = false;
            }
            UnitCommanders = new List<UnitCommander>();
        }
    }
}