namespace Sharky
{
    public class TargetPriorityCalculation
    {
        public float AirWinnability { get; set; }
        public int FrameCalculated { get; set; }
        public float GroundWinnability { get; set; }
        public float OverallWinnability { get; set; }
        public bool Overwhelm { get; set; }
        public TargetPriority TargetPriority { get; set; }
    }
}