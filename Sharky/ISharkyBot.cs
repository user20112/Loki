using SC2APIProtocol;
using System;
using System.Collections.Generic;

namespace Sharky
{
    public interface ISharkyBot
    {
        void OnEnd(ResponseObservation observation, Result result);

        IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation);

        void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, String opponentId);
    }
}