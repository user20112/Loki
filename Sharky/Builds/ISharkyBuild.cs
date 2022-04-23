using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky.Builds
{
    public interface ISharkyBuild
    {
        List<string> CounterTransition(int frame);

        void EndBuild(int frame);

        string Name();

        void OnFrame(ResponseObservation observation);

        void StartBuild(int frame);

        bool Transition(int frame);
    }
}