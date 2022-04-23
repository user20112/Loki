using System.Collections.Generic;

namespace Sharky.EnemyStrategies
{
    public class EnemyStrategyHistory
    {
        public EnemyStrategyHistory()
        {
            History = new Dictionary<int, string>();
        }

        public Dictionary<int, string> History { get; set; }
    }
}