﻿using System;
using System.Collections.Generic;

namespace Sharky.Builds.BuildChoosing
{
    public class Record
    {
        public List<DateTime> Losses { get; set; }
        public List<DateTime> Ties { get; set; }
        public List<DateTime> Wins { get; set; }
    }
}