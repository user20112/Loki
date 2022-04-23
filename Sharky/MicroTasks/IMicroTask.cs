using SC2APIProtocol;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sharky.MicroTasks
{
    public interface IMicroTask
    {
        bool Enabled { get; }
        float Priority { get; set; }
        List<UnitCommander> UnitCommanders { get; set; }

        void ClaimUnits(ConcurrentDictionary<ulong, UnitCommander> commanders);

        void Disable();

        void Enable();

        IEnumerable<Action> PerformActions(int frame);

        void RemoveDeadUnits(List<ulong> deadUnits);

        void ResetClaimedUnits();
    }
}