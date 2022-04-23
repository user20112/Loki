using System.Collections.Generic;

namespace Sharky
{
    public class ChronoData
    {
        public ChronoData()
        {
            ChronodUnits = new HashSet<UnitTypes>();
            ChronodUpgrades = new HashSet<Upgrades>();
        }

        public HashSet<UnitTypes> ChronodUnits { get; set; }
        public HashSet<Upgrades> ChronodUpgrades { get; set; }
    }
}