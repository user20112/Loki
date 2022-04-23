namespace Sharky.EnemyStrategies
{
    public interface IEnemyStrategy
    {
        bool Active { get; }

        bool Detected { get; }

        string Name();

        void OnFrame(int frame);
    }
}