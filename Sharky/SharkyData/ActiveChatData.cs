using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky
{
    public class ActiveChatData
    {
        public List<Action> ChatActions { get; set; }
        public string EnemyName { get; set; }
        public double TimeModulation { get; set; }
    }
}