using System.Collections.Generic;

namespace Sharky.MicroTasks
{
    public class RepairData
    {
        public RepairData(UnitCalculation unit)
        {
            UnitToRepair = unit;
            Repairers = new List<UnitCommander>();
            DesiredRepairers = 0;
        }

        public int DesiredRepairers { get; set; }
        public List<UnitCommander> Repairers { get; set; }
        public UnitCalculation UnitToRepair { get; set; }
    }
}