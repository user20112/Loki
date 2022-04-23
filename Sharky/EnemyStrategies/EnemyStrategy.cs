using Sharky.Chat;
using System;

namespace Sharky.EnemyStrategies
{
    public abstract class EnemyStrategy : IEnemyStrategy
    {
        protected ActiveUnitData ActiveUnitData;
        protected ChatService ChatService;
        protected DebugService DebugService;
        protected EnemyStrategyHistory EnemyStrategyHistory;
        protected FrameToTimeConverter FrameToTimeConverter;
        protected SharkyOptions SharkyOptions;
        protected UnitCountService UnitCountService;
        public bool Active { get; private set; }
        public bool Detected { get; private set; }

        public string Name()
        {
            return GetType().Name;
        }

        public void OnFrame(int frame)
        {
            var detected = Detect(frame);

            if (detected)
            {
                Active = true;

                if (!Detected)
                {
                    EnemyStrategyHistory.History[frame] = Name();
                    Console.WriteLine($"{frame} {FrameToTimeConverter.GetTime(frame)} Detected: {Name()}");
                    Detected = true;
                    DetectedChat();
                }
                DebugService.DrawText($"Active: {Name()}");
            }
            else
            {
                Active = false;
            }
        }

        protected abstract bool Detect(int frame);

        protected void DetectedChat()
        {
            ChatService.SendChatType($"{Name()}-EnemyStrategy");
            if (SharkyOptions.TagsEnabled)
            {
                ChatService.SendAllyChatMessage($"Tag:EnemyStrategy-{Name()}", true);
            }
        }
    }
}