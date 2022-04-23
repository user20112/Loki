﻿using System.Collections.Generic;

namespace Sharky.Counter
{
    public class CounterInfo
    {
        public List<CounterUnit> EfficiencyAgainst { get; set; }
        public List<CounterUnit> StrongAgainst { get; set; }
        public List<CounterUnit> SupportAgainst { get; set; }
        public List<CounterUnit> WeakAgainst { get; set; }
        // TODO: set efficiency against values
    }
}