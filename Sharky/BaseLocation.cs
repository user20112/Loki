﻿using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky
{
    public class BaseLocation
    {
        public BaseLocation()
        {
            MineralFields = new List<Unit>();
            VespeneGeysers = new List<Unit>();
        }

        public Point2D BehindMineralLineLocation { get; set; }
        public List<MiningInfo> GasMiningInfo { get; set; }
        public Point2D Location { get; set; }
        public List<Unit> MineralFields { get; set; }
        public Point2D MineralLineBuildingLocation { get; set; }
        public int MineralLineDefenseUnbuildableFrame { get; set; }
        public Point2D MineralLineLocation { get; set; }
        public List<MiningInfo> MineralMiningInfo { get; set; }
        public Unit ResourceCenter { get; set; }
        public List<Unit> VespeneGeysers { get; set; }
    }
}