﻿using System.Collections.Generic;

namespace Sharky.MicroTasks.Harass
{
    public class HarassInfo
    {
        public BaseLocation BaseLocation { get; set; }
        public List<UnitCommander> Harassers { get; set; }
        public int LastClearedFrame { get; set; }
        public int LastDefendedFrame { get; set; }
        public int LastPathFailedFrame { get; set; }
    }
}