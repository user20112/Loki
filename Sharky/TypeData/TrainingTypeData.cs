using System.Collections.Generic;

namespace Sharky
{
    public class TrainingTypeData
    {
        public Abilities Ability { get; set; }
        public int Food { get; set; }
        public int Gas { get; set; }
        public bool IsAddOn { get; set; }
        public int Minerals { get; set; }
        public HashSet<UnitTypes> ProducingUnits { get; set; }
        public bool RequiresTechLab { get; set; }
        public Abilities WarpInAbility { get; set; }
    }
}