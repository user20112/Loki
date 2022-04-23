﻿using SC2APIProtocol;
using System;
using System.Collections.Generic;

namespace Sharky.Managers
{
    public interface IManager
    {
        bool NeverSkip { get; }
        bool SkipFrame { get; set; }

        void OnEnd(ResponseObservation observation, Result result);

        IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation);

        void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, String opponentId);
    }
}