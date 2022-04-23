using System.Collections.Generic;

namespace Sharky
{
    public class Tournament
    {
        public Dictionary<string, Dictionary<string, List<List<string>>>> BuildSequences { get; set; }
        public bool Enabled { get; set; }
        public string Folder { get; set; }
    }
}