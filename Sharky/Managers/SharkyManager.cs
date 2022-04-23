﻿using SC2APIProtocol;
using System.Collections.Generic;

namespace Sharky.Managers
{
    public abstract class SharkyManager : IManager
    {
        public virtual bool NeverSkip
        { protected set { } get { return false; } }

        public virtual bool SkipFrame { get; set; }

        public virtual void OnEnd(ResponseObservation observation, Result result)
        {
        }

        public virtual IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            return new List<Action>();
        }

        public virtual void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentId)
        {
        }
    }
}