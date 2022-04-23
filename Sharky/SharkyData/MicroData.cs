using Sharky.MicroControllers;
using System.Collections.Generic;

namespace Sharky
{
    public class MicroData
    {
        public IIndividualMicroController IndividualMicroController { get; set; }
        public Dictionary<UnitTypes, IIndividualMicroController> IndividualMicroControllers { get; set; }
    }
}