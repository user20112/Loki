using System;

namespace Sharky.Chat
{
    [Serializable]
    public class Chat
    {
        public string botName { get; set; }
        public string message { get; set; }
        public long time { get; set; }
        public string user { get; set; }
    }
}