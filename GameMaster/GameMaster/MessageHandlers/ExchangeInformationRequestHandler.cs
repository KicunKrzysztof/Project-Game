﻿using CommunicationLibrary;
using GameMaster.Configuration;
using GameMaster.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMaster.MessageHandlers
{
    public class ExchangeInformationRequestHandler : MessageHandler
    {
        protected override void CheckAgentPenaltyIfNeeded(Map map)
        {
            CheckIfAgentHasPenalty(map);
        }
        protected override bool CheckRequest(Map map)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Map map)
        {
            throw new NotImplementedException();
        }

        protected override Message GetResponse(Map map)
        {
            throw new NotImplementedException();
        }

        protected override void ReadMessage(MessagePayload payload)
        {
            throw new NotImplementedException();
        }

        protected override void SetTimeout(GMConfiguration config, Map map)
        {
            throw new NotImplementedException();
        }
    }
}