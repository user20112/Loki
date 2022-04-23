using System.Collections.Generic;

namespace Sharky.Pathing
{
    public class ChokePoints
    {
        public ChokePoints()
        {
            Good = new List<ChokePoint>();
            Neutral = new List<ChokePoint>();
            Bad = new List<ChokePoint>();
        }

        public List<ChokePoint> Bad { get; set; }
        public List<ChokePoint> Good { get; set; }
        public List<ChokePoint> Neutral { get; set; }
    }
}