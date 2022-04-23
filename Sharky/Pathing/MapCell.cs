namespace Sharky.Pathing
{
    public class MapCell
    {
        public bool Buildable { get; set; }
        public bool CurrentlyBuildable { get; set; }
        public float EnemyAirDpsInRange { get; set; }
        public float EnemyAirSplashDpsInRange { get; set; }
        public float EnemyGroundDpsInRange { get; set; }
        public float EnemyGroundSplashDpsInRange { get; set; }
        public float EnemyZerglingDps { get; set; }
        public bool HasCreep { get; set; }
        public bool InEnemyDetection { get; set; }
        public bool InEnemyVision { get; set; }
        public bool InSelfDetection { get; set; }
        public bool InSelfVision { get; set; }
        public int LastFrameAlliesTouched { get; set; }
        public int LastFrameVisibility { get; set; }
        public int NumberOfAllies { get; set; }
        public int NumberOfEnemies { get; set; }
        public bool PathBlocked { get; set; }
        public bool PoweredBySelfPylon { get; set; }
        public float SelfAirDpsInRange { get; set; }
        public float SelfGroundDpsInRange { get; set; }
        public int TerrainHeight { get; set; }
        public int Visibility { get; set; }
        public bool Walkable { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}