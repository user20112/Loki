using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sharky.Chat
{
    public interface IChatDataService
    {
        List<ChatData> DefaultChataData { get; }

        List<string> GetChatMessage(ChatData chatData, Match matchData, string enemyName);

        ChatTypeData GetChatTypeData(string chatType);

        List<string> GetChatTypeMessage(ChatTypeData chatTypeData, string enemyName);
    }
}