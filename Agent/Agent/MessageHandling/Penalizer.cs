﻿using CommunicationLibrary;
using CommunicationLibrary.Error;
using CommunicationLibrary.Model;
using CommunicationLibrary.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent.MessageHandling
{
    public class Penalizer
    {
        Dictionary<MessageType, int> _responsePenalties = new Dictionary<MessageType, int>(); //in miliseconds
        int _exchangePenalty;
        private bool _unblockTimeUnknown = true;
        private DateTime _blockedUntil;
        public bool UnderPenalty => _unblockTimeUnknown || DateTime.Now < _blockedUntil;
        public Penalizer(Penalties penalties)
        {
            ParsePenalties(penalties);
        }
        private void ParsePenalties(Penalties penalties)
        {
            ParsePenalty(MessageType.CheckHoldedPieceResponse, penalties.CheckForSham);
            ParsePenalty(MessageType.DestroyPieceResponse, penalties.DestroyPiece);
            ParsePenalty(MessageType.DiscoveryResponse, penalties.Discovery);
            ParsePenalty(MessageType.MoveResponse, penalties.Move);
            ParsePenalty(MessageType.PutPieceResponse, penalties.PutPiece);
            //TODO:
            //Temporary, because currently there is no PickPiece penalty
            ParsePenalty(MessageType.PickPieceResponse, penalties.DestroyPiece);
            _exchangePenalty = Int32.Parse(penalties.InformationExchange);
        }
        private void ParsePenalty(MessageType type, string penaltyString)
        {
            var penaltyValue = Int32.Parse(penaltyString);
            _responsePenalties.Add(type, penaltyValue);
        }
        public void PenalizeOnReceive(Message receivedMessage)
        {
            DateTime newBlockedUntil = DateTime.MinValue;

            if (_responsePenalties.ContainsKey(receivedMessage.MessageId))
                newBlockedUntil = DateTime.Now.AddMilliseconds(_responsePenalties[receivedMessage.MessageId]);
            else if (receivedMessage.MessageId == MessageType.PenaltyNotWaitedError)
                newBlockedUntil = ((PenaltyNotWaitedError)receivedMessage.GetPayload()).WaitUntill;

            if(DateTime.Compare(_blockedUntil, newBlockedUntil) < 0)
            {
                _blockedUntil = newBlockedUntil;
                _unblockTimeUnknown = false;
            }
        }

        public void PenalizeOnSend(Message sentMessage)
        {
            //assumes that if message is being sent then there is no penalty
            if (sentMessage.MessageId == MessageType.ExchangeInformationRequest
                || sentMessage.MessageId == MessageType.ExchangeInformationResponse)
            {
                _blockedUntil = DateTime.Now.AddMilliseconds(_exchangePenalty);
                _unblockTimeUnknown = false;
            }
            else
            {
                _unblockTimeUnknown = true;
            }
        }

        public void ClearPenalty()
        {
            _unblockTimeUnknown = false;
            _blockedUntil = DateTime.MinValue;
        }
    }
}
