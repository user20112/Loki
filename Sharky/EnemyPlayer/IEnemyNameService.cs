using System.Collections.Generic;

namespace Sharky.EnemyPlayer
{
    public interface IEnemyNameService
    {
        string GetEnemyNameFromId(string id, List<EnemyPlayer> enemies);

        string GetNameFromChat(string chat, List<EnemyPlayer> enemies);

        string GetNameFromGame(Game game, List<EnemyPlayer> enemies);
    }
}