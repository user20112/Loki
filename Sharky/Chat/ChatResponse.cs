using System;
using System.Collections.Generic;

namespace Sharky.Chat
{
    [Serializable]
    public class ChatResponse
    {
        public double confidence { get; set; }
        public dynamic metadata { get; set; }
        public List<string> response { get; set; }
    }
}