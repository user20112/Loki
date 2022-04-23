using System;

namespace Sharky
{
    public class FrameToTimeConverter
    {
        private SharkyOptions SharkyOptions;

        public FrameToTimeConverter(SharkyOptions sharkyOptions)
        {
            SharkyOptions = sharkyOptions;
        }

        public TimeSpan GetTime(int frame)
        {
            return TimeSpan.FromSeconds(frame / SharkyOptions.FramesPerSecond);
        }
    }
}