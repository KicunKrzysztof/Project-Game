﻿using CommunicationLibrary;
using CommunicationLibrary.Request;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using FibonacciHeap;
using Agent.AgentBoard;
using Agent.MessageHandling;
using CommunicationLibrary.Response;
using CommunicationLibrary.Information;
using Agent.Strategies;
using Serilog;

namespace Agent
{
    public class Agent : IDisposable
    {
        public AgentConfiguration _configuration { get; set; }
        private SenderReceiverQueueAdapter _communicator;
        private TcpClient _client;
        public AgentInfo agentInfo;

        public Agent(AgentConfiguration configuration)
        {
            this._configuration = configuration;
            _client = new TcpClient(_configuration.CsIp, _configuration.CsPort);
            NetworkStream stream = _client.GetStream();
            this._communicator = new SenderReceiverQueueAdapter(new StreamMessageSenderReceiver(stream, new Parser()));
        }

        public void StartListening()
        {
            if (TryJoinGame())
            {
                MessageHandler m = new MessageHandler(_communicator, agentInfo);
                m.HandleMessages();
            }
        }
        public bool TryJoinGame()
        {
            //not tested

            Message joinGameRequest = new Message<JoinGameRequest>() { MessagePayload = new JoinGameRequest { TeamId = _configuration.TeamId } };
            _communicator.Send(joinGameRequest);

            Log.Debug("Sending join game request: {@Request}", joinGameRequest);
            Message m = _communicator.Take();
            if (m.MessageId != MessageType.JoinGameResponse)
            {
                Log.Error("No responce for join game request");
                return false;
            }
            var joinGameResponse = (JoinGameResponse)m.GetPayload();
            if (!(joinGameResponse.Accepted ?? false))
            {
                Log.Information("Join game request declined");
                return false;
            }
            m = _communicator.Take();
            if (m.MessageId != MessageType.GameStarted)
            {
                Log.Error("No information about starting game");
                return false;
            }
            var gameStarted = (GameStarted)m.GetPayload();
            SetAgentInfo(gameStarted);
            Log.Information("GAME STARTED");
            return true;
        }

        public void SetAgentInfo(GameStarted gameInfo)
        {
            var strategy = new StrategyHandler(gameInfo.BoardSize.X.Value, gameInfo.BoardSize.Y.Value).GetStrategy(_configuration.Strategy);
            this.agentInfo = new AgentInfo(strategy, gameInfo);
        }

        public void Dispose()
        {
            _client.Dispose();
            _communicator.Dispose();
        }
    }
}
