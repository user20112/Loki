﻿using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky.Counter
{
    public class UnitCounterData
    {
        public UnitCounterData(Unit unit)
        {
            Unit = unit;
            CounterUnits = new List<CounterUnit>();
        }

        public List<CounterUnit> CounterUnits { get; set; }
        public Unit Unit { get; set; }
    }
}